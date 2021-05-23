using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public int mapX = 10;
    public int mapY = 10;
    public int mapZ = 10;

	public float maxMapDistance;

    public bool finishedIntroduction = false;

	private Value units = 10000000;

	private float currentSpawnRateIncrease = 0.4f;
	public float resourceSpawnRate = 1.0f;
	public float spawnRateIncreaseCost = 600.0f;
	
	public float resourceValueMultiplier = 1.0f;
	public float resourceValueIncreaseCost = 800.0f;

	public float unitReturnRate = 0.5f;
	public float unitReturnIncreaseCost = 1000.0f;

	//in iterations of 100 seconds
	public float maxPeriodInactive = 72.0f;
	public float maxInactiveIncreaseCost = 2000.0f;

	[SerializeField]
	private Text unitsText;

	public SaveManager saveManager;

	void Awake()
    {
		maxMapDistance = Mathf.Sqrt((mapX * mapX) + (mapY * mapY) + (mapZ * mapZ));
		

		//this is where loading happens (if there is anything to load)
		saveManager = new SaveManager();
		if(saveManager.Load(this, out List<SaveManager.BaseData> basesData, out TimeSpan difference, out int totalAgents))
		{
			Debug.Log("Game Manager Loading!");
			BuildManager buildManager = GameObject.Find("BuildManager").GetComponent<BuildManager>();
			buildManager.LoadBases(basesData, difference, totalAgents);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		unitsText.text = units.GetStringVal();
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void AddUnits(Value v)
	{
		//units.AddVal(v);
		units += (v * unitReturnRate);

		unitsText.text = units.GetStringVal();
	}

	public bool IncrementSpawnRate()
	{
		if (units > spawnRateIncreaseCost)
			units -= spawnRateIncreaseCost;
		else
			return false;

		unitsText.text = units.GetStringVal();

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

		unitsText.text = units.GetStringVal();

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

		unitsText.text = units.GetStringVal();

		unitReturnIncreaseCost *= 100.0f;

		//this can go over 1.0f, giving more units than resources earned.
		unitReturnRate += 0.1f;
		return true;
	}

	public bool IncrementMaxInactive()
	{
		if (units > maxInactiveIncreaseCost)
			units -= maxInactiveIncreaseCost;
		else
			return false;

		unitsText.text = units.GetStringVal();

		maxInactiveIncreaseCost *= 100.0f;

		//Inactive time should increase by 30 mins each upgrade. (18 x 100 seconds)
		maxPeriodInactive += 18;
		return true;
	}

	private void OnApplicationQuit()
	{
		//This also happens when closing the editor.
		BuildManager buildManager = GameObject.Find("BuildManager").GetComponent<BuildManager>();

		saveManager.Save(buildManager.allBases, this);
	}

}
