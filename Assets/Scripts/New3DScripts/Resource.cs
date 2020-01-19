using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{

    public float moveSpeed = 1.0f;

    Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        target = new Vector3(transform.position.x, transform.position.y, -1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (transform.position.z < .0f)
            Destroy(gameObject);
    }
}
