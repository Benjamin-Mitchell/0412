using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    // Start is called before the first frame update

    public bool building = false;
    GameObject beingBuilt;
    Base beingBuiltBaseComp;

    [SerializeField]
    GameObject buildSphere;

    [SerializeField]
    GameObject assetToBuild; //need to change this to pull a dynamically allocated prefab.

    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    UIManager _UIManager;

    //Vector3 previousPos
    bool pickedBuildTarget = false;
    float rotationSpeed = 1.0f;

    private Base referanceBase;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (building)
        {
            bool hit = false;
			beingBuilt.transform.Rotate(new Vector3(0.2f, 0.2f, 0.0f));

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            // if desktop, show where it will be placed.
            Vector3 point = GetBuildSpherePos(true, ref hit);

#elif (UNITY_ANDROID || UNITY_IOS)
            if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Vector3 point = GetBuildSpherePos(true, ref hit);
#endif
            if (hit && !pickedBuildTarget)
            {
                beingBuilt.transform.position = point;
                beingBuiltBaseComp.baseStages[beingBuiltBaseComp.stage].SetActive(true);

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
                if (Input.GetMouseButtonDown(0))
                {
#endif
                    _UIManager.enableConfirmUI(FinishBuild);
                    pickedBuildTarget = true;
                    //TODO: some text to display how to rotate?
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
                }
#endif
                //NEED TO DISABLE BASE FUNCTIONALITY UNTIL BUILT!!

                //TODO: APPLY A COOL EFFECT HERE (WHILE BUILDING EFFECT)

            }
#if (UNITY_ANDROID || UNITY_IOS) && !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
            }
#endif

            //if (pickedBuildTarget
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            //        && Input.GetMouseButton(0)
#elif (UNITY_ANDROID || UNITY_IOS)
            //        && (Input.touchCount > 0)
#endif
            //)
            //{
            //    float XaxisRotation = Input.GetAxis("Mouse X") * rotationSpeed;
            //    float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSpeed;
            //    // select the axis by which you want to rotate the GameObject
            //    beingBuilt.transform.Rotate(Vector3.down, XaxisRotation);
            //    beingBuilt.transform.Rotate(Vector3.right, YaxisRotation);
            //}
        }
    }

    private Vector3 GetBuildSpherePos(bool livePos, ref bool success)
    {
        success = false;
        Vector3 result = Vector3.zero;
        Ray ray;

        if (!livePos)
            ray = Camera.main.ScreenPointToRay(new Vector3(0.5f, 0.5f, 0f));
        else
        {
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            // if desktop, show where it will be placed.
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#elif (UNITY_ANDROID || UNITY_IOS)
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif
        }

        RaycastHit hit;
        int layerMask = ~ LayerMask.GetMask("Base");
        if (Physics.Raycast(ray, out hit, 150.0f, layerMask))
        {
            if (hit.collider.gameObject.name == "BuildSphere")
            {
                result = hit.point;
                success = true;
            }
        }

        return result;
    }

    public void BeginBuild(Vector3 basePos, Base refBase)
    {
        // enable sphere to show range. give it appropriate scale based on build radius.
        buildSphere.SetActive(true);
        buildSphere.transform.localScale = new Vector3(15.0f, 15.0f, 15.0f);
        buildSphere.transform.position = basePos;

        bool hit = false;
        Vector3 spawnPos = GetBuildSpherePos(false, ref hit);

		assetToBuild = Resources.Load("Base_" + refBase.baseType) as GameObject;

        beingBuilt = Instantiate(assetToBuild, spawnPos, Quaternion.identity);
        beingBuiltBaseComp = beingBuilt.GetComponent<Base>();
        int newStage = refBase.stage - 1;
        beingBuiltBaseComp.stage = newStage;
        beingBuiltBaseComp.baseStages[0].SetActive(false);
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        beingBuiltBaseComp.baseStages[newStage].SetActive(true);
        beingBuiltBaseComp.enabled = false;
#endif

        referanceBase = refBase;

        building = true;
    }

    public void StopBuild()
    {
        building = false;

        buildSphere.SetActive(false);
        Destroy(beingBuilt);

		pickedBuildTarget = false;
		beingBuilt = null;
    }

    void FinishBuild()
    {
        building = false;
        buildSphere.SetActive(false);
        
        beingBuiltBaseComp.enabled = true;
        beingBuilt.transform.eulerAngles = new Vector3(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f));

		Base b = beingBuilt.GetComponent<Base>();
		b.heldResource -= b.reqToBuild;

		_UIManager.DefaultState();
        referanceBase.numBuilds++;
        beingBuiltBaseComp.numBuilds = referanceBase.numBuilds;
        pickedBuildTarget = false;

		beingBuilt = null;
	}
}
