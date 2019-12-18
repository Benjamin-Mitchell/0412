using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent agent;
    private GameManager gameManager;
    private Interactable target;
    private float interactionTimeRemaining;
    private int spotTaken = 0;

    private int debugNumber = 0;
    enum State
    {
        WAITING,
        INTERACTING,
        WALKING
    }
    private State state = State.WAITING;
    private float initialY = 0.0f;


    public int ID;

    private int points;
    public int Points
    {
        get { return points; }
        set { points = value; }
    }



    // Use this for initialization
    void Awake () {
        points = 0;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
	}

    void Start()
    {
        initialY = transform.position.y;
    }
	
	// Update is called once per frame
	void Update () {

        ////TODO: Modify agent properties based on number of points


        //float factor = 1.0f + (1.0f * ((float)points / 4.0f));
        //Debug.Log(factor);
        //float factor = 2.0f;
        //transform.localScale *= factor;
        //transform.position = new Vector3(transform.position.x, 100.0f, transform.position.z);

        switch(state)
        {
            case State.WALKING:
                if (!agent.pathPending && !agent.hasPath)
                {
                    state = State.INTERACTING;
                    interactionTimeRemaining = target.timeRequired;
                }
                break;
            case State.INTERACTING:
                interactionTimeRemaining -= Time.deltaTime;
                if (interactionTimeRemaining <= 0.0f)
                {
                    state = State.WAITING;
                    Interactable oldTarget = target;
                    int oldSpot = spotTaken;
                    points += target.pointWorth;
                    gameManager.notifyTaskCompletion(ID /*, target.pointWorth*/);
                    oldTarget.spotsAvailable[oldSpot] = true;
                }
                break;
            case State.WAITING:
                //do an animation here or something one day.
                break;
            default:
                Debug.Log("AGENT: Shouldn't have got here");
                break;
        }
	}

    public void assignTarget(Interactable interactable, Vector3 freeSpot, int spotTaken)
    {
        this.spotTaken = spotTaken;
        target = interactable;
        agent.SetDestination(freeSpot);
        state = State.WALKING;
    }
}
