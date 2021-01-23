using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: change this name to AgentPathfinding
public class AgentPathfinder : MonoBehaviour
{
    List<Vector3> path = new List<Vector3>();
    Pathfinder pathFinder;

    public float moveSpeed = 1.0f;

    //How often should the path information be reset (in seconds)
    private float pathUpdateStep = 3.0f;

	private float actionTime;

	[System.NonSerialized]
	public bool hasPath = false;

	private bool pathTraversalEnabled = true;

	AgentStats stats;
	Vector3 oldPosition;

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
				transform.position = Vector3.MoveTowards(transform.position, path[0], moveSpeed * Time.deltaTime);
				stats.distanceTravelled += Vector3.Distance(transform.position, oldPosition);
				transform.LookAt(path[0]);

				if (Vector3.Distance(transform.position, path[0]) < 0.05f)
				{
					path.RemoveAt(0);
				}
			}
        }
		else
		{
			hasPath = false;
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

    public void orderImmediateMovement()
    {
        actionTime = pathUpdateStep;
    }
}
