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
    float pathUpdateStep = 3.0f;

	private float actionTime;

    // Start is called before the first frame update
    void Awake()
    {
        pathFinder = GameObject.FindGameObjectWithTag("PathFinder").GetComponent<Pathfinder>();
		actionTime = pathUpdateStep;
    }

    // Update is called once per frame
    void Update()
    {
        actionTime += Time.deltaTime;

        if (path.Count != 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[0], moveSpeed * Time.deltaTime);
            transform.LookAt(path[0]);

            if(Vector3.Distance(transform.position, path[0]) < 0.05f)
            {
                path.RemoveAt(0);
            }
        }
    }

    public void SetPath(GameObject target)
    {
        //do a ray-cast first, this is more optimal if possible.
        if (Physics.Raycast(transform.position, target.transform.position - transform.position, out RaycastHit hit))
        {
			Debug.DrawRay(transform.position, target.transform.position);
            // target is in direct vision
            if (hit.collider.gameObject == target)
            {
                actionTime = pathUpdateStep;
                SetSingleNodePath(target.transform.position);
                return;
            }
        }

        //continue to update position to follow target
        if (actionTime < pathUpdateStep)
        {
			return;
        }
		float t = Time.time * 1000;

        // raycase failed, do 3d A*.
        NullifyPath();

		float t2 = Time.time * 1000;
		Debug.Log("took " + (t2 - t)  +" milliseconds to nullify path");

        path = pathFinder.requestPath(transform.position, target.transform.position);

		Debug.Log("took " + (t2 - t) + " milliseconds to find new path");
		Debug.Log("took " + ((Time.time * 1000) - t) + " milliseconds total");

		Debug.Log("------------------------------------");
		Debug.Log("------------------------------------");
		Debug.Log("------------------------------------");


		if (path == null)
        {
			Debug.Log("Slow thing is happening here, bad!");
            path = new List<Vector3>();
            return;
        }

		actionTime = 0;
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
