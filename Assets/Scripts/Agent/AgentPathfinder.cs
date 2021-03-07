using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: change this name to AgentPathfinding
public class AgentPathfinder : MonoBehaviour
{
    List<Vector3> path = new List<Vector3>();
    Pathfinder pathFinder;

    public float moveSpeed = 1.0f;
	public float rotationSpeed = 0.1f;

    //How often should the path information be reset (in seconds)
    private float pathUpdateStep = 3.0f;

	private float actionTime;

	[System.NonSerialized]
	public bool hasPath = false;

	private bool pathTraversalEnabled = true;

	AgentStats stats;
	Vector3 oldPosition;

	/// Orbit variables
	private float orbitRadius = 5.0f;
	private Vector3 axis = Vector3.up;
	private float orbitRotationSpeed = 10.0f;
	private float radiusCorrectionSpeed = 0.5f;
	private Vector3 previousPos;

	private GameObject orbitTarget;

	// Start is called before the first frame update
	void Awake()
    {
        pathFinder = GetComponent<Pathfinder>();
		actionTime = pathUpdateStep;
		stats = GetComponent<AgentStats>();
    }

	private void Start()
	{
		oldPosition = transform.position;
	}

	// Update is called once per frame
	void Update()
    {
        actionTime += Time.deltaTime;
		
		if (path.Count != 0)
        {
			if (pathTraversalEnabled)
			{
				//transform.position = Vector3.MoveTowards(transform.position, path[0], moveSpeed * Time.deltaTime);
				transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, moveSpeed * Time.deltaTime);
				stats.distanceTravelled += Vector3.Distance(transform.position, oldPosition);
				oldPosition = transform.position;

				//transform.LookAt(path[0]);

				//smoother rotation, so the units don't jerk towards their target.
				float rotationStep = rotationSpeed * Time.deltaTime;
				Vector3 moveDirection = path[0] - transform.position;
				Vector3 newDirection = Vector3.RotateTowards(transform.forward, moveDirection, rotationStep, 0.0f);
				transform.rotation = Quaternion.LookRotation(newDirection);

				Debug.DrawRay(transform.position, path[0] - transform.position, Color.red);
				Debug.DrawRay(transform.position, transform.forward, Color.green);
				Debug.DrawRay(transform.position, newDirection, Color.blue);

				if (Vector3.Distance(transform.position, path[0]) < 0.05f)
				{
					path.RemoveAt(0);

					//if a raycast in the direction of the next path node hits nothing, skip this path node
					while (!Physics.Raycast(transform.position, path[1] - transform.position, 1000.0f))
					{
						Debug.Log("Skipping a step!");
						path.RemoveAt(0);
						if (path.Count <= 1)
							break;
					}
				}
			}
		}
		else
		{
			hasPath = false;
		}


		if (orbitTarget && !pathTraversalEnabled)
		{
			OrbitTarget();
		}
	}

	public void SetPath(Vector3 target)
	{
		//do a ray-cast first, this is more optimal if possible.
		if (!Physics.Raycast(transform.position, target - transform.position, out RaycastHit hit))
		{
			Debug.DrawRay(transform.position, target - transform.position);
			// target is in direct vision
			if (!hit.collider || true)
			{
				//ray-cast hit nothing... so it has a straight path to a target with no gameobject.
				actionTime = pathUpdateStep;
				SetSingleNodePath(target);
				hasPath = true;
				return;
			}
		}

		//only update the path ever pathUpdateStep seconds.
		if (actionTime < pathUpdateStep)
		{
			return;
		}

		// raycast failed, do 3d A*.
		NullifyPath();

		//this kicks off a thread to calculate a path.
		pathFinder.requestPath(transform.position, target, this);


		if (path == null)
		{
			Debug.Log("Slow thing is happening here, bad!");
			path = new List<Vector3>();
			return;
		}

		actionTime = 0;
	}

    public void SetPath(GameObject target)
    {

		Debug.DrawRay(transform.position, target.transform.position - transform.position);
		//do a ray-cast first, this is more optimal if possible. returns true if hits a collider
		if (Physics.Raycast(transform.position, target.transform.position - transform.position, out RaycastHit hit))
        {

			// target is in direct vision
			if (hit.collider.gameObject == target)
            {
                actionTime = pathUpdateStep;
                SetSingleNodePath(target.transform.position);
				hasPath = true;
                return;
            }

			//ELSE, something else was in the way.

			//only update the path ever pathUpdateStep seconds.
			if (actionTime < pathUpdateStep)
			{
				return;
			}

			// raycast failed, do 3d A*.
			NullifyPath();
			
			//this kicks off a thread to calculate a path.
			pathFinder.requestPath(transform.position, target.transform.position, this);


			if (path == null)
			{
				Debug.Log("Slow thing is happening here, bad!");
				path = new List<Vector3>();
				return;
			}

			actionTime = 0;
        }

	}

	public void SetOrbitTarget(GameObject target)
	{
		orbitTarget = target;

		//need the colliderManager to figure out the size of the object
		colliderManager colliderMgr;
		if ((colliderMgr = target.GetComponent<colliderManager>()) == null)
		{
			colliderMgr = target.GetComponentInChildren<colliderManager>();
		}

		Vector3 targetSize = colliderMgr.size;
		orbitRadius = Mathf.Max(targetSize.x, targetSize.y, targetSize.z);
		orbitRadius *= 1.5f;

		orbitRadius = Mathf.Max(orbitRadius, 2.5f);
	}

	private void OrbitTarget()
	{
		//update radius to be a function of the objects size - can probably re-use collider size calculations


		//Movement
		// change axis here to avoid always using the same orbit axis. this will be necessary when there are multiple agents per base.
		// axis = cross product of forward and backward transform vectors? What does that look like?
		transform.RotateAround(orbitTarget.transform.position, axis, orbitRotationSpeed * Time.deltaTime);
		Vector3 orbitDesiredPosition = (transform.position - orbitTarget.transform.position).normalized * orbitRadius + orbitTarget.transform.position;
		transform.position = Vector3.Slerp(transform.position, orbitDesiredPosition, Time.deltaTime * radiusCorrectionSpeed);

		//Rotation
		Vector3 relativePos = transform.position - previousPos;
		Quaternion rotation = Quaternion.LookRotation(relativePos);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, radiusCorrectionSpeed * Time.deltaTime);
		previousPos = transform.position;
	}

	public void AllowPathTraversal(bool b)
	{
		pathTraversalEnabled = b;
	}

	public void NotifyPathAvailable(List<Vector3> p)
	{
		path = p;
		hasPath = true;
	}

	public void SetSingleNodePath(Vector3 target)
    {
        //This is really innefficient. Don't need to be remaking and nullifying the Vector array for this.
        NullifyPath();
        path.Add(target);

    }

    public void NullifyPath()
    {
        if(path.Count != 0)
            path.Clear();
    }

    public void OrderImmediateMovement()
    {
        actionTime = pathUpdateStep;
    }
}
