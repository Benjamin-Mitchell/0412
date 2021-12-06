using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour
{
	private string[] possibleNames =
	{ "The Cottage", "Ground Zero", "Beginnings", "Alpha One", "The Dojo", "King's Market", "The Zoo", "The Kennel",
	"The Shard", "Castle", "The Den", "Queen's Chambers", "Flat 35", "The Royal Court", "Skylab", "Salyut", "ISS",
	"TianGong", "Kosmos", "Mir", "Genesis", "OPS 0855", "DOS-2", "Gateway", "Springer", "Habitat", "Shuttle"};

	[NonSerialized]
	public string baseName;

	public GameObject[] baseStages = new GameObject[5];

	//TODO? Or should each base have a static scaling factor, since it will eventually scale up?
	public float[] sphereScalingFactors = new float[5];

	public Value[] heldResources;

	//private Value heldResource = 0;
	//public Value HeldResource {get { return heldResource;  } set { heldResource = value; } }

	public Value[] reqsToUpgrade;

	//private Value reqToUpgrade = 0;
	//public Value ReqToUpgrade { get { return reqToUpgrade; } set { reqToUpgrade = value; } }

	public Value[] reqsToBuild;
	//private Value reqToBuild = 0;
	//public Value ReqToBuild { get { return reqToBuild; } set { reqToBuild = value; } }

	private Value maxLoadGain = 0.0f;

	[NonSerialized]
	public int stage = 0;

	[NonSerialized]
    public int numBuilds = 0;

	[SerializeField]
	public Vector3 rotationFactor;

	[NonSerialized]
	public string baseTypeString;

	public int baseTier;

	// for tap mechanic to boost agents
	[NonSerialized]
	public float tapSeconds = 0.0f;

	[NonSerialized]
	public float increasePercent = 0.0f;

    public GameObject agentDefaultTarget;

	[NonSerialized]
	public GameObject buildSphere;

	[NonSerialized]
	public List<AgentGameplay> agents = new List<AgentGameplay>();

	[System.NonSerialized]
	public int numAgents = 0;

	private GameManager gameManager;

	//used to identify the base during save/load
	[NonSerialized]
	public int ID;

    private void Awake()
    {
		baseName = possibleNames[UnityEngine.Random.Range(0, possibleNames.Length)];

		GameObject spherePrefab = (GameObject)Resources.Load("BuildSphere", typeof(GameObject));
        buildSphere = GameObject.Instantiate(spherePrefab, transform.position, Quaternion.identity);
        buildSphere.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
		heldResources = new Value[baseTier + 1];

		for(int i = 0; i < heldResources.Length; i++)
		{
			heldResources[i] = 0;
		}

		baseTypeString = gameObject.name.Contains("Clone") ? gameObject.name.Substring(5, gameObject.name.Length - 5 - 7) : gameObject.name.Substring(5);
        GameObject agentPrefab = (GameObject)Resources.Load("Agent_" + baseTypeString, typeof(GameObject));

        baseStages[stage].SetActive(true);

		if(numAgents == 0)
			AddAgent(agentPrefab, transform.position + transform.forward, out AgentGameplay a);

        reqsToUpgrade = RequiredToUpgrade();

		gameManager = GameManager.Instance;
	}

    // Update is called once per frame
    void Update()
    {
		transform.Rotate(rotationFactor);

        //DEBUG ONLY
        if(Input.GetKeyDown(KeyCode.G))
        {
			for(int i = 0; i < heldResources.Length; i++)
				heldResources[i] += 100;
        }
		
		//DEBUG ONLY
		if(Input.GetKeyDown(KeyCode.H))
		{
			for (int i = 0; i < heldResources.Length; i++)
				heldResources[i] += 100000000;
		}
	}

	public void AddAgent(GameObject prefab, Vector3 position, out AgentGameplay a)
	{
		a = Instantiate(prefab, position, Quaternion.identity).GetComponent<AgentGameplay>();
		a.setBase(this);
		agents.Add(a);
		numAgents++;
	}

	public bool CanBuildBase()
	{
		for (int i = 0; i < heldResources.Length; i++)
		{
			if (heldResources[i] < reqsToBuild[i])
				return false;
		}

		return true;
	}

	//false if failed to upgrade
	public bool UpgradeBase()
    {
        if (IsFullyUpgraded())
            return false;

		//check if we have the resources available
		for(int i = 0; i < heldResources.Length; i++)
		{
			if (heldResources[i] < reqsToUpgrade[i])
				return false;
		}

		for (int i = 0; i < heldResources.Length; i++)
			heldResources[i] -= reqsToUpgrade[i];

        baseStages[stage].SetActive(false);
        stage++;

        for (int i = 0; i < agents.Count; i++)
            agents[i].setStage(stage);

        baseStages[stage].SetActive(true);
        reqsToUpgrade = RequiredToUpgrade();
		return true;
    }

    public bool IsFullyUpgraded()
    {
        bool val = stage < 4 ? false : true;
        return val;
    }

	public void LoadSetup(SaveManager.BaseData bData, TimeSpan difference, float spawnRate, int totalAgents, out float maxResourceCollect)
	{
		//extract information from saved BaseData
		ID = bData.ID;
		baseName = bData.baseName;
		stage = bData.currentStage;
		baseTypeString = bData.baseTypeString;
		tapSeconds = bData.boostTime;
		heldResources = bData.heldResources;

		int agentsToLoad = bData.numAgents;

		baseStages[stage].SetActive(true);

		gameManager = GameManager.Instance;

		//resource gained should be a function of:
		//	- Time difference,
		//	- Num agents & move speeds,
		//  - Spawn rate of resource, spawn rate is in seconds.
		//
		// Resources gained / 100 secs @ 1 move speed. (R) - 1.25
		// Available resources per 100 sec: ((100 / spawn rate)/ gameManager.spawnrate) / (number of agents) (A)
		// Number of 100 sec intervals passed: time diff in seconds / 100(T)
		// Average resource Value(V)
		// 
		// gained = T * (R * moveSpeed) * (A * V)

		float R = 1.4f;
		float A = (100 / (spawnRate / gameManager.resourceSpawner.resourceSpawnRateUpgrade) / (float)totalAgents);
		double t = difference.TotalSeconds / 100.0;
		float T = Mathf.Min((float)t, gameManager.maxPeriodInactive);
		float V = 5;

		float gain = 0.0f;
		maxResourceCollect = 0.0f;

		GameObject agentPrefab = (GameObject)Resources.Load("Agent_" + baseTypeString, typeof(GameObject));

		//agents are spawned in a circle around base when loading. might be better ways to do this in the future.
		//e.g. spawn en-route to resources or come out of the base one-by-one.
		float spawnRadius = 4.0f;
		
		for (int i = 0; i < agentsToLoad; i++)
		{
			float theta = (i * (2 * Mathf.PI) / agentsToLoad);
			float x = transform.position.x + (spawnRadius * Mathf.Cos(theta));
			float z = transform.position.z + (spawnRadius * Mathf.Sin(theta));
			Vector3 newAgentPos = new Vector3(x, transform.position.y, z);
			AddAgent(agentPrefab, newAgentPos, out AgentGameplay a);
			a.setStage(stage);
			a.stats.SetStats(bData.agentDatas[i]);

			float moveSpeed = a.gameObject.GetComponent<AgentPathfinder>().moveSpeed;

			float agentMaxRes = R * moveSpeed;

			maxResourceCollect += agentMaxRes;
			float test = T * agentMaxRes * (/*A * */ V);
			Debug.Log("This agent earned max " + test + " resources while you were away!");
			gain += test;
		}

		maxLoadGain = gain;
		//heldResource += gain;
	}

	//actual resource gain needs to take into account globally available resource after all other agents & bases take a slice.
	public void LoadSetupFinalize(float ratioAvailable)
	{
		//Some way of determining how much of each resource would have been available is needed
		//e.g. 10% teabag, 20% pie etc. etc.
		for(int i = 0; i < heldResources.Length; i++)
		{
			heldResources[i] += (maxLoadGain * ratioAvailable * gameManager.resourceSpawner.resourceSpawnRates[i]);
		}
		
	}

	public void AddResourcesOverTime(Value val, float time)
	{
		float percent = (1.0f + (increasePercent / 100.0f)) * gameManager.resourceValueMultiplier;

		Value mulVal = val * percent;

		for(int i = 0; i < heldResources.Length; i++)
			StartCoroutine(IncrementResourceOverTime(mulVal, time, i));	
	}

	public IEnumerator IncrementResourceOverTime(Value val, float time, int index)
	{
		float timePassed = 0;

		while (timePassed < time)
		{
			Value increment = val * (Time.deltaTime / time);
			heldResources[index] += increment;
			gameManager.AddUnits(increment);
			timePassed += Time.deltaTime;
			yield return null;
		}
	}

	public void addResources(int val, int index)
    {
        float percent = 1.0f + (increasePercent / 100.0f);
        float fVal = (float)val * percent;
		heldResources[index] += fVal;
    }

	[Header("Upgrade Settings")]
	[Tooltip("Base Cost for each Upgrade")]
    public int upgradeBaseCost = 50;
	[Tooltip("Multiple of Base Cost for Upgrade")]
	public int multiplePerUpgrade = 1;
	[Tooltip("Number of Powers")]
	public int powerPerUpgrade = 2; 

    public Value[] RequiredToUpgrade()
    {
        int stageVal = stage + 1;
		Value[] temp = new Value[heldResources.Length];
		int i = heldResources.Length - 1;
		int baseCost = ((upgradeBaseCost * multiplePerUpgrade) * stageVal) ^ stageVal ^ powerPerUpgrade;
		while(i >= 0)
		{
			temp[i] = baseCost;
			baseCost /= 4;
			i--;
		}
		return temp;
    }

	//TODO: Make this better, use some cooler maths
	public void RequiredToBuild()
	{
		reqsToBuild = new Value[heldResources.Length];
		for (int j = 0; j < heldResources.Length; j++)
			reqsToBuild[j] = Value.Pow(Value.Pow(reqsToUpgrade[j], 1.2f), Mathf.Pow((numBuilds + 1), 1.2f));
	}


	//isUprade: true == upgrade, false == build
	public void DeductResources(bool isUpgrade)
	{
		for(int i = 0; i < heldResources.Length; i++)
		{
			heldResources[i] -= isUpgrade ? reqsToUpgrade[i] : reqsToBuild[i];
		}
	}
}
