using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fragment : MonoBehaviour {

	private float h;

	private List<Vector2> points;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public void InstantiateShatter (float h, List<Vector2> points, Material mat) {
		this.meshFilter = this.gameObject.AddComponent<MeshFilter> ();
		this.meshRenderer = this.gameObject.AddComponent<MeshRenderer> ();
		this.meshRenderer.material = mat;
		this.points = points;
		this.h = h;
		this.CreateShatter ();
		this.gameObject.AddComponent<MeshCollider> ().convex = true;
		this.gameObject.AddComponent<Rigidbody> ();
	}

	void CreateShatter () {
		List<Vector3> vertecies = new List<Vector3> ();
		List<int> triangles = new List<int> ();

		CreateShatterMesh (vertecies, triangles);

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

	void CreateShatterMesh (List<Vector3> vertecies, List<int> triangles) {
		List<Edge> edges = new List<Edge> ();

		triangles.Clear ();

		foreach (Vector2 p in points) {
			vertecies.Add (new Vector3 (p.x, 0, p.y));
		}

		int verteciesCount = vertecies.Count;
		int count = points.Count;

		if (count == 4) {
			edges.Add (new Edge (1, 3));
			edges.Add (new Edge (2, 0));
			edges.Add (new Edge (0, 1));
			edges.Add (new Edge (3, 2));
		} else {
			edges.Add (new Edge (1, 2));
			edges.Add (new Edge (0, 1));
			edges.Add (new Edge (2, 0));
		}

		if (count == 4) {
			triangles.Add (3);
			triangles.Add (1);
			triangles.Add (0);
			triangles.Add (0);
			triangles.Add (2);
			triangles.Add (3);
		} else {
			triangles.Add (2);
			triangles.Add (1);
			triangles.Add (0);
		}

		for (int i = 0; i < verteciesCount; i++) {
			Vector3 v = vertecies [i] + new Vector3 (0, h, 0);
			vertecies.Add (v);
		}

		int triangleCount = triangles.Count;
		for (int i = 0; i < triangleCount; i += 3) {
			triangles.Add(triangles [i + 2] + verteciesCount);
			triangles.Add(triangles [i + 1] + verteciesCount);
			triangles.Add(triangles [i + 0] + verteciesCount);
		}

		for (int i = 0; i < count; i++) {
			triangles.Add (edges [i].p1);
			triangles.Add (edges [i].p2);
			triangles.Add (edges [i].p2 + verteciesCount);
			triangles.Add (edges [i].p2 + verteciesCount);
			triangles.Add (edges [i].p1 + verteciesCount);
			triangles.Add (edges [i].p1);
		}
	}
}
