using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//manages general access to collider size regardless of what type of collider is attached.

//can extend for usage by mesh collider for more complex grid assembly and pathfinding.

public class colliderManager : MonoBehaviour
{
	[System.NonSerialized]
    public Vector3 size;

	public MeshFilter meshToMeasure;
	private Mesh mesh;


	// Start is called before the first frame update
	void Awake()
	{
		mesh = meshToMeasure.mesh;
		CalculateSize();
	}

	private void getMeshColliderSize()
	{
		//localise the verts
		Vector3[] verts = mesh.vertices;

		//break the vertices up into x, y and z components. 
		float[] xArray = new float[verts.Length];
		float[] yArray = new float[verts.Length];
		float[] zArray = new float[verts.Length];
	
		for (int i = 0; i < verts.Length; i++)
		{
			xArray[i] = verts[i].x;
			yArray[i] = verts[i].y;
			zArray[i] = verts[i].z;
		}
	
	
		//Now get min and max value of each array. The size is the differnce.
		float xMin = Mathf.Min(xArray);
		float xMax = Mathf.Max(xArray);
		size.x = xMax - xMin;
	
	
		float yMin = Mathf.Min(yArray);
		float yMax = Mathf.Max(yArray);
		size.y = yMax - yMin;
	
		float zMin = Mathf.Min(zArray);
		float zMax = Mathf.Max(zArray);
		size.z = zMax - zMin;
	
	}

	//private void getMeshColliderSize()
	//{
	//	//localise the verts
	//	Vector3[] verts = mesh.vertices;
	//
	//	float xMin = verts[0].x, xMax = verts[0].x;
	//	float yMin = verts[0].y, yMax = verts[0].y;
	//	float zMin = verts[0].z, zMax = verts[0].z;
	//
	//
	//	for (int i = 1; i < verts.Length; i++)
	//	{
	//		xMin = verts[0].x < xMin ? verts[0].x : xMin; 
	//		xMax = verts[0].x > xMax ? verts[0].x : xMax;
	//
	//		yMin = verts[0].y < yMin ? verts[0].y : yMin; 
	//		yMax = verts[0].y > yMax ? verts[0].y : yMax;
	//
	//		zMin = verts[0].z < zMin ? verts[0].z : zMin;
	//		zMax = verts[0].z > zMax ? verts[0].z : zMax;
	//	}		
	//}

	//
	public void CalculateSize()
	{
		BoxCollider boxCol;
		if ((boxCol = GetComponent<BoxCollider>()) != null)
		{
			size = Vector3.Scale(transform.localScale, boxCol.size);
			return;
		}
		else if ((boxCol = GetComponentInChildren<BoxCollider>()) != null)
		{
			size = Vector3.Scale(transform.localScale, boxCol.size);
			return;
		}

		SphereCollider sphereCol;
		if ((sphereCol = GetComponent<SphereCollider>()) != null)
		{
			float radius = sphereCol.radius;
			size = new Vector3(radius / 2.0f, radius / 2.0f, radius / 2.0f);
			return;
		}
		else if ((sphereCol = GetComponentInChildren<SphereCollider>()) != null)
		{
			float radius = sphereCol.radius;
			size = new Vector3(radius / 2.0f, radius / 2.0f, radius / 2.0f);
			return;
		}

		//MeshCollider meshCol;
		//if ((meshCol = GetComponent<MeshCollider>()) != null)
		//{
		getMeshColliderSize();
		//}
		//else if ((meshCol = GetComponentInChildren<MeshCollider>()) != null)
		//{
		//getMeshColliderSize();
		//}

		//add any further colliders to test here if and when we add them.
	}

}
