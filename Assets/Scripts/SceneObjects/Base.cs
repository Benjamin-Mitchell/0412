using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour
{
    public GameObject[] baseStages = new GameObject[5];

    public float[] sphereScalingFactors = new float[5];

    private Value heldResource = 0;
	public Value HeldResource {get { return heldResource;  } set { heldResource = value; } }
    private Value reqToUpgrade = 0;
    public Value ReqToUpgrade { get { return reqToUpgrade; } set { reqToUpgrade = value; } }
	private Value reqToBuild = 0;
	public Value ReqToBuild { get { return reqToBuild; } set { reqToBuild = value; } }

	public int stage = 0;
    public int numBuilds = 0;

	[SerializeField]
	public Vector3 rotationFactor;

    public string baseType;

    // for tap mechanic to boost agents
    public float tapSeconds = 0.0f;
    public float increasePercent = 0.0f;

    public GameObject agentDefaultTarget;

    public GameObject buildSphere;

	private List<AgentGameplay> agents = new List<AgentGameplay>();

	[System.NonSerialized]
	public int numAgents = 0;

	private GameManager gameManager;

	//used to identify the base during save/load
	public int ID;

    private void Awake()
    {
        GameObject spherePrefab = (GameObject)Resources.Load("BuildSphere", typeof(GameObject));
        buildSphere = GameObject.Instantiate(spherePrefab, transform.position, Quaternion.identity);
        buildSphere.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        baseType = gameObject.name.Contains("Clone") ? gameObject.name.Substring(5, gameObject.name.Length - 5 - 7) : gameObject.name.Substring(5);
        GameObject agentPrefab = (GameObject)Resources.Load("Agent_" + baseType, typeof(GameObject));

        baseStages[stage].SetActive(true);

		if(numAgents == 0)
			AddAgent(agentPrefab, transform.position + transform.forward, out AgentGameplay a);

        reqToUpgrade = requiredToUpgrade();

		gameManager = GameManager.Instance;
	}

    // Update is called once per frame
    void Update()
    {
		transform.Rotate(rotationFactor);


        //DEBUG ONLY
        if(Input.GetKeyDown(KeyCode.G))
        {
            heldResource += 100;
        }
    }

	public void AddAgent(GameObject prefab, Vector3 position, out AgentGameplay a)
	{
		a = Instantiate(prefab, position, Quaternion.identity).GetComponent<AgentGameplay>();
		a.setBase(this);
		agents.Add(a);
		numAgents++;
	}

    public void UpgradeBase()
    {
        if (IsFullyUpgraded())
            return;
        heldResource -= reqToUpgrade;
        baseStages[stage].SetActive(false);
        stage++;

        for (int i = 0; i < agents.Count; i++)
            agents[i].setStage(stage);

        baseStages[stage].SetActive(true);
        reqToUpgrade = requiredToUpgrade();
    }

    public bool IsFullyUpgraded()
    {
        bool val = stage < 4 ? false : true;
        return val;
    }

	public void LoadSetup(SaveManager.BaseData bData)
	{
		//extract information from saved BaseData
		ID = bData.ID;
		stage = bData.currentStage;
		baseType = bData.baseType;
		tapSeconds = bData.boostTime;
		heldResource = bData.heldResource;

		int agentsToLoad = bData.numAgents;

		baseStages[stage].SetActive(true);

		GameObject agentPrefab = (GameObject)Resources.Load("Agent_" + baseType, typeof(GameObject));

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
		}
			
	}

	public void AddResourcesOverTime(Value val, float time)
	{
		float percent = (1.0f + (increasePercent / 100.0f)) * gameManager.GetResourceValueMultiplier();

		Value mulVal = val * percent;

		StartCoroutine(IncrementResourceOverTime(mulVal, time));
	}

	public IEnumerator IncrementResourceOverTime(Value val, float time)
	{
		float timePassed = 0;

		while (timePassed < time)
		{
			Value increment = val * (Time.deltaTime / time);
			heldResource += increment;
			gameManager.AddUnits(increment);
			timePassed += Time.deltaTime;
			yield return null;
		}
	}

	public void addResources(int val)
    {
        float percent = 1.0f + (increasePercent / 100.0f);
        float fVal = (float)val * percent;
		heldResource += fVal;
    }


    public int baseVal = 50;
    public int multiplePerUpgrade = 1;
    //multiple is done before power
    public int powerPerUpgrade = 2; 

    public int requiredToUpgrade()
    {
        int stageVal = stage + 1;
        return ((baseVal * multiplePerUpgrade) * stageVal) ^ stageVal ^ powerPerUpgrade;
    }
}
