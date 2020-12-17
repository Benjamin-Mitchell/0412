using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
	public List<Base> allBases = new List<Base>();

	public bool building = false;
	GameObject beingBuilt;
    Base beingBuiltBaseComp;

    [SerializeField]
    GameObject assetToBuild; //need to change this to pull a dynamically allocated prefab.

    [SerializeField]
    InputManager inputManager;

    GameManager gameManager;

    [SerializeField]
    UIManager _UIManager;

    //Vector3 previousPos
    bool pickedBuildTarget = false;
    float rotationSpeed = 1.0f;

    private Base referanceBase;
    void Start()
    {
        gameManager = GameManager.Instance;

        if (!gameManager.finishedIntroduction)
        {
            assetToBuild = (GameObject)Resources.Load("Base_Dirty", typeof(GameObject));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.finishedIntroduction)
            IntroLoop();
        else
            DefaultPlayLoop();
    }

    /// <summary>
    /// INTRO SECTION START
    /// </summary>

    void IntroLoop()
    {
        if(building)
        {

        }
    }

    public void BeginIntroBuild()
    {
        Vector3 defaultStartPosition = new Vector3(gameManager.mapX / 2.0f, gameManager.mapY / 2.0f, gameManager.mapZ / 2.0f);
        beingBuilt = Instantiate(assetToBuild, defaultStartPosition, Quaternion.identity);
		allBases.Add(beingBuilt.GetComponent<Base>());

        _UIManager.DisableBuildUI();

        //TODO: Remove/rework this for actual introduction.
        EndIntroBuild();
    }


    void EndIntroBuild()
    {

        //TODO: Remove/rework this for actual introduction.
        gameManager.finishedIntroduction = true;
    }


    /// <summary>
    /// INTRO SECTION END
    /// </summary>

    /// <summary>
    /// DEFAULT PLAY SECTION START
    /// </summary>


    void DefaultPlayLoop()
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
            if (hit.collider.gameObject.name.Contains("BuildSphere"))
            {
                result = hit.point;
                success = true;
            }
        }

        return result;
    }

	public void BeginBuild(Base refBase, bool isFreshBuild, string baseType = "")
	{
		bool hit = false;
		Vector3 spawnPos = GetBuildSpherePos(false, ref hit);

		string s = isFreshBuild ? baseType : refBase.baseType;
		assetToBuild = Resources.Load("Base_" + s) as GameObject;

		beingBuilt = Instantiate(assetToBuild, spawnPos, Quaternion.identity);
		beingBuiltBaseComp = beingBuilt.GetComponent<Base>();

		int newStage = isFreshBuild ? 0 : refBase.stage - 1;
        beingBuiltBaseComp.stage = newStage;

        beingBuiltBaseComp.baseStages[0].SetActive(false);

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        beingBuiltBaseComp.baseStages[newStage].SetActive(true);
        beingBuiltBaseComp.enabled = false;
#endif
		if (isFreshBuild)
		{
			foreach(Base b in allBases)
			{
				b.buildSphere.SetActive(true);
				b.buildSphere.transform.localScale = new Vector3(15.0f, 15.0f, 15.0f);
			}
		}
		else
		{
			// enable sphere to show range. give it appropriate scale based on build radius.
			refBase.buildSphere.SetActive(true);

			//need to organize individual sphere scales (factor in base type, base stage and isFreshBuild)
			refBase.buildSphere.transform.localScale = new Vector3(15.0f, 15.0f, 15.0f);
		}

        referanceBase = refBase;

		building = true;


		//Reset input to prevent current mouse down from triggering next build stage. 
		Input.ResetInputAxes();
    }

    public void StopBuild()
    {
        building = false;

		//there can be no refeance base when we are building a fresh base. 
		foreach(Base b in allBases)
			b.buildSphere.SetActive(false);

        Destroy(beingBuilt);

		pickedBuildTarget = false;
		beingBuilt = null;
    }

    void FinishBuild()
    {
		//if its a fresh build, there was never a referance base assigned.
		bool isFreshBuild = referanceBase == null;
		
		building = false;

		if(isFreshBuild)
			foreach(Base b in allBases) { b.buildSphere.SetActive(false); }
		else
			referanceBase.buildSphere.SetActive(false);

		beingBuiltBaseComp.enabled = true;
		allBases.Add(beingBuiltBaseComp);

		//update sphere position for new base (in its final position)
		beingBuiltBaseComp.buildSphere.transform.position = beingBuiltBaseComp.gameObject.transform.position;

        beingBuilt.transform.eulerAngles = new Vector3(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f));

		if(!isFreshBuild)
			referanceBase.HeldResource -= referanceBase.ReqToBuild;

		inputManager.MoveCamTo(beingBuilt.GetComponent<Base>());

		_UIManager.DisableUI();

		if (!isFreshBuild)
		{
			referanceBase.numBuilds++;
			beingBuiltBaseComp.numBuilds = referanceBase.numBuilds;
		}
        pickedBuildTarget = false;

		beingBuilt = null;
	}

	//TODO: Update with new types when they are ready.
	string[] types = new string[5] { "Dirty", "Pie", "Seller", "Proxy", "Teabag" };
	public void BuildFreshBase(int baseTypeIndex)
	{

		if(!gameManager.finishedIntroduction)
		{
			BeginIntroBuild();
		}
		else
		{
			BeginBuild(null, true, types[baseTypeIndex]);
			_UIManager.BuildFreshProcess();
		}
	}
}
