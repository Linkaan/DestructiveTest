using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiveCubeSharedVerts : MonoBehaviour {

	public Material mat;

	public float width;
	public float height;
	public float length;

	/* Coffecient between 0 and 1 where 0 means indesctructable and 1 means 100% descrutable */
	public float breakability;

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	void Start () {
		this.meshFilter = this.gameObject.AddComponent<MeshFilter> ();
		this.meshRenderer = this.gameObject.AddComponent<MeshRenderer> ();
		this.meshRenderer.material = this.mat;
		this.CreateCube ();
	}

	void CreateCube () {
		List<Vector3> vertecies = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Edge> edges = new List<Edge> ();

		CreateCubeMesh (vertecies, triangles, edges);

		Mesh mesh = new Mesh ();
		mesh.vertices = vertecies.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		Vector3[] normals = mesh.normals;
		for (int i = 0; i < normals.Length; i++) {
			normals [i] = -normals [i];
		}
		mesh.normals = normals;

		for (int m = 0; m < mesh.subMeshCount; m++) {
			int[] tris = mesh.GetTriangles (m);
			for (int i = 0; i < tris.Length; i+=3) {
				int temp = tris[i + 0];
				tris[i + 0] = tris[i + 1];
				tris[i + 1] = temp;
			}
			mesh.SetTriangles (tris, m);
		}

		meshFilter.mesh = mesh;
	}

	void CreateCubeMesh (List<Vector3> vertecies, List<int> triangles, List<Edge> edges) {
		float w = width, l = length, h = height;

		vertecies.Add (new Vector3 (0, 0, 0));
		vertecies.Add (new Vector3 (w, 0, 0));
		vertecies.Add (new Vector3 (0, 0, l));
		vertecies.Add (new Vector3 (w, 0, l));

		edges.Add (new Edge (0, 1));
		edges.Add (new Edge (2, 0));
		edges.Add (new Edge (1, 3));
		edges.Add (new Edge (3, 2));

		triangles.Add (3);
		triangles.Add (1);
		triangles.Add (0);
		triangles.Add (0);
		triangles.Add (2);
		triangles.Add (3);

		for (int i = 0; i < 4; i++) {
			Vector3 v = vertecies [i] + new Vector3 (0, h, 0);
			vertecies.Add (v);
		}

		for (int i = 0; i < 6; i += 3) {
			triangles.Add(triangles [i + 2] + 4);
			triangles.Add(triangles [i + 1] + 4);
			triangles.Add(triangles [i + 0] + 4);
		}

		for (int i = 0; i < 4; i++) {
			triangles.Add (edges [i].p1);
			triangles.Add (edges [i].p2);
			triangles.Add (edges [i].p2 + 4);
			triangles.Add (edges [i].p2 + 4);
			triangles.Add (edges [i].p1 + 4);
			triangles.Add (edges [i].p1);
		}
	}
	
	void Update () {
		
	}
}
