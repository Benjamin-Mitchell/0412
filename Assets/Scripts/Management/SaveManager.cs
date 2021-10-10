using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager
{
	public struct AgentData
	{
		public string agentName;
		public Value resourceCollected;
		public Value distanceTravelled;
	}

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
		public List<AgentData> agentDatas;
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
		SaveCustomValue("SpawnRateCost", gameManager.spawnRateIncreaseCost);

		//Value Multiplier
		PlayerPrefs.SetFloat("ValueMultiplier", gameManager.resourceValueMultiplier);
		SaveCustomValue("ValueMultiplierCost", gameManager.resourceValueIncreaseCost);

		//Unit Return Rate
		PlayerPrefs.SetFloat("UnitReturnRate", gameManager.unitReturnRate);
		SaveCustomValue("UnitReturnRateCost", gameManager.unitReturnIncreaseCost);

		//Max Period Inactive
		PlayerPrefs.SetFloat("MaxPeriodInactive", gameManager.maxPeriodInactive);
		SaveCustomValue("MaxPeriodInactiveCost", gameManager.maxInactiveIncreaseCost);

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

			//agent stats
			for(int j = 0; j < b.numAgents; j++)
			{
				string agentString = "Agent_" + j + "_";
				
				//agent name
				PlayerPrefs.SetString(baseString + agentString + "AgentName", b.agents[j].stats.agentName);

				//agent resource collected
				Value vAgent = b.agents[j].stats.resourceCollected;
				PlayerPrefs.SetFloat(baseString + agentString + "ResourceCollectedRawVal", vAgent.GetRawVal());
				PlayerPrefs.SetInt(baseString + agentString + "ResourceCollectedDenotation", vAgent.GetRawDenotation());

				//agent distance travelled
				vAgent = b.agents[j].stats.distanceTravelled;
				PlayerPrefs.SetFloat(baseString + agentString + "DistanceTravelledRawVal", vAgent.GetRawVal());
				PlayerPrefs.SetInt(baseString + agentString + "DistanceTravelledDenotation", vAgent.GetRawDenotation());

			}
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

		Value v;
		//Game Manager Variables
		//Spawn Rate
		gameManager.resourceSpawnRate =  PlayerPrefs.HasKey("SpawnRate") ? PlayerPrefs.GetFloat("SpawnRate") : gameManager.resourceSpawnRate;
		gameManager.spawnRateIncreaseCost =  LoadCustomValue("SpawnRateCost", out v) ? v : gameManager.spawnRateIncreaseCost;

		//Value Multiplier
		gameManager.resourceValueMultiplier = PlayerPrefs.HasKey("ValueMultiplier") ? PlayerPrefs.GetFloat("ValueMultiplier") : gameManager.resourceValueMultiplier; 
		gameManager.resourceValueIncreaseCost = LoadCustomValue("ValueMultiplierCost", out v) ? v : gameManager.resourceValueIncreaseCost;

		//Unit Return Rate
		gameManager.unitReturnRate = PlayerPrefs.HasKey("UnitReturnRate") ? PlayerPrefs.GetFloat("UnitReturnRate") : gameManager.unitReturnRate;
		gameManager.unitReturnIncreaseCost = LoadCustomValue("UnitReturnRateCost", out v) ? v : gameManager.unitReturnIncreaseCost;

		//Max Period Inactive
		gameManager.maxPeriodInactive = PlayerPrefs.HasKey("MaxPeriodInactive") ? PlayerPrefs.GetFloat("MaxPeriodInactive") : gameManager.maxPeriodInactive;
		gameManager.maxInactiveIncreaseCost = LoadCustomValue("MaxPeriodInactiveCost", out v) ? v : gameManager.maxInactiveIncreaseCost;


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

			b.agentDatas = new List<AgentData>();

			for(int j = 0; j < b.numAgents; j++)
			{
				string agentString = "Agent_" + j + "_";
				AgentData a;

				a.agentName = PlayerPrefs.GetString(baseString + agentString + "AgentName");

				float aVal = PlayerPrefs.GetFloat(baseString + agentString + "ResourceCollectedRawVal");
				int aDenotation = PlayerPrefs.GetInt(baseString + agentString + "ResourceCollectedDenotation");
				a.resourceCollected = new Value(aVal, (Value.Denotation)aDenotation);

				aVal = PlayerPrefs.GetFloat(baseString + agentString + "DistanceTravelledRawVal");
				aDenotation = PlayerPrefs.GetInt(baseString + agentString + "DistanceTravelledDenotation");
				a.distanceTravelled = new Value(aVal, (Value.Denotation)aDenotation);

				b.agentDatas.Add(a);
				///TEST
			}
			bases.Add(b);
		}

		Debug.Log("Loading Successful!");
		return true;
	}

	private void SaveCustomValue(String name, Value val)
	{
		PlayerPrefs.SetFloat(name + "RawValKey", val.GetRawVal());
		PlayerPrefs.SetInt(name + "DenotationKey", val.GetRawDenotation());
	}

	private bool LoadCustomValue(String name, out Value val)
	{
		val = 0;
		if (!PlayerPrefs.HasKey(name + "RawValKey"))
			return false;
		float rawVal = PlayerPrefs.GetFloat(name + "RawValKey");
		int denotation = PlayerPrefs.GetInt(name + "DenotationKey");
		val = new Value(rawVal, (Value.Denotation)denotation);
		return true;
	}
}
