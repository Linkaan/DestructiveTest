using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInspector : MonoBehaviour {

	private MeshFilter meshFilter;

	// Use this for initialization
	void Start () {
		meshFilter = GetComponent<MeshFilter> ();
		Mesh mesh = meshFilter.mesh;

		Debug.Log (mesh.vertices.Length);
		Debug.Log (mesh.triangles.Length);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
