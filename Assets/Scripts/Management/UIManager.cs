using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject UI_object;

    [SerializeField]
    Image resourceImage;

    [SerializeField]
    Text resourceText;


    float baseResource = 0;
    float baseReqToUpdate = 0;
    bool fullyUpgraded = false;
    Base baseRef = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!baseRef)
            return;


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

    //For now you can only select bases
    public void EnableUI(Base b)
    {
        UI_object.SetActive(true);
        baseRef = b;
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
    }
}
