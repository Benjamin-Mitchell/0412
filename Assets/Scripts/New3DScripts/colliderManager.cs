using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//manages general access to collider size regardless of what type of collider is attached.

//can extend for usage by mesh collider for more complex grid assembly and pathfinding.

public class colliderManager : MonoBehaviour
{

    public Vector3 size;


    // Start is called before the first frame update
    void Awake()
    {
        BoxCollider boxCol;
        if ((boxCol = GetComponent<BoxCollider>()) != null)
        {
            size = GetComponent<BoxCollider>().size;
            return;
        }

        SphereCollider sphereCol;
        if ((sphereCol = GetComponent<SphereCollider>()) != null)
        {
            float radius = GetComponent<SphereCollider>().radius;
            size = new Vector3(radius/2.0f, radius / 2.0f, radius / 2.0f);
            return;
        }


        //add any further colliders to test here if and when we add them.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
