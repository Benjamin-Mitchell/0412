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

	private Value units = 10000000;

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
		unitsText.text = units.GetStringVal();

		//Value val = 100000;
		
		//Debug.Log("initial Value: " + val.GetStringVal());
		
		//Value temp = 1000;
		////val += 1000;
		
		////Debug.Log("After adding 1000: " + val.GetStringVal());

		////val -= 1000;
		
		////Debug.Log("After subtracting 1000: " + val.GetStringVal());

		////val -= 1000000000;

		////Debug.Log("After subtracting 1000000000: " + val.GetStringVal());

		////val += 100000;

		////Debug.Log("After adding 100000(100k): " + val.GetStringVal());

		//val -= 99999.0f;

		//Debug.Log("After substracting 99999(99.999k): " + val.GetStringVal());
		//Debug.Log("val.val: " + val.val);

		//val += temp;
		
		//Debug.Log("After adding 1k: " + val.GetStringVal());
		
		//val -= temp;
		
		//Debug.Log("After subtracting 1k: " + val.GetStringVal());
		
		//val *= 5;
		
		//Debug.Log("After multiplying by 5: " + val.GetStringVal());
		
		//val /= 5;
		
		//Debug.Log("After dividing by 5: " + val.GetStringVal());
		
		//temp = 5;
		//val *= temp;
		
		//Debug.Log("After multiplying by 5(Value): " + val.GetStringVal());
		
		//val /= temp;
		
		//Debug.Log("After dividing by 5(Value): " + val.GetStringVal());
		
		//temp = 5000;
		//Value tempTwo = 5000000;
		
		
		//Debug.Log("Is 5000 greater than 5000000?:" + (temp > tempTwo ? "Yes!" : "No!"));
		//Debug.Log("Is 5000000 greater than 5000?:" + (tempTwo > temp ? "Yes!" : "No!"));
		
		
		//Debug.Log("Is 5000 less than 5000000?:" + (temp < tempTwo ? "Yes!" : "No!"));
		//Debug.Log("Is 5000000 less than 5000?:" + (tempTwo < temp ? "Yes!" : "No!"));

		//Debug.Log( tempTwo.ToFloat() + " / " + temp.ToFloat() + " is "+ (tempTwo / temp).ToFloat());
		//Debug.Log("Ben Test ToFloat! And Create a ToDouble!");

		//temp = 560;
		//tempTwo = 33400;
		
		//Debug.Log("A random number between 560 and 33400 is:" + Value.RandomRange(temp, tempTwo).GetStringVal());
		
		//temp = 10;
		
		//Debug.Log("10 to the power of 5 is: " + Value.Pow(temp, 5).GetStringVal());
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
