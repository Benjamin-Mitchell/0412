using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGameplay : MonoBehaviour
{
    public GameObject[] agentStages = new GameObject[5];
    public Base associatedBase;

    int currentStage = 0;

    // Start is called before the first frame update
    void Start()
    {
        agentStages[currentStage].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setStage(int stage)
    {
        if (stage == currentStage)
            return;

        agentStages[currentStage].SetActive(false);
        currentStage = stage;
        agentStages[currentStage].SetActive(true);
    }
}
