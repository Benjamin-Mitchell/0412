using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RenderDistance : MonoBehaviour
{
	private GameObject cam;

	[Serializable]
	public struct PSystem
	{
		[SerializeField]
		public GameObject sys;

		[SerializeField]
		public float distanceToStopRendering;
	}

	public PSystem[] PSystems;
    // Start is called before the first frame update
    void Start()
    {
		cam = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
		for(int i = 0; i < PSystems.Length; i++)
		{
			if(Vector3.Distance(cam.transform.position, transform.position) > PSystems[i].distanceToStopRendering)
			{
				PSystems[i].sys.SetActive(false);
			}else
			{
				PSystems[i].sys.SetActive(true);
			}
		}
    }
}
