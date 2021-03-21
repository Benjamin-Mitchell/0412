using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BaseRotate : MonoBehaviour
{
	public Vector3 rotation;
    // Start is called before the first frame update
    void Start()
    {
		rotation *= 120;
    }

    // Update is called once per frame
    void Update()
    {
		//transform.Rotate(rotation);
    }

	private void OnRenderObject()
	{
		transform.Rotate(rotation * Time.deltaTime);
	}
}
