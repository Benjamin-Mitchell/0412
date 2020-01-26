using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGameplay : MonoBehaviour
{
    public GameObject[] agentStages = new GameObject[5];
    public Base associatedBase;

    //How often should the path information be reset (in seconds)
    public float pathUpdateStep;

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
    private A_Agent agentPathfinding;

    private Resource moveToTarget;
    private Vector3 returnTarget;
    private Vector3 spawnTarget;

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
            case State.UnderSpawnOrders:
                if (Vector3.Distance(transform.position, spawnTarget) < 0.1f)
                {
                    agentPathfinding.nullifyPath();
                    state = State.Idle;
                }

                break;
            case State.ChasingResource:

                //TODO: check if resource is still present, or pick one that is guaranteed to still be in range.
                

                // TODO: need to modify distance to resource beforce collecting it
                if (Vector3.Distance(transform.position, moveToTarget.transform.position) < 0.3f)
                {
                    // Get Resource
                    carryingValue = resourceTarget.value;
                
                    Destroy(resourceTarget.gameObject);
                    state = State.ReturningResource;
                    actionTime = .0f;
                }
                
                
                //do a ray-cast first, this is more optimal if possible.
                RaycastHit hit;
                if(Physics.Raycast(transform.position, moveToTarget.transform.position - transform.position, out hit))
                {
                    // target is in direct vision
                    if (hit.collider.gameObject == moveToTarget.gameObject)
                    {
                        actionTime = pathUpdateStep;
                        agentPathfinding.setSingleNodePath(moveToTarget.transform.position);
                        break;
                    }
                    
                }
                

                //continue to update position to follow target
                if (actionTime >= pathUpdateStep)
                {
                    actionTime = .0f;
                    agentPathfinding.setPath(moveToTarget.transform.position);
                    
                }

                

                break;
            case State.ReturningResource:

                if (Vector3.Distance(transform.position, returnTarget) < 0.05f)
                {
                    associatedBase.addResources(carryingValue);
                    carryingValue = .0f;
                    state = State.Idle;
                    actionTime = .0f;
                }

                // if the ray hits nothing, it's a clear path
                if (!Physics.Raycast(transform.position, returnTarget - transform.position, Vector3.Distance(returnTarget, transform.position)))
                {
                    actionTime = pathUpdateStep;
                    agentPathfinding.setSingleNodePath(returnTarget);
                    break;
                }


                //TODO: can I do this less often on return trip?
                if (actionTime > pathUpdateStep)
                {
                    actionTime = .0f;
                    agentPathfinding.setPath(returnTarget);
                }


                

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
        // make sure a movement check is done immediately;
        actionTime = pathUpdateStep;


        resourceTarget = resource;
        moveToTarget = resourceTarget;
        state = State.ChasingResource;
    }

    public void moveTo(Vector3 target)
    {
        spawnTarget = target;
        agentPathfinding.setPath(target);
        state = State.UnderSpawnOrders;
    }
}
