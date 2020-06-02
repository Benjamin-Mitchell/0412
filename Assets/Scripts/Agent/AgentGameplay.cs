using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGameplay : MonoBehaviour
{
    public GameObject[] agentStages = new GameObject[5];
    public Base associatedBase;

    

    public enum State
    {
        UnderSpawnOrders,
        ChasingResource,
        ReturningResource,
        Idle
    };

    private State state = State.Idle;

    int currentStage = 0;

    [SerializeField]
    private AgentPathfinder agentPathfinding;

    private Resource moveToTarget;
    private GameObject defaultTarget;

    private Resource resourceTarget;
    //how much value am I currently carrying
    private float carryingValue;


    /// Idle Rotation variables
    private Vector3 centre;
    private float radius;
    private Vector3 axis;
    private float orbitRotationSpeed = 10.0f;
    private float radiusCorrectionSpeed = 0.5f;
    private Vector3 previousPos;



    // Start is called before the first frame update
    void Start()
    {
        agentStages[currentStage].SetActive(true);
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
                //is this overcomplicated currently?
                if (Vector3.Distance(transform.position, defaultTarget.transform.position) < 0.1f)
                {
                    agentPathfinding.NullifyPath();
                    state = State.Idle;
                }

                break;
            case State.ChasingResource:

                //TODO: check if resource is still present, or pick one that is guaranteed to still be in range.
                

                // TODO: need to modify distance to resource beforce collecting it
                if (Vector3.Distance(transform.position, moveToTarget.transform.position) < 0.3f)
                {
                    //TODO: An idle moment or two here (maybe an animation?) would be really cool.
                    // Get Resource
                    carryingValue = resourceTarget.value;
                
                    Destroy(resourceTarget.gameObject);
                    state = State.ReturningResource;
                    break;
                }

                agentPathfinding.SetPath(moveToTarget.gameObject);

                break;
            case State.ReturningResource:

                if (Vector3.Distance(transform.position, defaultTarget.transform.position) < 0.05f)
                {
                    associatedBase.addResources((int)carryingValue);
                    carryingValue = .0f;
                    state = State.Idle;
                    break;
                }

                agentPathfinding.SetPath(defaultTarget);     

                break;
            case State.Idle:
                
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

    public void setResourceTarget(Resource resource)
    {
        // make sure a movement check is done immediately with new target
        agentPathfinding.orderImmediateMovement();


        resourceTarget = resource;
        moveToTarget = resourceTarget;
        state = State.ChasingResource;
    }
}
