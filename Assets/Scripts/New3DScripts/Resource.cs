using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public float value;
    public float moveSpeed = 0.5f;
    public bool assignedToAgent = false;

    Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        target = new Vector3(transform.position.x, transform.position.y, -1.0f);

        //add some slight variation to the value - not sure if this is useful..
        float minValue = value - ((10.0f / 100.0f) * value);
        float maxValue = value + ((10.0f / 100.0f) * value);
        value = Random.Range(minValue, maxValue);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (transform.position.z < .0f)
            Destroy(gameObject);
    }
}
