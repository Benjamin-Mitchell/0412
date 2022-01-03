using System;
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

	int nextBaseID = 0;

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

	void BuildBase(GameObject asset, Vector3 pos, out Base b)
	{
		beingBuilt = Instantiate(asset, pos, Quaternion.identity);
		b = beingBuilt.GetComponent<Base>();
		//allBases.Add(b);

		b.ID = nextBaseID;
		nextBaseID++;
	}

	public void LoadBases(List<SaveManager.BaseData> bases, TimeSpan difference, int totalAgents)
	{
		gameManager = GameManager.Instance;
		Debug.Log("Loading Bases! bases count: " + bases.Count);

		ResourceSpawner resourceSpawner = GameObject.Find("ResourceSpawner").GetComponent<ResourceSpawner>();

		float totalRes = 0.0f;
		for (int i = 0; i < bases.Count; i++)
		{
			SaveManager.BaseData data = bases[i];

			GameObject asset = Resources.Load("Base_" + data.baseTypeString) as GameObject;

			BuildBase(asset, data.position, out Base b);
			allBases.Add(b);
			b.LoadSetup(data, difference, resourceSpawner.spawnRate, totalAgents, out float maxResourcesCollected);
			totalRes += maxResourcesCollected;
		}

		//total available spawns
		float A = (100 / (resourceSpawner.spawnRate / gameManager.resourceSpawner.resourceSpawnRateUpgrade) / (float)totalAgents);

		float ratioAvailable = A / totalRes;

		if (ratioAvailable > 1.0f)
		{
			ratioAvailable = 1.0f;
		}
		else
		{
			Debug.Log("Not enough resources available for maximum returns. Upgrade Resource spawn rate!");
		}

		//Load setup 2 needs information gathered from all bases/agents to calculate gains.
		for (int i = 0; i < allBases.Count; i++)
		{
			allBases[i].LoadSetupFinalize(ratioAvailable);
		}
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

		
		BuildBase(assetToBuild, defaultStartPosition, out Base b);
		//beingBuilt = Instantiate(assetToBuild, defaultStartPosition, Quaternion.identity);
		allBases.Add(b);

        _UIManager.DisableBuildUI();

        //TODO: Remove/rework this for actual introduction.
        EndIntroBuild();
    }


    void EndIntroBuild()
    {
        gameManager.SetResourceSpawnRates(0);
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
            beingBuilt.transform.Rotate(new Vector3(0.1f, 0.1f, 0.0f));

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            // if desktop, show where it will be placed.
            Vector3 point = GetBuildSpherePos(true, ref hit);
            //Debug.Log(point);

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
            Debug.Log(hit.collider.gameObject.name);
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

		string s = isFreshBuild ? baseType : refBase.baseTypeString;
		assetToBuild = Resources.Load("Base_" + s) as GameObject;

		//beingBuilt = Instantiate(assetToBuild, spawnPos, Quaternion.identity);
		BuildBase(assetToBuild, spawnPos, out beingBuiltBaseComp);

		int newStage = isFreshBuild ? 0 : refBase.stage - 1;
        beingBuiltBaseComp.stage = newStage;

        beingBuiltBaseComp.baseStages[0].SetActive(false);

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        beingBuiltBaseComp.baseStages[newStage].SetActive(true);
        beingBuiltBaseComp.enabled = false;
#endif
		foreach(Base b in allBases)
		{
			b.buildSphere.SetActive(true);
            //build sphere size is flat value (15.0f) + (new_stage * 3.0f);
            float x = 15.0f + ((float)b.stage * 3.0f);
            b.buildSphere.transform.localScale = new Vector3(x, x, x);
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

		foreach(Base b in allBases) { b.buildSphere.SetActive(false); }

		beingBuiltBaseComp.enabled = true;
		allBases.Add(beingBuiltBaseComp);

		//update sphere position for new base (in its final position)
		beingBuiltBaseComp.buildSphere.transform.position = beingBuiltBaseComp.gameObject.transform.position;

        beingBuilt.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0.0f, 360.0f), UnityEngine.Random.Range(0.0f, 360.0f), UnityEngine.Random.Range(0.0f, 360.0f));

        if (!isFreshBuild)
            referanceBase.DeductResources(false); //false: isUpgrade

		inputManager.MoveCamTo(beingBuilt.GetComponent<Base>());

		_UIManager.DisableUI();

		if (!isFreshBuild)
		{
			referanceBase.numBuilds++;
			beingBuiltBaseComp.numBuilds = referanceBase.numBuilds;
		}

        if (isFreshBuild)
        {
            gameManager.SetResourceSpawnRates(beingBuiltBaseComp.baseType);
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
