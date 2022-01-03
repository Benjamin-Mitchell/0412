using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    UIManager _UIManager;

    [SerializeField]
    BuildManager buildManager;

    [SerializeField]
    GameObject cam;


    public bool inputEnabled = true;
    //
    private bool dragging;
    private Vector2 previousPos;
    private Vector3 camLookAt;
    //
    float distance,  prevDistance;
    float minDistance = 5.0f;
    float maxDistance = 25.0f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;
    Quaternion rotation;
    float previousTouchDistance;
    //
    bool traversing = false;
	bool targetMoves = false;
	GameObject traversalMovingTarget;

    Vector3 traversalTarget;
    float traversalDuration = 1.5f;
    float traversalTimePassed;
    float traversalSpeed;
    float traversalAngularSpeed;
    Vector3 traversalDirection;

    // Start is called before the first frame update
    void Start()
    {
        camLookAt = new Vector3(GameManager.Instance.mapX/2.0f,GameManager.Instance.mapY/2.0f,GameManager.Instance.mapZ/2.0f);

        distance = Vector3.Distance(cam.transform.position, camLookAt);
        
        cam.transform.LookAt(camLookAt);

        Vector3 angles = cam.transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;

        Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
        rotation = toRotation;
    }

    // Update is called once per frame
    void Update()
    {
		if (!traversing && inputEnabled)
        {
			//object tap control
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
			if (Input.GetMouseButtonDown(0) && !buildManager.building)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#elif (UNITY_ANDROID || UNITY_IOS)
            if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Ended))   //might need to modify this for zooming -> rotating change
            {
                   Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif
                RaycastHit hit;

				//don't allow object swapping if building.
				//TODO: Should a player be allowed to tap on the object to cancel building?
				if (!buildManager.building)
				{
					if (Physics.Raycast(ray, out hit))
					{
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {

                            if (hit.collider.CompareTag("Base"))
                            {
                                MoveCamTo(hit.collider.gameObject.GetComponentInParent<Base>());
                                //MoveCamTo(hit.collider.gameObject.transform.parent.GetComponent<Base>());
                                return;
                            }
                            else if (hit.collider.CompareTag("Agent"))
                            {
                                //colliders are on the individual agent stage objects (for size correctness)
                                //These are children of the actual agent object.
                                MoveCamTo(hit.collider.gameObject.GetComponentInParent<AgentStats>());
                                return;
                            }
                        }
					}
				}
            }
#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
            else if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
            {
                    dragging = true;
                    previousPos = getInputScreenPos(0);
            }
#endif

            //camera zoom control
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            distance -= Input.GetAxis("Mouse ScrollWheel") * 3.0f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
#elif (UNITY_ANDROID || UNITY_IOS)
            if(Input.touchCount == 2)
            {
                Vector2 pos0 = getInputScreenPos(0);
                Vector2 pos1 = getInputScreenPos(1);
                
                if(Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    previousTouchDistance = Mathf.Abs(Vector2.Distance(pos0, pos1));
                }
                else
                {
                    float touchDist = Mathf.Abs(Vector2.Distance(pos0, pos1));
                    float distanceDifference = (touchDist - previousTouchDistance) * Time.deltaTime;
                    distance -= distanceDifference;
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);
                    previousTouchDistance = touchDist;
                }
                
            }
            else{
                previousTouchDistance = 0.0f;
            }

#endif

            //camera rotation control
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            //On desktop, rotate by holding the right click.
            if (Input.GetMouseButtonDown(1))
            {
                dragging = true;
                previousPos = getInputScreenPos(0);
            }
#endif

            if (dragging)
            {
                //check if we need to stop moving the camera
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
                if (Input.GetMouseButtonUp(1))
#elif (UNITY_ANDROID || UNITY_IOS)
                if ((Input.touchCount != 1))    //should stop rotating if either 0 taps, or 2 taps (zoom)
#endif
                {
                    dragging = false;
                    return;
                }

                Vector2 currentPos = getInputScreenPos(0);
                Vector2 direction = currentPos - previousPos;

                previousPos = currentPos;

                rotationYAxis += direction.x;
                rotationXAxis -= direction.y;
                rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
                Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
                rotation = toRotation;
                cam.transform.rotation = rotation;
            }

            if (dragging || distance != prevDistance)
            {
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + camLookAt;
                cam.transform.position = position;
				prevDistance = distance;
            }

			if (targetMoves)
			{
				//Make sure the camera keeps following the moving target
				Vector3 positionDiff = traversalMovingTarget.transform.position - camLookAt;
				cam.transform.position += positionDiff;
				
				camLookAt = traversalMovingTarget.transform.position;
			}
		}
        else if (traversing)
        {
            //move towards traversal target within traversal time.

			//need to recalculate traversal if the target moves (like agents)
			if(targetMoves)
			{
				camLookAt = traversalMovingTarget.transform.position;
				CalcTraversalTarget();
			}

            float step = traversalSpeed * Time.deltaTime;
            float angularStep = traversalAngularSpeed * Time.deltaTime;

            cam.transform.position = Vector3.MoveTowards(cam.transform.position, traversalTarget, step);


            Vector3 newDirection = Vector3.RotateTowards(cam.transform.forward, traversalDirection, angularStep, 0.0f);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            cam.transform.rotation = Quaternion.LookRotation(newDirection);

            traversalTimePassed += Time.deltaTime;
            
            if (traversalTimePassed >= traversalDuration)
            {
                traversing = false;
                Vector3 angles = cam.transform.eulerAngles;
                rotationYAxis = angles.y;
                rotationXAxis = angles.x;
            
                Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
                rotation = toRotation;
            }
        }
	}

	public void MoveCamTo(Base b)
	{
		if (_UIManager)
			_UIManager.EnableUI(b);

		traversing = true;
		targetMoves = false;
		camLookAt = b.gameObject.transform.position;
		traversalTimePassed = 0.0f;

		CalcTraversalTarget();
	}

	public void MoveCamTo(AgentStats stats)
	{
		if (_UIManager)
			_UIManager.EnableUI(stats);

		traversing = true;
		targetMoves = true;
		camLookAt = stats.gameObject.transform.position;
		traversalMovingTarget = stats.gameObject;
		traversalTimePassed = 0.0f;

		CalcTraversalTarget();
	}

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }


    public Vector2 getInputScreenPos(int n)
    {
        Vector3 pos;
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        pos = Input.mousePosition;
#elif (UNITY_ANDROID || UNITY_IOS)
        pos = Input.GetTouch(n).position;
#endif
        Vector2 retPos = new Vector2(pos.x, pos.y);
        return retPos;
    }

    void CalcTraversalTarget()
    {
        Vector3 target;
        traversalDirection = camLookAt - cam.transform.position;

        //calculate future look rotation
        rotation = Quaternion.LookRotation(traversalDirection);

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + camLookAt;
        target = position;

        traversalTarget = target;

        traversalSpeed = Vector3.Distance(cam.transform.position, target)/(traversalDuration - traversalTimePassed);

        traversalAngularSpeed = (Vector3.Angle(cam.transform.forward, traversalDirection) * Mathf.Deg2Rad)/(traversalDuration - traversalTimePassed);
    }
}


