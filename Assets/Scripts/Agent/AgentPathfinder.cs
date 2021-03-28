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
	public float orbitRotationSpeed = 10.0f;

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
	private float radiusCorrectionSpeed = 2.0f;
	private Vector3 previousPos;


	private GameObject orbitDynamicTarget;
	private Vector3 orbitStaticTarget;
	private bool orbitStatic = true;

	private LayerMask targetLayerMask;
	float maxRayDistance;
	// Start is called before the first frame update
	void Awake()
    {
        pathFinder = GetComponent<Pathfinder>();
		actionTime = pathUpdateStep;
		stats = GetComponent<AgentStats>();
    }

	private void Start()
	{
		//agent rays cant collide with other agents.
		targetLayerMask =~ LayerMask.GetMask("Agent");

		//max ray distance is based on the size of the map. Plus offset because safety.
		maxRayDistance = GameManager.Instance.maxMapDistance + 10.0f;

		oldPosition = transform.position;
	}

	// Update is called once per frame
	void Update()
    {
        actionTime += Time.deltaTime;

		Debug.Log("New Frame, no. : " + Time.frameCount);

		if (path.Count != 0)
        {
			Debug.Log("Path Count not 0: " + path.Count);
			if (pathTraversalEnabled)
			{
				Debug.Log("Path Traversal enabled");
				//Debug.Log("Path Traversal enabled!! Path Length: " + path.Count);

				CustomMoveTowards(path[0]);

				if (Vector3.Distance(transform.position, path[0]) < 0.05f)
				{
					Debug.Log("Next Path");
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

		//orbitDynamicTarget is always set, even if the target is static.
		if (orbitDynamicTarget && !pathTraversalEnabled)
		{
			OrbitTarget();
		}
	}

	public void SetPath(Vector3 target)
	{
		//do a ray-cast first, this is more optimal if possible.
		if (!Physics.Raycast(transform.position, target - transform.position, out RaycastHit hit, maxRayDistance, targetLayerMask))
		{
			Debug.DrawRay(transform.position, target - transform.position);
			// target is in direct vision
			if (!hit.collider || true)
			{
				//ray-cast hit nothing... so it has a straight path to a target with no gameobject.
				actionTime = pathUpdateStep;
				Debug.Log("Set single node path 2");
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
		Debug.Log("Set path Nullify 2");
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
		Debug.Log("Set path 1");
		Debug.DrawRay(transform.position, target.transform.position - transform.position);
		//do a ray-cast first, this is more optimal if possible. returns true if hits a collider
		if (Physics.Raycast(transform.position, target.transform.position - transform.position, out RaycastHit hit, maxRayDistance , targetLayerMask))
        {
			Debug.Log("Raycast passed");
			// target is in direct vision
			if (hit.collider.gameObject == target)
            {
                actionTime = pathUpdateStep;
				Debug.Log("Set single node path 1, hit target: " + target.name);
				SetSingleNodePath(target.transform.position);
				hasPath = true;
                return;
            }

			//ELSE, something else was in the way.
			Debug.Log("Set path Nullify 1, missed target: " + target.name + " hit: " + hit.collider.gameObject.name);

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
			return;
        }
		Debug.Log("Raycast failed");

	}

	public void SetOrbitTarget(GameObject target, bool isStatic = true)
	{
		orbitDynamicTarget = target;

		//need the colliderManager to figure out the size of the object
		colliderManager colliderMgr;
		if ((colliderMgr = target.GetComponent<colliderManager>()) == null)
		{
			colliderMgr = target.GetComponentInChildren<colliderManager>();
		}

		Vector3 targetSize = colliderMgr.size;
		orbitRadius = Mathf.Max(targetSize.x, targetSize.y, targetSize.z);
		orbitRadius *= 1.25f;

		orbitRadius = Mathf.Max(orbitRadius, 2.5f);

		orbitStatic = isStatic;

		if(isStatic)
		{
			orbitStaticTarget = orbitDynamicTarget.transform.position;
		}
	}

	private void OrbitTarget()
	{
		//update radius to be a function of the objects size - can probably re-use collider size calculations

		Vector3 orbitTarget = orbitStatic ? orbitStaticTarget : orbitDynamicTarget.transform.position;

		//Movement
		// change axis here to avoid always using the same orbit axis. this will be necessary when there are multiple agents per base.
		// axis = cross product of forward and backward transform vectors? What does that look like?
		Vector3 orbitPos = GetRotateAroundPos(orbitTarget, axis, orbitRotationSpeed * Time.deltaTime);

		CustomMoveTowards(orbitPos);

		//previousPos = transform.position;

		Debug.Log("Orbiting!!!");
	}

	private void CustomMoveTowards(Vector3 target)
	{
		Debug.Log("Custom Move Towards");
		//transform.position = Vector3.MoveTowards(transform.position, path[0], moveSpeed * Time.deltaTime);
		transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, moveSpeed * Time.deltaTime);
		stats.distanceTravelled += Vector3.Distance(transform.position, oldPosition);
		oldPosition = transform.position;

		//smoother rotation, so the units don't jerk towards their target.
		float rotationStep = rotationSpeed * Time.deltaTime;
		Vector3 moveDirection = target - transform.position;
		Vector3 newDirection = Vector3.RotateTowards(transform.forward, moveDirection, rotationStep, 0.0f);
		transform.rotation = Quaternion.LookRotation(newDirection);
	}

	private Vector3 GetRotateAroundPos(Vector3 center, Vector3 axis, float angle)
	{
		Vector3 ret;
		Vector3 pos = transform.position;
		Quaternion rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
		Vector3 dir = pos - center; // find current direction relative to center
		dir = rot * dir; // rotate the direction

		ret = center + dir;
		return ret;
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
		Debug.Log("Setting Single Node Path");
		//This is really innefficient. Don't need to be remaking and nullifying the Vector array for this.
		NullifyPath();

        path.Add(target);
    }

    public void NullifyPath()
    {
		Debug.Log("Nullifying Path");
		if (path.Count != 0)
            path.Clear();
    }

    public void OrderImmediateMovement()
    {
        actionTime = pathUpdateStep;
    }
}
