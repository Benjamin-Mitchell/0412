using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGameplay : MonoBehaviour
{
	private ResourceSpawner resourceSpawner;

    public GameObject[] agentStages = new GameObject[5];
    public Base associatedBase;

	private float loadingTime = 0.0f;

	private float timeToLoad = 5.0f;

	AgentStats stats;

    public enum State
    {
        UnderSpawnOrders,
        ChasingResource,
        ReturningResource,
		LoadingResource,
		UnloadingResource,
        Idle
    };

    private State state = State.Idle;

    int currentStage = 0;
	
    private AgentPathfinder agentPathfinding;

    private Resource resourceTarget;
    private GameObject defaultTarget;

	// using defaultTarget will copy by reference (latest position of the target)
	// returnPosition will copy the value out
	private Vector3 returnPosition;

    //how much value am I currently carrying
    private Value carryingValue;

	private void Awake()
	{
		agentPathfinding = gameObject.GetComponent<AgentPathfinder>();
		stats = GetComponent<AgentStats>();
	}
	// Start is called before the first frame update
	void Start()
    {
        agentStages[currentStage].SetActive(true);
		resourceSpawner = GameObject.FindGameObjectWithTag("ResourceSpawner").GetComponent<ResourceSpawner>();

	}

    // Update is called once per frames
    void Update()
    {
		if(Input.GetKey(KeyCode.Q))
			Debug.Log(gameObject.name +"'s Current State: " + state);
        UpdateState();
    }
    
    void UpdateState()
    {
        switch(state)
        {
            case State.UnderSpawnOrders:
				//is this overcomplicated?
				if (Vector3.Distance(transform.position, returnPosition) < 0.1f)
                {
                    agentPathfinding.NullifyPath();
					IdleState();
                }

                break;
            case State.ChasingResource:

                //TODO: check if resource is still present, or pick one that is guaranteed to still be in range.
                

                // TODO: need to modify distance to resource beforce collecting it
                if (Vector3.Distance(transform.position, resourceTarget.transform.position) < 2.5f)
                {
                    // Get Resource
                    carryingValue = resourceTarget.value;
					
					BeginLoading();
					break;
                }

                agentPathfinding.SetPath(resourceTarget.gameObject);

                break;
            case State.ReturningResource:

				if (Vector3.Distance(transform.position, returnPosition) < 0.2f)
                {
					BeginUnloading();
                    break;
                }

                agentPathfinding.SetPath(returnPosition);     

                break;
			case State.LoadingResource:
				
				if (loadingTime > timeToLoad && agentPathfinding.hasPath)
				{
					loadingTime = .0f;
					ReturnWithResource();
					break;
				}

				loadingTime += Time.deltaTime;

				break;
			case State.UnloadingResource:
				loadingTime += Time.deltaTime;

				if (loadingTime > timeToLoad && agentPathfinding.hasPath)
				{
					agentPathfinding.AllowPathTraversal(true);
					loadingTime = .0f;

					stats.resourceCollected += carryingValue;
					carryingValue = .0f;

					IdleState();
					break;
				}

				if(resourceTarget == null)
					resourceTarget = resourceSpawner.RequestResource();

				break;
			case State.Idle:

				//check for resources
				if (resourceTarget != null)
				{
					agentPathfinding.SetPath(resourceTarget.gameObject);

					if (agentPathfinding.hasPath)
					{
						BeginChasing();
					}
				}
				else
				{
					resourceTarget = resourceSpawner.RequestResource();
				}


				//chill out
				//agentPathfinding.OrbitTarget(associatedBase.gameObject);

				break;

        }
    }

	private void BeginChasing()
	{
		state = State.ChasingResource;
		agentPathfinding.AllowPathTraversal(true);

	}

	private void ReturnWithResource()
	{
		Destroy(resourceTarget.gameObject);
		state = State.ReturningResource;
		agentPathfinding.AllowPathTraversal(true);
	}

	private void BeginLoading()
	{
		agentPathfinding.SetPath(associatedBase.gameObject);
		agentPathfinding.SetOrbitTarget(resourceTarget.gameObject);
		returnPosition = defaultTarget.transform.position;
		resourceTarget.consume();
		state = State.LoadingResource;
		agentPathfinding.AllowPathTraversal(false);
	}

	private void BeginUnloading()
	{
		//check here too
		resourceTarget = resourceSpawner.RequestResource();
		agentPathfinding.SetOrbitTarget(associatedBase.gameObject);

		if (resourceTarget != null)
			agentPathfinding.SetPath(resourceTarget.gameObject);

		associatedBase.AddResourcesOverTime(carryingValue, timeToLoad);

		state = State.UnloadingResource;
		agentPathfinding.AllowPathTraversal(false);
	}

	private void IdleState()
	{
		state = State.Idle;
		agentPathfinding.AllowPathTraversal(false);
		agentPathfinding.SetOrbitTarget(associatedBase.gameObject);

	}

    public State getState()
    {
        return state;
    }

    public void setBase(Base b)
    {
        associatedBase = b;
        defaultTarget = b.agentDefaultTarget;
		returnPosition = defaultTarget.transform.position;
		agentPathfinding.SetPath(returnPosition);
        state = State.UnderSpawnOrders;
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
