using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    // Start is called before the first frame update

    public bool building = false;
    GameObject beingBuilt;

    [SerializeField]
    GameObject buildSphere;

    [SerializeField]
    GameObject toBuild; //need to change this to pull a dynamically allocated prefab.

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
            Vector3 point = GetBuildSpherePos(true, ref hit);

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            // if desktop, show where it will be placed.
            if (hit)
            {
                beingBuilt.transform.position = point;
                if (Input.GetMouseButtonDown(0))
                    FinishBuild();
            }
#elif (UNITY_ANDROID || UNITY_IOS)
            // IMPLEMENT
            // if mobile, allow users to tap where they might want it,
#endif


            //add a "build?" check.
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
            //IMPLEMENT

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

        toBuild = Resources.Load("Base_" + refBase.baseType) as GameObject;

        beingBuilt = Instantiate(toBuild, spawnPos, Quaternion.identity);
        Base b = beingBuilt.GetComponent<Base>();
        b.enabled = false;

        referanceBase = refBase;

        building = true;
    }

    public void StopBuild()
    {
        building = false;
        buildSphere.SetActive(false);
        Destroy(toBuild);
    }

    void FinishBuild()
    {
        building = false;
        buildSphere.SetActive(false);
        Base b = beingBuilt.GetComponent<Base>();
        b.enabled = true;

        referanceBase.numBuilds++;
        b.numBuilds = referanceBase.numBuilds;
    }
}
