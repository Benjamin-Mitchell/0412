using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGameplay : MonoBehaviour
{
	private ResourceSpawner resourceSpawner;

    public GameObject[] agentStages = new GameObject[5];
    public Base associatedBase;

	private float loadingTime = 0.0f;

	[SerializeField]
	private float timeToLoad = 1.5f;

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

    [SerializeField]
    private AgentPathfinder agentPathfinding;

    private Resource resourceTarget;
    private GameObject defaultTarget;

	private Vector3 returnPosition;

    //how much value am I currently carrying
    private float carryingValue;


    /// Idle Rotation variables
    private Vector3 centre;
    private float radius;
    private Vector3 axis;
    private float orbitRotationSpeed = 10.0f;
    private float radiusCorrectionSpeed = 0.5f;
    private Vector3 previousPos;


	//for unloading
	private float sinceLastVisualUpdate = 0;


	// Start is called before the first frame update
	void Start()
    {
        agentStages[currentStage].SetActive(true);
		resourceSpawner = GameObject.FindGameObjectWithTag("ResourceSpawner").GetComponent<ResourceSpawner>();

	}

    // Update is called once per frames
    void Update()
    {
        UpdateState();
    }
    
    void UpdateState()
    {
        switch(state)
        {
            case State.UnderSpawnOrders:
                //is this overcomplicated?
                if (Vector3.Distance(transform.position, defaultTarget.transform.position) < 0.1f)
                {
                    agentPathfinding.NullifyPath();
                    state = State.Idle;
                }

                break;
            case State.ChasingResource:

                //TODO: check if resource is still present, or pick one that is guaranteed to still be in range.
                

                // TODO: need to modify distance to resource beforce collecting it
                if (Vector3.Distance(transform.position, resourceTarget.transform.position) < 0.3f)
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

                //agentPathfinding.SetPath(defaultTarget);     

                break;
			case State.LoadingResource:

				if (loadingTime > timeToLoad && agentPathfinding.hasPath)
				{
					loadingTime = .0f;
					Destroy(resourceTarget.gameObject);
					state = State.ReturningResource;
					agentPathfinding.AllowPathTraversal(true);
					break;
				}

				loadingTime += Time.deltaTime;

				break;
			case State.UnloadingResource:

				
				//how much has been uploaded since we last updated the base's value
				float increment = carryingValue * (Time.deltaTime / timeToLoad);
				sinceLastVisualUpdate += increment;

				if (sinceLastVisualUpdate > 1.0f)
				{
					sinceLastVisualUpdate -= 1.0f;
					associatedBase.addResources(1);
				}

				loadingTime += Time.deltaTime;
				

				if (loadingTime > timeToLoad && agentPathfinding.hasPath)
				{
					agentPathfinding.AllowPathTraversal(true);
					loadingTime = .0f;
					carryingValue = .0f;
					state = State.Idle;
					break;
				}

				break;
			case State.Idle:

				//check for resources
				if (resourceTarget != null)
				{
					agentPathfinding.SetPath(resourceTarget.gameObject);

					if (agentPathfinding.hasPath)
					{
						state = State.ChasingResource;
					}
				}					
				else
					resourceTarget = resourceSpawner.RequestResource();



				//chill out

				//Movement
				//transform.RotateAround(associatedBase.gameObject.transform.position, axis, orbitRotationSpeed * Time.deltaTime);
				//Vector3 orbitDesiredPosition = (transform.position - associatedBase.gameObject.transform.position).normalized * radius + associatedBase.gameObject.transform.position;
				//transform.position = Vector3.Slerp(transform.position, orbitDesiredPosition, Time.deltaTime * radiusCorrectionSpeed);
				//
				////Rotation
				//Vector3 relativePos = transform.position - previousPos;
				//Quaternion rotation = Quaternion.LookRotation(relativePos);
				//transform.rotation = Quaternion.Slerp(transform.rotation, rotation, radiusCorrectionSpeed * Time.deltaTime);
				//previousPos = transform.position;


				break;

        }
    }

	private void BeginLoading()
	{
		agentPathfinding.SetPath(defaultTarget);
		returnPosition = defaultTarget.transform.position;
		state = State.LoadingResource;
		agentPathfinding.AllowPathTraversal(false);
	}

	private void BeginUnloading()
	{
		//check here too
		resourceTarget = resourceSpawner.RequestResource();

		if (resourceTarget != null)
			agentPathfinding.SetPath(resourceTarget.gameObject);


		state = State.UnloadingResource;
		agentPathfinding.AllowPathTraversal(false);
	}

    public State getState()
    {
        return state;
    }

    public void setBase(Base b)
    {
        associatedBase = b;
        defaultTarget = b.agentDefaultTarget;
        agentPathfinding.SetPath(defaultTarget);
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
