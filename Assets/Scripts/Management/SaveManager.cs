using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager
{
	//bases will need an ID to identify which base they are. This can be in build order.
	public struct BaseData
	{
		public int ID;
		public Vector3 position;
		public int currentStage;
		public string baseType;
		public float boostTime;
		public Value heldResource;
		public int numAgents;
	}

	//something for total game status
	//e.g. total resource, upgrades purchased etc.
	float totalResource;

	public void Save(List<Base> allBases, GameManager gameManager)
	{
		//time when last quit.
		PlayerPrefs.SetString("TimeString", System.DateTime.Now.ToBinary().ToString());

		//Game Manager Variables
		//Spawn Rate
		PlayerPrefs.SetFloat("SpawnRate", gameManager.resourceSpawnRate);

		//Value Multiplier
		PlayerPrefs.SetFloat("ValueMultiplier", gameManager.resourceValueMultiplier);

		//Unit Return Rate
		PlayerPrefs.SetFloat("UnitReturnRate", gameManager.unitReturnRate);

		//Number of bases saved
		PlayerPrefs.SetInt("NumberOfBases", allBases.Count);

		//base saves
		for(int i = 0; i < allBases.Count; i++)
		{
			Base b = allBases[i];
			string baseString = "Base_" + b.ID + "_";

			//ID
			PlayerPrefs.SetInt(baseString, b.ID);

			//position
			PlayerPrefs.SetFloat(baseString + "Position_X", b.transform.position.x);
			PlayerPrefs.SetFloat(baseString + "Position_Y", b.transform.position.y);
			PlayerPrefs.SetFloat(baseString + "Position_Z", b.transform.position.z);

			//current stage
			PlayerPrefs.SetInt(baseString + "Stage", b.stage);

			//base type
			PlayerPrefs.SetString(baseString + "Type", b.baseType);

			//boost time remaining
			PlayerPrefs.SetFloat(baseString + "BoostTime", b.tapSeconds);

			//held resource
			Value v = b.HeldResource;
			PlayerPrefs.SetFloat(baseString + "Val", v.GetRawVal());
			PlayerPrefs.SetInt(baseString + "Denotation", v.GetRawDenotation());

			//number of agents
			PlayerPrefs.SetInt(baseString + "NumAgents", b.numAgents);
		}
		Debug.Log("Saving!");
	}

	//return true if loads successfully
	public bool Load(GameManager gameManager, out List<BaseData> bases, out TimeSpan difference, out int totalAgents)
	{
		Debug.Log("Loading!");

		bases = new List<BaseData>();
		difference = new TimeSpan();

		totalAgents = 0;
		//if nothing has been saved.... this is the first time.
		if (!PlayerPrefs.HasKey("NumberOfBases"))
			return false;

		//Bases
		int numBases = PlayerPrefs.GetInt("NumberOfBases");

		if (numBases < 1)
			return false;

		long temp = Convert.ToInt64(PlayerPrefs.GetString("TimeString"));

		DateTime oldDate = DateTime.FromBinary(temp);

		//how long since last close
		difference = System.DateTime.Now.Subtract(oldDate);

		//Game Manager Variables
		//Spawn Rate
		gameManager.resourceSpawnRate =  PlayerPrefs.GetFloat("SpawnRate");

		//Value Multiplier
		gameManager.resourceValueMultiplier = PlayerPrefs.GetFloat("ValueMultiplier");

		//Unit Return Rate
		gameManager.unitReturnRate = PlayerPrefs.GetFloat("UnitReturnRate");

		gameManager.resourceSpawnRate = 1.0f;
		gameManager.resourceValueMultiplier = 1.0f;
		gameManager.unitReturnRate = 0.5f;



		for (int i = 0; i < numBases; i++)
		{
			BaseData b;

			string baseString = "Base_" + i + "_";
			
			//ID
			b.ID = PlayerPrefs.GetInt(baseString);

			//position
			b.position.x = PlayerPrefs.GetFloat(baseString + "Position_X");
			b.position.y = PlayerPrefs.GetFloat(baseString + "Position_Y");
			b.position.z = PlayerPrefs.GetFloat(baseString + "Position_Z");

			//current stage
			b.currentStage = PlayerPrefs.GetInt(baseString + "Stage");

			//base type
			b.baseType = PlayerPrefs.GetString(baseString + "Type");

			//boost time remaining
			b.boostTime = PlayerPrefs.GetFloat(baseString + "BoostTime");

			//held resource
			float val = PlayerPrefs.GetFloat(baseString + "Val");
			int denotation = PlayerPrefs.GetInt(baseString + "Denotation");
			b.heldResource = new Value(val, (Value.Denotation)denotation);

			//number of agents
			b.numAgents = PlayerPrefs.GetInt(baseString + "NumAgents");
			totalAgents += b.numAgents;

			bases.Add(b);
		}

		Debug.Log("Loading Successful!");
		return true;
	}
}
