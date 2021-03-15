using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
	//maximum amount it can move in each direction (positive and negative)
	public Vector3 amplitude;

	public bool PermanentShake = false;

	//frequency - number of times it moves per second.
	public float frequency;

	private bool tempShake = false;

	private Vector3 currentMovement = Vector3.zero;
	private float timeToWait;
	private float timePassed;

	Vector3 target;
	private float moveSpeed;
	Vector3 initialPos;
	Vector3 prevPos;

    // Start is called before the first frame update
    void Start()
    {
		initialPos = transform.position;
		prevPos = transform.position;
		timeToWait = 1.0f / frequency;

		//static moveSpeed
		moveSpeed = Mathf.Max(amplitude.x, amplitude.y, amplitude.z) * timeToWait;
    }

    // Update is called once per frame
    void Update()
    {
		if (!PermanentShake && !tempShake)
			return;

		timePassed += Time.deltaTime;

		transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed);

		currentMovement += transform.position - prevPos;

		prevPos = transform.position;

		if (timePassed > timeToWait)
		{
			timePassed = 0.0f;
			DoShake();
		}

		if(transform.position.x > (initialPos.x + amplitude.x) || 
			transform.position.x < (initialPos.x - amplitude.x) ||
			transform.position.y > (initialPos.y + amplitude.y) ||
			transform.position.y < (initialPos.y - amplitude.y) ||
			transform.position.z > (initialPos.z + amplitude.z) ||
			transform.position.z < (initialPos.z - amplitude.z)
			)
		{
			Debug.Log("Strayed too far!!");
		}
    }

	void DoShake()
	{
		float MaxXNeg = -amplitude.x - currentMovement.x;
		float MaxXPos = amplitude.x - currentMovement.x;

		float MaxYNeg = -amplitude.y - currentMovement.y;
		float MaxYPos = amplitude.y - currentMovement.y;

		float MaxZNeg = -amplitude.z - currentMovement.z;
		float MaxZPos = amplitude.z - currentMovement.z;

		Vector3 moveVector = new Vector3(
			Random.Range(MaxXNeg, MaxXPos),
			Random.Range(MaxYNeg, MaxYPos),
			Random.Range(MaxZNeg, MaxZPos));

		target = new Vector3(
			transform.position.x + moveVector.x,
			transform.position.y + moveVector.y,
			transform.position.z + moveVector.z);


		//currentMovement += moveVector;

		//dynamically changing move speed
		moveSpeed = Mathf.Max(Mathf.Abs(moveVector.x), Mathf.Abs(moveVector.y), Mathf.Abs(moveVector.z)) * timeToWait;
		//Debug.Log("move vector: " + moveVector);
		//transform.position.
	}

	public void EnableShakeForTime(float time)
	{
		StartCoroutine(EnableShakePeriod(time));
	}

	private IEnumerator EnableShakePeriod(float time)
	{
		tempShake = true;
		yield return new WaitForSeconds(time);
		tempShake = false;
	}

	public void EnableShake()
	{
		tempShake = true;
	}

	public void DisableShake()
	{
		tempShake = false;
	}
}
