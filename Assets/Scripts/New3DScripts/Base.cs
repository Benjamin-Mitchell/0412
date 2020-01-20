using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    public GameObject[] baseStages = new GameObject[5];
    public GameObject agentPrefab;

    public float heldResource = .0f;

    //Delete once replaced with upgrades
    public float timer = 0.0f;
    
    private int stage = 0;

    //TODO: make this a list and update functionality for multiple agents.
    private List<AgentGameplay> agents = new List<AgentGameplay>();


    private List<Resource> resourcesInScene = new List<Resource>();


    // Start is called before the first frame update
    void Start()
    {
        baseStages[stage].SetActive(true);

        AgentGameplay agent = Instantiate(agentPrefab, transform.position + transform.forward, Quaternion.identity).GetComponent<AgentGameplay>();
        agent.setBase(this);
        //move agent some arbitrary amount forward.
        agent.moveTo(transform.position + (transform.forward * 4.0f));
        agents.Add(agent);
    }

    // Update is called once per frame
    void Update()
    {
        //value += Time.deltaTime;

        if(timer > 3.0f && stage < 4)
            SwitchBase();

        //TODO: Find more efficient way of doing this
        UpdateAgents();
    }

    void UpdateAgents()
    {
        resourcesInScene = GameObject.FindGameObjectsWithTag("Resource");
        for (int i = 0; i < agents.Count;i++)
        {
            if(agents[i].state == State::Idle)
            {
                //find free resource and assign
                for (int j = 0; j < resourcesInScene.Count; j++)
                {
                    if(!resourcesInScene[j].assignedToAgent)
                    {
                        resourcesInScene[j].assignedToAgent = true;
                        agents[i].setResourceTarget(resourcesInScene[j]);
                    }
                }
            }
        }
    }

    void SwitchBase()
    {
        baseStages[stage].SetActive(false);
        stage++;

        for (int i = 0; i < agents.Count; i++)
            agent.setStage(stage);

        baseStages[stage].SetActive(true);
        timer = 0.0f;
    }

    public void addResources(int val)
    {
        heldResource += val;
    }
}
