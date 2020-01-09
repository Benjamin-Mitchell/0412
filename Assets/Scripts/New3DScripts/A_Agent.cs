using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Agent : MonoBehaviour
{
    List<Vector3> path = new List<Vector3>();
    Pathfinder pathFinder;

    public float moveSpeed = 1.0f;

    //TEST
    Vector3 target1 = new Vector3(2.5f, 2, 5);
    Vector3 target2 = new Vector3(2.5f, 2, 0);
    bool targetswitch = false;

    // Start is called before the first frame update
    void Start()
    {
        pathFinder = GameObject.FindGameObjectWithTag("PathFinder").GetComponent<Pathfinder>();
    }

    // Update is called once per frame
    void Update()
    {

        if(path.Count == 0)
        {
            Debug.Log("CURRENT LOCATION: " + transform.position);

            float x = Random.Range(pathFinder.gridBottomCorner.x, pathFinder.gridBottomCorner.x + Pathfinder.gridSize);
            float y = Random.Range(pathFinder.gridBottomCorner.y, pathFinder.gridBottomCorner.y + Pathfinder.gridSize);
            float z = Random.Range(pathFinder.gridBottomCorner.z, pathFinder.gridBottomCorner.z + Pathfinder.gridSize);
            
            Vector3 target = new Vector3(x, y, z);

            //Vector3 target = targetswitch ? target1 : target2;
            
            //targetswitch = !targetswitch;
            Debug.Log("TARGET: " + target);

            path = pathFinder.requestPath(transform.position, target);

            //null path = couldnt reach destination
            if (path == null)
            {
                path = new List<Vector3>();
                return;
            }

            Debug.Log("PATH LENGTH: " + path.Count);

            for(int i = 0; i < path.Count; i++)
            {
               Debug.Log("Path " + i + ": " + path[i]);
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, path[0], moveSpeed * Time.deltaTime);
            
            if(Vector3.Distance(transform.position, path[0]) < 0.1f)
            {
                path.RemoveAt(0);
            }
        }


    }
}
