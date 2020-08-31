﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour
{
    public GameObject[] baseStages = new GameObject[5];

    public float[] sphereScalingFactors = new float[5];

    public int heldResource = 0;
    public int reqToUpgrade = 0;
    public int reqToBuild = 0;
    
    public int stage = 0;
    public int numBuilds = 0;
    
    private List<AgentGameplay> agents = new List<AgentGameplay>();

    //TODO: this should probably be held globally somewhere instead of updated for each base.
    private List<Resource> resourcesInScene = new List<Resource>();

	[SerializeField]
	public Vector3 rotationFactor;

    public string baseType;

    // for tap mechanic to boost agents
    public float tapSeconds = 0.0f;
    public float increasePercent = 0.0f;

    public GameObject agentDefaultTarget;

    public GameObject buildSphere;

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

        AgentGameplay agent = Instantiate(agentPrefab, transform.position + transform.forward, Quaternion.identity).GetComponent<AgentGameplay>();
        agent.setBase(this);
        agents.Add(agent);

        reqToUpgrade = requiredToUpgrade();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAgents();

		transform.Rotate(rotationFactor);


        //DEBUG ONLY
        if(Input.GetKeyDown(KeyCode.G))
        {
            heldResource += 100;
        }
    }

    void UpdateAgents()
    {
        if (resourcesInScene.Count < 1)
            return;

        
        for (int i = 0; i < agents.Count;i++)
        {
            if(agents[i].getState() == AgentGameplay.State.Idle && resourcesInScene[0].transform.position.z < GameManager.Instance.mapZ)
            {
                //assign free resource and assign
                 agents[i].setResourceTarget(resourcesInScene[0]);
                 resourcesInScene.RemoveAt(0);
                 break;
            }
        }
    }

    public void grantResource(Resource r)
    {
        resourcesInScene.Add(r);
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

    public void addResources(int val)
    {
        float percent = 1.0f + (increasePercent / 100.0f);
        float fVal = (float)val * percent;
        heldResource += (int)fVal;
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
