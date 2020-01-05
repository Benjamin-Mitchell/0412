using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Agent : MonoBehaviour
{
    List<Vector3> path = new List<Vector3>();
    Pathfinder pathFinder;

    public float moveSpeed = 1.0f;

    //TEST
    Vector3 target1 = new Vector3(3, 1, 1);
    Vector3 target2 = new Vector3(-1, 1, 1);
    bool targetswitch = false;

    // Start is called before the first frame update
    void Start()
    {
        pathFinder = GameObject.FindGameObjectWithTag("PathFinder").GetComponent<Pathfinder>();
    }

    // Update is called once per frame
    void Update()
    {

        
        if(path.Count < 1)
        {
            float x = Random.Range(Pathfinder.centrePos.x - (Pathfinder.gridSize / 2.0f), Pathfinder.centrePos.x + (Pathfinder.gridSize / 2.0f));
            float y = Random.Range(Pathfinder.centrePos.y - (Pathfinder.gridSize / 2.0f), Pathfinder.centrePos.y + (Pathfinder.gridSize / 2.0f));
            float z = Random.Range(Pathfinder.centrePos.z - (Pathfinder.gridSize / 2.0f), Pathfinder.centrePos.z + (Pathfinder.gridSize / 2.0f));
            
            Vector3 target = new Vector3(x, y, z);

            //Vector3 target = targetswitch ? target1 : target2;
            //
            //targetswitch = !targetswitch;
            //Debug.Log("TARGET: " + target);

            path = pathFinder.requestPath(transform.position, target);

            //for(int i = 0; i < path.Count; i++)
            //{
            //    Debug.Log("Path " + i + ": " + path[i]);
            //}
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, path[path.Count - 1], moveSpeed * Time.deltaTime);
            
            if(Vector3.Distance(transform.position, path[path.Count - 1]) < 0.1f)
            {
                path.RemoveAt(path.Count - 1);
            }
        }


    }
}
