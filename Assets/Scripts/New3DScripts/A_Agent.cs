using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Agent : MonoBehaviour
{
    List<Vector3> path = new List<Vector3>();
    Pathfinder pathFinder;

    public float moveSpeed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        pathFinder = GameObject.FindGameObjectWithTag("PathFinder").GetComponent<Pathfinder>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(path.Count);
        if(path.Count < 1)
        {
            float x = Random.Range(Pathfinder.centrePos.x - (Pathfinder.gridSize / 2.0f), Pathfinder.centrePos.x + (Pathfinder.gridSize / 2.0f));
            float y = Random.Range(Pathfinder.centrePos.y - (Pathfinder.gridSize / 2.0f), Pathfinder.centrePos.y + (Pathfinder.gridSize / 2.0f));
            float z = Random.Range(Pathfinder.centrePos.z - (Pathfinder.gridSize / 2.0f), Pathfinder.centrePos.z + (Pathfinder.gridSize / 2.0f));

            Vector3 target = new Vector3(x, y, z);
            pathFinder.requestPath(transform.position, target);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, path[path.Count], moveSpeed * Time.deltaTime);

            if(Vector3.Distance(transform.position, path[path.Count]) < 0.1f)
            {
                path.RemoveAt(path.Count);
            }
        }
    }
}
