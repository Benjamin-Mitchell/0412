using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldInteractable : MonoBehaviour {

    public Vector3[] freeSpots = new Vector3[4];

    public bool[] spotsAvailable = new bool[4] { true, true, true, true };

    //default value in seconds
    public float timeRequired = 5.0f;

    //default point value
    public int pointWorth = 5;


	// Use this for initialization
	void Start () {
        Vector2 factor = new Vector2(transform.localScale.x * 1.5f, transform.localScale.z * 1.5f);

        freeSpots[0] = new Vector3(transform.position.x + factor.x, transform.position.y, transform.position.z);
        freeSpots[1] = new Vector3(transform.position.x - factor.x, transform.position.y, transform.position.z);
        freeSpots[2] = new Vector3(transform.position.x, transform.position.y, transform.position.z + factor.y);
        freeSpots[3] = new Vector3(transform.position.x, transform.position.y, transform.position.z - factor.y);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
