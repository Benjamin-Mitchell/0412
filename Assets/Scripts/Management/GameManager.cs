using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public int mapX = 10;
    public int mapY = 10;
    public int mapZ = 10;

    public bool finishedIntroduction = false;

	private float units = 10000000;

	private float currentSpawnRateIncrease = 0.4f;
	private float resourceSpawnRate = 1.0f;
	public float spawnRateIncreaseCost = 600.0f;
	
	private float resourceValueMultiplier = 1.0f;
	public float resourceValueIncreaseCost = 800.0f;

	private float unitReturnRate = 0.5f;
	public float unitReturnIncreaseCost = 1000.0f;

	[SerializeField]
	private Text unitsText;

	void Awake()
    {
        //this is where loading should happen
    }

    // Start is called before the first frame update
    void Start()
    {
		unitsText.text = units + " U";
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void AddUnits(float u)
	{
		units += (u * unitReturnRate);
		unitsText.text = (int)units + " U";
	}

	public bool IncrementSpawnRate()
	{
		if (units > spawnRateIncreaseCost)
			units -= spawnRateIncreaseCost;
		else
			return false;

		unitsText.text = units + " U";

		spawnRateIncreaseCost = Mathf.Pow(spawnRateIncreaseCost, 2) * 2;
		resourceSpawnRate *= (1.0f + currentSpawnRateIncrease);

		//has diminishing returns to avoid insane numbers of resources
		currentSpawnRateIncrease *= 0.9f;
		return true;
	}

	public bool IncrementValueMultiplier()
	{
		if (units > resourceValueIncreaseCost)
			units -= resourceValueIncreaseCost;
		else
			return false;

		unitsText.text = units + " U";

		resourceValueIncreaseCost = Mathf.Pow(resourceValueIncreaseCost, 2) * 4;

		resourceValueMultiplier *= 2.0f;
		return true;
	}

	public bool IncrementUnitReturn()
	{
		if (units > unitReturnIncreaseCost)
			units -= unitReturnIncreaseCost;
		else
			return false;

		unitsText.text = units + " U";

		unitReturnIncreaseCost *= 100.0f;

		//this can go over 1.0f, giving more units than resources earned.
		unitReturnRate += 0.1f;
		return true;
	}

	public float GetResourceSpawnRate()
	{
		return resourceSpawnRate;
	}

	public float GetResourceValueMultiplier()
	{
		return resourceValueMultiplier;
	}

	public float GetUnitReturnRate()
	{
		return unitReturnRate;
	}

}
