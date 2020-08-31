using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseUIDelegator : MonoBehaviour
{
	[SerializeField]
	int baseTypeIndex;

	EventTrigger trigger;

	[SerializeField]
	BuildManager buildManager;


	// Start is called before the first frame update
	void Start()
    {
		trigger = gameObject.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
		trigger.triggers.Add(entry);
	}

	public void OnPointerDownDelegate(PointerEventData data)
	{
		buildManager.BuildFreshBase(baseTypeIndex);
	}



    // Update is called once per frame
    void Update()
    {
        
    }
}
