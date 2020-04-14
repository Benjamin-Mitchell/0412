using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject UI_object;

    [SerializeField]
    GameObject backArrow;

    [SerializeField]
    Image resourceImage;

    [SerializeField]
    Text resourceText;

    [SerializeField]
    Image buildImage;

    [SerializeField]
    Text buildText;

    [SerializeField]
    BuildManager buildManager;

    float baseResource = 0;
    float baseReqToUpdate = 0;
    bool fullyUpgraded = false;
    Base baseRef = null;

    int buildReq;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (!baseRef)
            return;

        UpdateUpgradeVisual();
        UpdateBuildVisual();
    }

    private void UpdateUpgradeVisual()
    {
        resourceText.text = ReturnValueAstring(baseRef.heldResource);

        if (fullyUpgraded)
        {
            resourceImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            resourceImage.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            return;
        }

        baseResource = (float)baseRef.heldResource;
        baseReqToUpdate = (float)baseRef.reqToUpgrade;

        float percent = (baseResource / baseReqToUpdate);
        percent = Mathf.Clamp(percent, 0.0f, 1.0f);

        float r = 1.0f - percent;
        float g = percent;

        float a = 0.5f + (percent / 2.0f);
        resourceImage.color = new Color(r, g, 0.0f, a);

        resourceImage.transform.localScale = new Vector3(g, g, g);
    }

    private void UpdateBuildVisual()
    {
        if(baseRef.stage == 0)
        {
            buildImage.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            buildText.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            return;
        }

        buildText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        // costs reqToUpgrade ^ 2;
        buildText.text = "Build (" + ReturnValueAstring(buildReq) + ")";

        float percent = ((float)baseRef.heldResource / (float)buildReq);
        float r = 1.0f - percent;
        float g = percent;

        buildImage.color = new Color(r, g, 0.0f, 1.0f);

        float s = Mathf.Clamp(g, 0.4f, 1.0f);
        buildImage.transform.localScale = new Vector3(s, s, s);

    }

    //For now you can only select bases
    public void EnableUI(Base b)
    {
        baseRef = b;
        RecalculateBuildReq();
        UI_object.SetActive(true);
        fullyUpgraded = baseRef.IsFullyUpgraded();
    }

    public void DisableUI()
    {
        UI_object.SetActive(false);
        fullyUpgraded = false;
    }

    public string ReturnValueAstring(int value)
    {
        string s = value.ToString();
        return s;
    }

    public void UpgradeBase()
    {
        if (baseRef.heldResource >= baseRef.reqToUpgrade)
        {
            baseRef.UpgradeBase();
            fullyUpgraded = baseRef.IsFullyUpgraded();
        }
        RecalculateBuildReq();
    }

    public void BuildProcess()
    {
        if (baseRef.heldResource < buildReq)
            return;

        RecalculateBuildReq();

        baseRef.heldResource -= buildReq;

        // disable UI (except return arrow).
        UI_object.SetActive(false);
        backArrow.SetActive(true);

        buildManager.BeginBuild(baseRef.transform.position, baseRef);
    }

    public void DefaultState()
    {
        backArrow.SetActive(false);
        UI_object.SetActive(true);
        buildManager.StopBuild();
    }

    private void RecalculateBuildReq()
    { 
        buildReq = (int)Mathf.Pow((int)Mathf.Pow((float)baseRef.reqToUpgrade, 1.2f), (int)Mathf.Pow((float)(baseRef.numBuilds + 1), 1.2f));
    }
   
}
