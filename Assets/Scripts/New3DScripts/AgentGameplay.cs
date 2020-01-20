using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGameplay : MonoBehaviour
{
    public GameObject[] agentStages = new GameObject[5];
    public Base associatedBase;

    //How often should the path information be reset (in seconds)
    public float pathUpdateStep = 0.25f;

    enum State
    {
        UnderOrders,
        ChasingResource,
        ReturningResource,
        Idle
    };

    private State state = State::Idle;

    int currentStage = 0;

    [SerializeField]
    private A_Agent agentPathfinding;

    private Vector3 moveToTarget;
    private Vector3 returnTarget;

    private Resource resourceTarget;
    //how much value am I currently carrying
    private float carryingValue;

    private float actionTime = .0f;



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
        actionTime += Time.deltaTime;
        switch(state)
        {
            case State::UnderOrders:
                break;
            case State::ChasingResource:
                //continue to update position to follow target
                if (actionTime > pathUpdateStep)
                {
                    actionTime = .0f;
                    agentPathfinding.setPath(moveToTarget);
                }

                // TODO: need to modify distance to resource beforce collecting it
                if(Vector3.Distance(transform.position, moveToTarget) < 0.3f)
                {
                    // Get Resource
                    carryingValue = resourceTarget.value;

                    Destroy(resourceTarget.gameObject);
                    state = State::ReturningResources;
                    actionTime = .0f;
                }


                break;
            case State::ReturningResource:

                //TODO: can I do this less often on return trip?
                if (actionTime > pathUpdateStep)
                {
                    actionTime = .0f;
                    agentPathfinding.setPath(returnTarget);
                }


                if (Vector3.Distance(transform.position, moveToTarget) < 0.05f)
                {
                    associatedBase.addResources(carryingValue);
                    carryingValue = .0f;
                    state = State::Idle;
                    actionTime = .0f;
                }

                break;
            case State::Idle:

                //chill out

                //Movement
                transform.RotateAround(associatedBase.gameObject.transform.position, axis, orbitRoationSpeed * Time.deltaTime);
                Vector3 orbitDesiredPosition = (transform.position - associatedBase.gameObject.transform.position).normalized * radius + associatedBase.gameObject.transform.position;
                transform.position = Vector3.Slerp(transform.position, orbitDesiredPosition, Time.deltaTime * radiusCorrectionSpeed);

                //Rotation
                Vector3 relativePos = transform.position - previousPos;
                Quaternion rotation = Quaternion.LookRotation(relativePos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, radiusCorrectionSpeed * Time.deltaTime);
                previousPos = transform.position;


                break;

        }
    }

    public void setBase(Base b)
    {
        //this will do as a return target for now. Will need refining in the future.
        returnTarget = transform.position;


        associatedBase = b;
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
        resourceTarget = resource;
        moveTo = resourceTarget.gameObject.transform.position;
        state = State::ChasingResource;
    }

    public void moveTo(Vector3 target)
    {
        moveToTarget = target;
        agentPathfinding.setPath(target);
    }
}
