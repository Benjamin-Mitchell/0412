using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    public GameObject[] baseStages = new GameObject[5];
    public GameObject agentPrefab;

    public float value = 0.0f;
    
    private int stage = 0;
    private AgentGameplay agent;

    // Start is called before the first frame update
    void Start()
    {
        baseStages[stage].SetActive(true);
        agent = Instantiate(agentPrefab, transform.position + transform.forward, Quaternion.identity).GetComponent<AgentGameplay>();
    }

    // Update is called once per frame
    void Update()
    {
        value += Time.deltaTime;

        if(value > 3.0f && stage < 5)
            SwitchBase();
    }

    void SwitchBase()
    {
        baseStages[stage].SetActive(false);
        stage++;
        agent.setStage(stage);
        baseStages[stage].SetActive(true);
        value = 0.0f;
    }
}
