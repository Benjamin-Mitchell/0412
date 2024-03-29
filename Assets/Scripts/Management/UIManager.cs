﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField]
	GameObject baseUI;

	[SerializeField]
	Text baseNameText;

	[SerializeField]
	GameObject agentUI;

	[SerializeField]
	GameObject agentRenameUI;

	[SerializeField]
	InputField agentRenameInputField; 

	[SerializeField]
	Image resourceImage;

	[SerializeField]
	Text upgradeText;

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
	Text agentNameText;

	[SerializeField]
	Text resourceCollectedText;

	[SerializeField]
	Text distanceTravelledText;
	
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
	GameObject enableUIButton;

	[SerializeField]
	Text spawnRateUpgradeText, resourceValueUpgradeText, unitReturnRateUpgradeText, MaxInactiveUpgradeText;

	[SerializeField]
	GameObject []BuyUIPages;

	[SerializeField]
	Image[] valueImages = new Image[5];

	private float[] valueImagesStartY = new float[6] { -155.0f, -147.5f, -140.0f, -132.5f, -125.0f, -117.5f};

	private int buyUICurrentPage = 0;

	public delegate void ConfirmDelegate();
	public ConfirmDelegate confirm;

	public delegate void RevertDelegate();
	public RevertDelegate revert;

	//private Value baseResource = 0;
	//private Value baseReqToUpdate = 0;
	private bool fullyUpgraded = false;
	public Base baseRef = null;

	public AgentStats agentRef = null;

	private GameManager gameManager;

	public enum UIACTIVE {Base, Agent, none};
	UIACTIVE active = UIACTIVE.none;
	public UIACTIVE targeted = UIACTIVE.none;

    // Start is called before the first frame update
    void Start()
    {
		gameManager = GameManager.Instance;
		spawnRateUpgradeText.text = "Resource Spawn Rate: " + gameManager.resourceSpawner.resourceSpawnRateUpgrade + "\nUpgrade (" + gameManager.spawnRateIncreaseCost.GetStringVal() + " U)";
		resourceValueUpgradeText.text = "Resource Value Multiplier: " + gameManager.resourceValueMultiplier + "\nUpgrade (" + gameManager.resourceValueIncreaseCost.GetStringVal() + " U)";
		unitReturnRateUpgradeText.text = "Unit Return Rate: " + gameManager.unitReturnRate + "\nUpgrade (" + gameManager.unitReturnIncreaseCost.GetStringVal() + " U)";
		MaxInactiveUpgradeText.text = "Max Period Inactive: " + (gameManager.maxPeriodInactive * 100 / 60) + " minutes\nUpgrade (" + gameManager.maxInactiveIncreaseCost.GetStringVal() + " U)";

	}

    // Update is called once per frame
    void Update()
    {
		switch(active)
		{
			case UIACTIVE.Base:
				if (!baseRef)
					return;

				UpdateUpgradeVisual();
				UpdateBuildVisual();
				UpdateTapBoostVisual();
				break;
			case UIACTIVE.Agent:
				UpdateAgentVisuals();
				break;
			case UIACTIVE.none:

				break;
		}
    }

    private void UpdateUpgradeVisual()
    {
		string temp = "";
		for(int i = 0; i < baseRef.heldResources.Length; i++)
		{
			temp += baseRef.heldResources[i].GetStringVal() + "/" + baseRef.reqsToUpgrade[i].GetStringVal() + "\n";

		}
		upgradeText.text = temp;

		//place the value images in correct places.
		PlaceValueImages();


        if (fullyUpgraded)
        {
            resourceImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            resourceImage.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            return;
        }

		Value baseResource = Value.GetSumOfArray(baseRef.heldResources);
		Value baseReqToUpdate = Value.GetSumOfArray(baseRef.reqsToUpgrade);


		float percent = (baseResource / baseReqToUpdate).ToFloat();
        percent = Mathf.Clamp(percent, 0.0f, 1.0f);

        float r = 1.0f - percent;
        float g = percent;

        float a = 0.5f + (percent / 2.0f);
        resourceImage.color = new Color(r, g, 0.0f, a);

		g = Mathf.Max(g, 0.2f);

        resourceImage.transform.localScale = new Vector3(g, g, g);
    }

	private void PlaceValueImages()
	{
		int numImages = baseRef.heldResources.Length;
		float iteration = 15.0f;
		float startY = valueImagesStartY[numImages];

		for (int i = 0; i < baseRef.heldResources.Length; i++)
		{
			valueImages[i].rectTransform.anchoredPosition = new Vector2(valueImages[i].rectTransform.anchoredPosition.x, startY - (iteration * i));
		}
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

		string temp = "";
		for (int i = 0; i < baseRef.heldResources.Length; i++)
		{
			temp += baseRef.heldResources[i].GetStringVal() + "/" + baseRef.reqsToBuild[i].GetStringVal() + "\n";

		}
		buildText.text = temp;

		Value baseResource = Value.GetSumOfArray(baseRef.heldResources);
		Value baseReqToBuild = Value.GetSumOfArray(baseRef.reqsToBuild);

		float percent = (baseResource / baseReqToBuild).ToFloat();
        float r = 1.0f - percent;
        float g = percent;

        buildImage.color = new Color(r, g, 0.0f, 1.0f);

        float s = Mathf.Clamp(g, 0.4f, 1.0f);
        buildImage.transform.localScale = new Vector3(s, s, s);

    }

    private void UpdateTapBoostVisual()
    {
		if(baseRef.tapSeconds > Time.deltaTime)
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

	private void UpdateAgentVisuals()
	{
		agentNameText.text = agentRef.agentName;
		resourceCollectedText.text = "Resource Collected: " + agentRef.resourceCollected.GetStringVal();
		distanceTravelledText.text = "Distance Travelled: " + agentRef.distanceTravelled.GetStringVal();
	}

	public void EnableRenameAgentUI()
	{
		agentUI.SetActive(false);
		baseUI.SetActive(false);
		agentRenameUI.SetActive(true);
	}

	public void DisableRenameAgentUI(bool renamed)
	{
		if (active == UIACTIVE.Agent)
			agentUI.SetActive(true);
		else if (active == UIACTIVE.Base)
			baseUI.SetActive(true);
		agentRenameUI.SetActive(false);

		if (renamed)
		{
			if (agentRenameInputField.text.Length < 24)
			{
				if (active == UIACTIVE.Agent)
				{
					agentRef.agentName = agentRenameInputField.text;
					agentNameText.text = agentRef.agentName;
				}
				else if(active == UIACTIVE.Base)
				{
					baseRef.baseName = agentRenameInputField.text;
					baseNameText.text = baseRef.baseName;
				}
			}
			else
			{

			}
		}
	}

	public void EnableUI()
	{
		if (targeted == UIACTIVE.Base)
			EnableUI(baseRef);
		else if (targeted == UIACTIVE.Agent)
			EnableUI(agentRef);
		else if (targeted == UIACTIVE.none)
			Debug.Log("NONE");

		enableUIButton.SetActive(false);
	}

	public void EnableUI(Base b)
	{
		DefaultState();
		active = UIACTIVE.Base;
        baseRef = b;
		baseNameText.text = baseRef.baseName;
        baseRef.RequiredToBuild();
		baseUI.SetActive(true);
		for(int i = 0; i < valueImages.Length; i++)
		{
			if (i <= baseRef.baseType)
				valueImages[i].gameObject.SetActive(true);
			else
				valueImages[i].gameObject.SetActive(false);
		}
        fullyUpgraded = baseRef.IsFullyUpgraded();
    }

	public void EnableUI(AgentStats a)
	{
		DefaultState();
		active = UIACTIVE.Agent;
		agentRef = a;
		agentUI.SetActive(true);
	}

    public void DisableUI()
    {
		active = UIACTIVE.none;
		revertUI.SetActive(false);
		confirmUI.SetActive(false);
		baseUI.SetActive(false);
		agentUI.SetActive(false);
		enableUIButton.SetActive(true);
        fullyUpgraded = false;
    }

    public string ReturnValueAstring(int value)
    {
        string s = value.ToString();
        return s;
    }

    public void UpgradeBase()
    {
		if (baseRef.UpgradeBase())
		{
			fullyUpgraded = baseRef.IsFullyUpgraded();
			baseRef.RequiredToBuild();
		}
	}

	public void BuildProcess()
    {
		if (!baseRef.CanBuildBase())
			return;

        baseRef.RequiredToBuild();

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
		DisableBuildUI();
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
		agentUI.SetActive(false);
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

	public void IncrementBuyPageNumber()
	{
		BuyUIPages[buyUICurrentPage].SetActive(false);
		buyUICurrentPage++;
		BuyUIPages[buyUICurrentPage].SetActive(true);
	}

	public void DecrementBuyPageNumber()
	{
		BuyUIPages[buyUICurrentPage].SetActive(false);
		buyUICurrentPage--;
		BuyUIPages[buyUICurrentPage].SetActive(true);
	}

	public void SpawnRateIncrement()
	{
		if(gameManager.IncrementSpawnRate())
		{
			spawnRateUpgradeText.text = "Resource Spawn Rate: " + gameManager.resourceSpawner.resourceSpawnRateUpgrade + "\nUpgrade (" + gameManager.spawnRateIncreaseCost + " U)";
		}
	}

	public void ResourceValueIncrement()
	{
		if (gameManager.IncrementValueMultiplier())
		{
			resourceValueUpgradeText.text = "Resource Value Multiplier: " + gameManager.resourceValueMultiplier + "\nUpgrade (" + gameManager.resourceValueIncreaseCost + " U)";
		}
	}

	public void UnitReturnRateIncrement()
	{
		if (gameManager.IncrementUnitReturn())
		{
			unitReturnRateUpgradeText.text = "Unit Return Rate: " + gameManager.unitReturnRate + "\nUpgrade (" + gameManager.unitReturnIncreaseCost + " U)";
		}
	}

	public void MaxPeriodInactiveIncrement()
	{
		if(gameManager.IncrementMaxInactive())
		{
			//convert X number of 100Seconds to minutes.
			MaxInactiveUpgradeText.text = "Max Period Inactive: " + (gameManager.maxPeriodInactive * 100 / 60) + " minutes\nUpgrade (" + gameManager.maxInactiveIncreaseCost + " U)";
		}
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
