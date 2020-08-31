using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public float spawnRate;

    //input as a percentage (/100)
    public float spawnRateVariation;

    //prefab for Resource
    public List<GameObject> resources = new List<GameObject>();

	public BuildManager buildManager;

    int baseTicker = 0;

    float spawnTimer = .0f;
    float nextSpawnTime = .0f;

    float minSpawnTime, maxSpawnTime;
        
    // Start is called before the first frame update
    void Start()
    {
        minSpawnTime = spawnRate - ((spawnRateVariation/100.0f)*spawnRate);
        maxSpawnTime = spawnRate + ((spawnRateVariation/100.0f)*spawnRate);


        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;

        if(spawnTimer >= nextSpawnTime)
        {
            Spawn();
        }

    }


    void Spawn()
    {
		int numBases = buildManager.allBases.Count;

		if (numBases < 1)
			return;

		spawnTimer = .0f;
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);

        //resources will move across the Z axis (constant X and Y from start)
        Vector3 spawnPos = new Vector3(
            Random.Range(.0f, (float)GameManager.Instance.mapX),
            Random.Range(.0f, (float)GameManager.Instance.mapY), 
            (float)GameManager.Instance.mapZ);

        //don't currently track the resource
        Resource r = Instantiate(resources[Random.Range(0, resources.Count)], spawnPos, Quaternion.identity).GetComponent<Resource>();

		//rotationally grant resources
		buildManager.allBases[baseTicker].grantResource(r);
        baseTicker++;

        if (baseTicker >= buildManager.allBases.Count)
            baseTicker = 0;
    }
}
