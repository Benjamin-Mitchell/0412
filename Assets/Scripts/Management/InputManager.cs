using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    UIManager _UIManager;

    [SerializeField]
    GameObject cam;
    //
    private bool dragging;
    private Vector2 previousPos;
    private Vector3 camLookAt;
    //
    float distance, previousDistance;
    float minDistance = 5.0f;
    float maxDistance = 25.0f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;
    Quaternion rotation;
    float previousTouchDistance;

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

        //object tap control
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        if (Input.GetMouseButtonDown(0)/*Input.GetMouseButtonUp(0)*/)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#elif (UNITY_ANDROID || UNITY_IOS)
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Ended /*TouchPhase.Began*/))   //might need to modify this for zooming -> rotating change
        {
               Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Base"))
                {
                    _UIManager.EnableUI(hit.collider.gameObject.GetComponent<Base>());   
                }
            }
        }
#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
        else if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began /*TouchPhase.Began*/))
        {/*if ((Input.touchCount > 1) && (!dragging))*/
                dragging = true;
                previousPos = getInputScreenPos(0);
        }
#endif

        //camera zoom control
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        distance -= Input.GetAxis("Mouse ScrollWheel");
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
                distance += distanceDifference;
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

        if(dragging)
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

        if(dragging || distance != previousDistance)
        {
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + camLookAt;
            cam.transform.position = position;
        }
    }


    public static float ClampAngle(float angle, float min, float max)
    {
         if (angle < -360F)
             angle += 360F;
         if (angle > 360F)
             angle -= 360F;
         return Mathf.Clamp(angle, min, max);
    }

    
    Vector2 getInputScreenPos(int n)
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
}
