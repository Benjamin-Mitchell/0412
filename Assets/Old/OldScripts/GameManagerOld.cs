using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerOld : MonoBehaviour {

    //private int points;
    //public int Points
    //{
    //    get { return points; }
    //    set { points = value; }
    //}

    //dynamic list of agent-interactable objects.
    public List<OldInteractable> interactables;
    public List<Agent> agents;
    public List<int> freeAgents;

    private int pointMultiplier = 1;

    void Awake()
    {
        //points = 0;
    }

	// Use this for initialization
	void Start () {

        //TODO: see if there's a more efficient way of doing this.
        GameObject[] agentObjects = GameObject.FindGameObjectsWithTag("Agent");
        GameObject[] interactableObjects = GameObject.FindGameObjectsWithTag("Interactable");
        
        //bit of a round-about way of getting Agent Component list.
        foreach (GameObject agent in agentObjects){
            agents.Add(agent.GetComponent<Agent>());
        }

        foreach (GameObject interactable in interactableObjects)
        {
            interactables.Add(interactable.GetComponent<OldInteractable>());
        }

        AssignAgentIDs();
        AssignInitialTargets();
	}

    void AssignAgentIDs()
    {
        for(int i = 0; i < agents.Count; i++)
        {
            agents[i].ID = i;
        }
    }

    void AssignInitialTargets()
    {
        foreach(Agent agent in agents)
        {
            if (!assignTask(agent.ID))
                freeAgents.Add(agent.ID);
        }
    }
	
    // Update is called once per frame
    void Update () {
        if(freeAgents.Count > 0)
        {
            // TODO: implement me
        }
	}
    
    bool assignTask(int agentID)
    {
        int i = 0;
        while (i < interactables.Count)
        {
            for (int k = 0; k < interactables[i].spotsAvailable.Length; k++)
            {
                if (interactables[i].spotsAvailable[k])
                {
                    agents[agentID].assignTarget(interactables[i], interactables[i].freeSpots[k], k);
                    interactables[i].spotsAvailable[k] = false;
                    
                    return true;
                }
            }
            i++;

        }
        return false;
    }

    public void notifyTaskCompletion(int ID /*, int pointValue*/)
    {
        //points += pointValue * pointMultiplier;

        //assign a new task if one is available, otherwise keep free agent waiting.
        if(!assignTask(ID))
            freeAgents.Add(ID);

    }
}
