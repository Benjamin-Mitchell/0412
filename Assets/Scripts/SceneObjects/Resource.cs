using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public Value value;
    public float moveSpeed = 0.5f;

	public float initialScale = 0.0f;

    public int tier; //0: dirty, 1: pie, 2: proxy, 3: Seller, 4:Teabag

    Vector3 target;

	private bool canMove = true;

	private Shake shake;

	private float finalScale = 0.0f;

	// Start is called before the first frame update
	private void Awake()
	{
		transform.localScale = new Vector3(initialScale, initialScale, initialScale);
		transform.eulerAngles = new Vector3(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f));
		finalScale = Random.Range(0.6f, 1.6f);
	}
	void Start()
    {
		//shake = GetComponent<Shake>();
        target = new Vector3(transform.position.x, transform.position.y, -1.0f);

        //add some slight variation to the value - not sure if this is useful..
        Value minValue = value - (value * (10.0f / 100.0f));
        Value maxValue = value + (value * (10.0f / 100.0f));
		value = Value.RandomRange(minValue, maxValue);
    }

    // Update is called once per frame
    void Update()
    {
		if (initialScale < (finalScale))
		{
			initialScale += (Time.deltaTime / 5.0f);
			transform.localScale = new Vector3(initialScale, initialScale, initialScale);
		}


		if(canMove)
			transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

	public void consume()
	{
		//shake.EnableShake();
		//this is where an agent can consume a resource. Resources can have animations here.
		canMove = false;
	}
}
