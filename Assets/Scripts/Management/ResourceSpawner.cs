using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
	//Rough number of seconds between spawns (Uses some variation)
    public float spawnRate;

	//percentage (/100) chance for each resource to spawn.
	public float[] resourceSpawnRates;

	//input as a percentage (/100)
	public float spawnRateVariation;

	public float resourceSpawnRateUpgrade = 1.0f;

	//prefab for Resource
	public List<GameObject> resources = new List<GameObject>();

	public BuildManager buildManager;

    float spawnTimer = .0f;
    float nextSpawnTime = .0f;

    float minSpawnTime, maxSpawnTime;

	private List<Resource> availableResources = new List<Resource>();

	// Start is called before the first frame update
	void Start()
    {
		minSpawnTime = spawnRate - ((spawnRateVariation/100.0f)*spawnRate);
        maxSpawnTime = spawnRate + ((spawnRateVariation/100.0f)*spawnRate);


        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime) / resourceSpawnRateUpgrade;
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

	public void CalculateSpawnRates(int maxBaseTier)
	{
		if (maxBaseTier == 1)
			resourceSpawnRates = new float[1] { 1.0f };
		else if (maxBaseTier == 2)
			resourceSpawnRates = new float[2] { 0.6f, 0.4f };
		else if (maxBaseTier == 3)
			resourceSpawnRates = new float[3] { 0.5f, 0.3f, 0.2f};
		else if (maxBaseTier == 4)
			resourceSpawnRates = new float[4] { 0.4f, 0.3f, 0.2f, 0.1f };
		else if (maxBaseTier == 5)
			resourceSpawnRates = new float[5] { 0.3f, 0.25f, 0.2f, 0.15f, 0.1f };
	}

	private int GetResourceSpawnIndex()
	{
		float dice = Random.Range(.0f, 1.0f);
		float i = 0.01f;
		int res = -1;
		do
		{
			res++;
			i += resourceSpawnRates[res];
		} while (i < dice);

		return res;
	}

    void Spawn()
    {
		int numBases = buildManager.allBases.Count;

		if (numBases < 1)
			return;
		
		spawnTimer = .0f;
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime) / resourceSpawnRateUpgrade;

        //resources will move across the Z axis (constant X and Y from start)
        Vector3 spawnPos = new Vector3(
            Random.Range(.0f, (float)GameManager.Instance.mapX),
            Random.Range(.0f, (float)GameManager.Instance.mapY), 
            (float)GameManager.Instance.mapZ);
		
        Resource r = Instantiate(resources[GetResourceSpawnIndex()], spawnPos, Quaternion.identity).GetComponent<Resource>();

		availableResources.Add(r);
    }

	//return first available resource.
	public Resource RequestResource(int maxTier)
	{
		Resource r;

		if (availableResources.Count == 0)
			return null;

		for (int i = 0; i < availableResources.Count; i++)
		{
			if(availableResources[i].tier <= maxTier)
			{
				r = availableResources[i];
				availableResources.RemoveAt(i);
				return r;
			}
		}
		return null;
	}

	//Return resource closest to the requester.
	public Resource RequestResource(int maxTier, Vector3 pos)
	{
		Resource r;

		if (availableResources.Count == 0)
			return null;

		bool found = false;

		float minDistance = float.MaxValue;
		int minDistIndex = 0;

		for (int i = 0; i < availableResources.Count; i++)
		{
			float d = Vector3.Distance(availableResources[i].transform.position, pos);
			if (d < minDistance && availableResources[i].tier <= maxTier)
			{
				found = true;
				minDistance = d;
				minDistIndex = i;
			}
		}

		if (!found)
			return null;

		r = availableResources[minDistIndex];
		availableResources.RemoveAt(minDistIndex);
		return r;
	}
}
