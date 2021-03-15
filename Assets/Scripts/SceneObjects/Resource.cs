using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public Value value;
    public float moveSpeed = 0.5f;

    Vector3 target;

	private bool canMove = true;

	private Shake shake;

    // Start is called before the first frame update
    void Start()
    {
		shake = GetComponent<Shake>();
        target = new Vector3(transform.position.x, transform.position.y, -1.0f);

        //add some slight variation to the value - not sure if this is useful..
        Value minValue = value - (value * (10.0f / 100.0f));
        Value maxValue = value + (value * (10.0f / 100.0f));
		value = Value.RandomRange(minValue, maxValue);
    }

    // Update is called once per frame
    void Update()
    {
		if(canMove)
			transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (transform.position.z < .0f)
            Destroy(gameObject);
    }

	public void consume()
	{
		shake.EnableShake();
		//this is where an agent can consume a resource. Resources can have animations here.
		canMove = false;
	}
}
