﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
	//Rough number of seconds between spawns (Uses some variation)
    public float spawnRate;

    //input as a percentage (/100)
    public float spawnRateVariation;

    //prefab for Resource
    public List<GameObject> resources = new List<GameObject>();

	public BuildManager buildManager;

    float spawnTimer = .0f;
    float nextSpawnTime = .0f;

    float minSpawnTime, maxSpawnTime;

	private List<Resource> availableResources = new List<Resource>();

	private GameManager gameManager;

	// Start is called before the first frame update
	void Start()
    {
		gameManager = GameManager.Instance;

		minSpawnTime = spawnRate - ((spawnRateVariation/100.0f)*spawnRate);
        maxSpawnTime = spawnRate + ((spawnRateVariation/100.0f)*spawnRate);


        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime) / gameManager.resourceSpawnRate;
	}

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;

        if(spawnTimer >= nextSpawnTime)
        {
            Spawn();
        }

		for(int i = availableResources.Count - 1; i >= 0; i--)
		{
			if (availableResources[i].transform.position.x < 0.0f || availableResources[i].transform.position.y < 0.0f || availableResources[i].transform.position.z < 0.0f)
			{
				//need to remove it from the list, then destroy it
				GameObject temp = availableResources[i].gameObject;
				availableResources.RemoveAt(i);
				Destroy(temp);
			}
		}
    }

    void Spawn()
    {
		int numBases = buildManager.allBases.Count;

		if (numBases < 1)
			return;
		
		spawnTimer = .0f;
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime) / gameManager.resourceSpawnRate;

        //resources will move across the Z axis (constant X and Y from start)
        Vector3 spawnPos = new Vector3(
            Random.Range(.0f, (float)GameManager.Instance.mapX),
            Random.Range(.0f, (float)GameManager.Instance.mapY), 
            (float)GameManager.Instance.mapZ);
		
        Resource r = Instantiate(resources[Random.Range(0, resources.Count)], spawnPos, Quaternion.identity).GetComponent<Resource>();

		availableResources.Add(r);
    }

	//return first available resource.
	public Resource RequestResource()
	{
		Resource r;

		if (availableResources.Count == 0)
			return null;
		
		r = availableResources[0];
		availableResources.RemoveAt(0);

		return r;
	}

	//Return resource closest to the requester.
	public Resource RequestResource(Vector3 pos)
	{

		Resource r;

		if (availableResources.Count == 0)
			return null;

		float minDistance = Vector3.Distance(availableResources[0].transform.position, pos);
		int minDistIndex = 0;

		for (int i = 1; i < availableResources.Count; i++)
		{
			float d = Vector3.Distance(availableResources[i].transform.position, pos);
			if (d < minDistance)
			{
				minDistance = d;
				minDistIndex = i;
			}
		}

		r = availableResources[minDistIndex];
		availableResources.RemoveAt(minDistIndex);
		return r;
	}
}
