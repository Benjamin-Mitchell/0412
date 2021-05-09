using System.Collections;
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

	public Resource RequestResource()
	{
		Resource r;

		if (availableResources.Count == 0)
			return null;
		
		//TODO: make this return nearest resource instead of "first"? or some other smart allocation...
		r = availableResources[0];
		availableResources.RemoveAt(0);

		return r;
	}
}
