﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject baseUI;

    [SerializeField]
    Image resourceImage;

    [SerializeField]
    Text resourceText;

    [SerializeField]
    Image buildImage;

    [SerializeField]
    Text buildText;

    [SerializeField]
    Text tapTimeDisplayText;
    
    [SerializeField]
    Text tapBoostText;

    [SerializeField]
    Image tapFillBar;

    [SerializeField]
    BuildManager buildManager;

    [SerializeField]
    GameObject confirmUI;

	[SerializeField]
	GameObject revertUI;

    [SerializeField]
    GameObject buildNewUI;

    [SerializeField]
    GameObject hammerUI;

	[SerializeField]
	GameObject buyUI;

	[SerializeField]
	GameObject dollarUI;

	[SerializeField]
	Text spawnRateUpgradeText, resourceValueUpgradeText, unitReturnRateUpgradeText;

	public delegate void ConfirmDelegate();
    public ConfirmDelegate confirm;

	public delegate void RevertDelegate();
	public RevertDelegate revert;

	private float baseResource = 0;
    private float baseReqToUpdate = 0;
    private bool fullyUpgraded = false;
    private Base baseRef = null;

	private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
		gameManager = GameManager.Instance;
		spawnRateUpgradeText.text = "Resource Spawn Rate: " + gameManager.GetResourceSpawnRate() + "\nUpgrade (" + gameManager.spawnRateIncreaseCost + " U)";
		resourceValueUpgradeText.text = "Resource Value Multiplier: " + gameManager.GetResourceValueMultiplier() + "\nUpgrade (" + gameManager.resourceValueIncreaseCost + " U)";
		unitReturnRateUpgradeText.text = "Unit Return Rate: " + gameManager.GetUnitReturnRate() + "\nUpgrade (" + gameManager.unitReturnIncreaseCost + " U)";

	}

    // Update is called once per frame
    void Update()
    {

        if (!baseRef)
            return;

        UpdateUpgradeVisual();
        UpdateBuildVisual();
        UpdateTapBoostVisual();
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

		g = Mathf.Max(g, 0.2f);

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
        buildText.text = "Build (" + ReturnValueAstring(baseRef.reqToBuild) + ")";

        float percent = ((float)baseRef.heldResource / (float)baseRef.reqToBuild);
        float r = 1.0f - percent;
        float g = percent;

        buildImage.color = new Color(r, g, 0.0f, 1.0f);

        float s = Mathf.Clamp(g, 0.4f, 1.0f);
        buildImage.transform.localScale = new Vector3(s, s, s);

    }

    private void UpdateTapBoostVisual()
    {
        if (baseRef.tapSeconds <= 0)
            return;

        baseRef.tapSeconds -= Time.deltaTime;


        int hours = (int)Mathf.Floor(baseRef.tapSeconds / 3600.0f);
        int minutes = (int)Mathf.Floor(baseRef.tapSeconds / 60.0f) - (hours * 60);
        int seconds = (int)baseRef.tapSeconds % 60;

        string displayText = hours.ToString() + "h " + minutes.ToString() + "m " + seconds.ToString() + "s";

        tapTimeDisplayText.text = displayText;

        // its an integer value  2                                     57
        tapFillBar.fillAmount = ((float)minutes / 60.0f) + (((float)seconds / 60.0f) / 60.0f);

        baseRef.increasePercent = 10 * hours;
        tapBoostText.text = "Increased Resource Yield: " + baseRef.increasePercent + "%";
    }

    //For now you can only select bases
    public void EnableUI(Base b)
    {
        baseRef = b;
        RecalculateBuildReq();
		baseUI.SetActive(true);
        fullyUpgraded = baseRef.IsFullyUpgraded();
    }

    public void DisableUI()
    {
		revertUI.SetActive(false);
		confirmUI.SetActive(false);
		baseUI.SetActive(false);
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
        if (baseRef.heldResource < baseRef.reqToBuild)
            return;

        RecalculateBuildReq();

		// disable UI (except return arrow).
		baseUI.SetActive(false);

		//enable return arrow, add functions to revert delegate
		RevertDelegate revertable = DefaultStateBaseUIEnabled;
		revertable += buildManager.StopBuild;
		enableRevertUI(revertable);

        buildManager.BeginBuild(baseRef, false);
    }

	public void BuildFreshProcess()
	{
		RevertDelegate revertable = DefaultStatebuildEnabled;
		revertable += buildManager.StopBuild;
		enableRevertUI(revertable);
	}

    public void DefaultState()
    {
		DisableBuildUI();
		DisableBuyUI();
        revertUI.SetActive(false);
		confirmUI.SetActive(false);
		buildNewUI.SetActive(false);
		baseUI.SetActive(false);
    }

	public void DefaultStatebuildEnabled()
	{
		DefaultState();
		buildNewUI.SetActive(true);
	}

	public void DefaultStateBaseUIEnabled()
	{
		DefaultState();
		baseUI.SetActive(true);
	}

	public void EnableBuildUI()
    {
		DefaultState();
		buildNewUI.SetActive(true);
        hammerUI.SetActive(false);
    }

    public void DisableBuildUI()
    {
        buildNewUI.SetActive(false);
        hammerUI.SetActive(true);
    }

	public void EnableBuyUI()
	{
		DefaultState();
		buyUI.SetActive(true);
		dollarUI.SetActive(false);
	}

	public void DisableBuyUI()
	{
		
		buyUI.SetActive(false);
		dollarUI.SetActive(true);
	}

	public void SpawnRateIncrement()
	{
		if(gameManager.IncrementSpawnRate())
		{
			spawnRateUpgradeText.text = "Resource Spawn Rate: " + gameManager.GetResourceSpawnRate() + "\nUpgrade (" + gameManager.spawnRateIncreaseCost + " U)";
		}
	}

	public void ResourceValueIncrement()
	{
		if (gameManager.IncrementValueMultiplier())
		{
			resourceValueUpgradeText.text = "Resource Value Multiplier: " + gameManager.GetResourceValueMultiplier() + "\nUpgrade (" + gameManager.resourceValueIncreaseCost + " U)";
		}
	}

	public void UnitReturnRateIncrement()
	{
		if (gameManager.IncrementUnitReturn())
		{
			unitReturnRateUpgradeText.text = "Unit Return Rate: " + gameManager.GetUnitReturnRate() + "\nUpgrade (" + gameManager.unitReturnIncreaseCost + " U)";
		}
	}

	private void RecalculateBuildReq()
    {
		baseRef.reqToBuild = (int)Mathf.Pow((int)Mathf.Pow((float)baseRef.reqToUpgrade, 1.2f), (int)Mathf.Pow((float)(baseRef.numBuilds + 1), 1.2f));
    }

    public void addTapBoost()
    {
        // adds 3 minutes to tap boost time.
        baseRef.tapSeconds += 180.0f;
    }

    public void enableConfirmUI(ConfirmDelegate confirmable)
    {
        confirmUI.SetActive(true);
        confirm = confirmable;
    }

	public void enableRevertUI(RevertDelegate revertable)
	{
		revertUI.SetActive(true);
		revert = revertable;
	}

	public void onTapConfirm()
    {
        confirm?.Invoke();
    }

	public void onTapRevert()
	{
		revert?.Invoke();
	}
}
