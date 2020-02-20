using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Test Agent for ray-casting navigation.
// Currently deprecated in favour of A* nav system

public class Ray_Agent : MonoBehaviour
{
    // Bit shift the index of the layer (8/player) to get a bit mask
    int layerMask = 1 << 8;

    public GameObject[] targets;
    int targetIndex = 0;
    int maxTargetIndex = 0;

    int rayCastDistance = 1000;

    float rayDivergance = 10.0f;

    float moveSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        //invert bitmask (to avoid player layer)
        layerMask = ~layerMask;

        maxTargetIndex = targets.Length;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetpos = targets[targetIndex].transform.position;
        if (Vector3.Distance(transform.position, targetpos) < 0.5f)
        {
            targetIndex = (targetIndex + 1) % maxTargetIndex;
        }
        else
        {
            //"pathfind".
            RaycastHit hit;
            Vector3 transformForward = transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(transform.position, transformForward, out hit, rayCastDistance, layerMask))
            {
                Debug.DrawRay(transform.position, transformForward * hit.distance, Color.red);

                //array or ray hits - up, down, left, right.
                bool[] hits = new bool[4];
                bool hitSomething = false;
                float rayDiverganceAngle = rayDivergance;
                do
                {
                    hits.Equals(false);
                    hitSomething = false;
                    
                    float divergenceByLocalTransform = rayDiverganceAngle / 90.0f;

                    Vector3 up = transform.TransformDirection(new Vector3(0, -divergenceByLocalTransform, 1));
                    Vector3 down = transform.TransformDirection(new Vector3(0, divergenceByLocalTransform, 1));

                    Vector3 left = transform.TransformDirection(new Vector3(-divergenceByLocalTransform, 0, 1));
                    Vector3 right = transform.TransformDirection(new Vector3(divergenceByLocalTransform, 0, 1));


                    if (Physics.Raycast(transform.position, up, out hit, rayCastDistance, layerMask))
                    {
                        Debug.DrawRay(transform.position, up * hit.distance, Color.red);
                        hits[0] = true;
                        hitSomething = true;
                    }
                    if (Physics.Raycast(transform.position, down, out hit, rayCastDistance, layerMask))
                    {
                        Debug.DrawRay(transform.position, down * hit.distance, Color.red);
                        hits[1] = true;
                        hitSomething = true;
                    }
                    if (Physics.Raycast(transform.position, left, out hit, rayCastDistance, layerMask))
                    {
                        Debug.DrawRay(transform.position, left * hit.distance, Color.red);
                        hits[2] = true;
                        hitSomething = true;
                    }
                    if (Physics.Raycast(transform.position, right, out hit, rayCastDistance, layerMask))
                    {
                        Debug.DrawRay(transform.position, right * hit.distance, Color.red);
                        hits[3] = true;
                        hitSomething = true;
                    }

                    rayDiverganceAngle += rayDivergance;

                    if (rayDiverganceAngle > 100.0f)
                        break;

                } while (hitSomething == true);

                //test which direction to move
                Vector3[] directions = new Vector3[4];
                //directions[0] = left;
                



            }
            else
            {
                // hit nothing
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.green);
            }
        }
    }
}
