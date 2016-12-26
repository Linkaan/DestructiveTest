using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiveCubeUniqueVerts : MonoBehaviour {

	public Material mat;

	public float width;
	public float height;
	public float length;

	/* Coffecient between 0 and 1 where 0 means indesctructable and 1 means 100% descrutable */
	public float breakability;

	public float maxShatterRadius = 1;
	public float minShatterRadius = 0.05f;
	public float wiggleness = 0.1f;

	public float minLines = 10;
	public float maxLines = 25;

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	private bool hasShatterd = false;

	List<List<Vector2>> fractures = new List<List<Vector2>> ();

	void Start () {
		this.meshFilter = this.gameObject.AddComponent<MeshFilter> ();
		this.meshRenderer = this.gameObject.AddComponent<MeshRenderer> ();
		this.meshRenderer.material = this.mat;
		this.CreateCube ();
		this.gameObject.AddComponent<BoxCollider> ();
		this.gameObject.AddComponent<Rigidbody> ();
	}

	void OnCollisionEnter(Collision col) {
		float otherMass;
		if (col.rigidbody)
			otherMass = col.rigidbody.mass;
		else
			otherMass = 1000;
		Vector3 force = col.relativeVelocity * otherMass;
		if (force.magnitude > 10) {
			Debug.Log ("force: " + force.magnitude);
			Destroy (this.GetComponent<Rigidbody>());
			Destroy (this.GetComponent<Collider>());
			Destroy (this.meshFilter);
			Destroy (this.meshRenderer);
			List<Vector3> points = new List<Vector3> ();
			foreach (ContactPoint point in col.contacts) {
				points.Add (point.point);
			}
			Vector3 midpoint = GetMeanVector (points.ToArray ());
			this.CreateShatteredCube (midpoint/*new Vector3(width / 2f, 0, length / 2f)*/);
		}
	}

	Vector3 GetMeanVector(Vector3[] positions) {
		if (positions.Length == 0)
			return Vector3.zero;
		float x = 0f;
		float y = 0f;
		float z = 0f;
		foreach (Vector3 pos in positions) {
			x += pos.x;
			y += pos.y;
			z += pos.z;
		}
		return new Vector3(x / positions.Length, y / positions.Length, z / positions.Length);
	}

	void CreateCube () {
		List<Vector3> vertecies = new List<Vector3> ();
		List<int> triangles = new List<int> ();

		CreateCubeMesh (vertecies, triangles);

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

	void CreateCubeMesh (List<Vector3> vertecies, List<int> triangles) {
		List<Edge> edges = new List<Edge> ();
		float w = width, l = length, h = height;

		for (int i = 0; i < 3; i++) {
			vertecies.Add (new Vector3 (0, 0, 0));
			vertecies.Add (new Vector3 (w, 0, 0));
			vertecies.Add (new Vector3 (0, 0, l));
			vertecies.Add (new Vector3 (w, 0, l));
		}
			
		edges.Add (new Edge (1, 3));
		edges.Add (new Edge (2, 0));
		edges.Add (new Edge (0, 1));
		edges.Add (new Edge (3, 2));

		triangles.Add (3);
		triangles.Add (1);
		triangles.Add (0);
		triangles.Add (0);
		triangles.Add (2);
		triangles.Add (3);

		for (int i = 0; i < 12; i++) {
			Vector3 v = vertecies [i] + new Vector3 (0, h, 0);
			vertecies.Add (v);
		}

		for (int i = 0; i < 6; i += 3) {
			triangles.Add(triangles [i + 2] + 12);
			triangles.Add(triangles [i + 1] + 12);
			triangles.Add(triangles [i + 0] + 12);
		}

		for (int i = 0; i < 4; i++) {
			triangles.Add (edges [i].p1 + Convert.ToInt32(i < 2) * 4 + 4);
			triangles.Add (edges [i].p2 + Convert.ToInt32(i < 2) * 4 + 4);
			triangles.Add (edges [i].p2 + 12 + Convert.ToInt32(i < 2) * 4 + 4);
			triangles.Add (edges [i].p2 + 12 + Convert.ToInt32(i < 2) * 4 + 4);
			triangles.Add (edges [i].p1 + 12 + Convert.ToInt32(i < 2) * 4 + 4);
			triangles.Add (edges [i].p1 + Convert.ToInt32(i < 2) * 4 + 4);
		}
	}

	void CreateShatteredCube (Vector3 point) {
		Vector3 localPoint = transform.InverseTransformPoint (point);
		List<List<Vector2>> lines = new List<List<Vector2>> ();
		List<Vector2> points = new List<Vector2> ();
		List<Vector2> outerPoints = new List<Vector2> ();
		float baseRadius = Mathf.Lerp (minShatterRadius, maxShatterRadius, Mathf.InverseLerp (1, 0, breakability));
		int linesCount = (int) UnityEngine.Random.Range (minLines, maxLines);
		for (int i = 0; i < linesCount; i++) {
			float baseRot = (360f / linesCount) * i;
			float rotMaxMin = (360f / linesCount) / 2 * wiggleness;
			float newRadius = baseRadius;
			List<Vector2> line = new List<Vector2> ();
			Vector2 currentPoint = new Vector2(localPoint.x, localPoint.z);
			line.Add (currentPoint);
			while (true) {
				newRadius *= 2;
				float rot = baseRot + UnityEngine.Random.Range (-rotMaxMin, rotMaxMin);
				currentPoint = new Vector2 (localPoint.x + newRadius * Mathf.Cos (Mathf.Deg2Rad * rot), localPoint.z + newRadius * Mathf.Sin (Mathf.Deg2Rad * rot));
				if (currentPoint.x > 0 && currentPoint.y > 0 && currentPoint.x < width && currentPoint.y < length) {
					line.Add (currentPoint);
					points.Add (currentPoint);
				} else {
					//line.Add (currentPoint);
					Vector2 op = FindPointOnRectangleEdge (width, length, 360 - rot);
					outerPoints.Add (op);
					line.Add (op);
					break;
				}
			}
			lines.Add (line);
		}
		if (lines.Count > 0) {
			List<Vector2> last = lines [0];
			//fractures.Add (last);
			for (int i = 1; i < lines.Count; i++) {
				List<Vector2> current = lines [i];
				AddFracture (last, current, outerPoints);
				last = current;
				//fractures.Add (last);
			}
			AddFracture (last, lines [0], outerPoints);
			/*
			Dictionary<Vector2, List<Vector2>> cornerPoints = new Dictionary<Vector2, List<Vector2>> ();

			List<Vector2> edgePoints = FindEdgePoints ();
			foreach (Vector2 p in points) {
				if (GetNumConnections (p) < 2) {
					Vector2 key = GetClosestPoint (edgePoints, p);
					if (!cornerPoints.ContainsKey (key)) {
						List<Vector2> corner = new List<Vector2> ();
						corner.Add (key);
						cornerPoints.Add (key, corner);
					}
					cornerPoints [key].Add (p);
				}
			}

			foreach (List<Vector2> corner in cornerPoints.Values) {
				Debug.Log ("Corner fracture");
				foreach (Vector2 p in corner) {
					Debug.Log (p);
				}
				Debug.Log ("----------------");
				fractures.Add (corner);
			}*/
			Vector2 lastp = outerPoints [0];
			for (int i = 1; i < outerPoints.Count; i++) {
				Vector2 current = outerPoints [i];
				//AddFracture (last, current, outerPoints);
				Vector2 midpoint = Vector2.Lerp (lastp, current, 0.5f);
				if (!(midpoint.x == lastp.x || midpoint.x == current.x ||
					midpoint.y == lastp.y || midpoint.y == current.y)) {
					List<Vector2> fracture = new List<Vector2> ();
					fracture.Add (lastp);
					fracture.Add (GetClosestPoint (FindEdgePoints (), midpoint));
					fracture.Add (current);
					fractures.Add (fracture);
				}
				lastp = current;
				//fractures.Add (last);
			}
		}
	}

	int GetNumConnections (Vector2 p) {
		int connections = 0;
		foreach (List<Vector2> frac in fractures) {
			if (frac.Contains (p))
				connections++;
		}
		return connections;
	}

	bool IsConnected (Vector2 last, Vector2 current) {
		foreach (List<Vector2> frac in fractures) {
			if (frac.Contains(last) && frac.Contains(current)) {
				return true;
			}
		}
		return false;
	}

	void AddEdgeFracture (Vector2 last, Vector2 current, List<Vector2> points) {
		Vector2 midpoint = Vector2.Lerp (last, current, 0.5f);
		Vector2 point = GetClosestPoint(points, midpoint);
		List<Vector2> fracture = new List<Vector2> ();
		if (midpoint.x == last.x || midpoint.x == current.x ||
		    midpoint.y == last.y || midpoint.y == current.y) { // if midpoint is on polygon edge
			fracture.Add (last);
			fracture.Add (point);
			fracture.Add (current);
		} else {
			fracture.Add (last);
			fracture.Add (point);
			fracture.Add (GetClosestPoint (FindEdgePoints (), midpoint));
			fracture.Add (current);
		}
		fractures.Add (fracture);
	}

	List<Vector2> FindEdgePoints () {
		List<Vector2> edgePoints = new List<Vector2> ();
		float w = width, l = length;
		edgePoints.Add (new Vector2 (0, 0));
		edgePoints.Add (new Vector2 (w, 0));
		edgePoints.Add (new Vector2 (0, l));
		edgePoints.Add (new Vector2 (w, l));
		return edgePoints;
	}

	Vector2 GetClosestPoint (List<Vector2> points, Vector2 point) {
		Vector2 closestPoint = Vector2.zero;
		float closestDistanceSqr = Mathf.Infinity;
		foreach(Vector2 potentialPoint in points) {
			Vector3 directionToTarget = potentialPoint - point;
			float dSqrToTarget = directionToTarget.sqrMagnitude;
			if (dSqrToTarget < closestDistanceSqr) {
				closestDistanceSqr = dSqrToTarget;
				closestPoint = potentialPoint;
			}
		}

		return closestPoint;
	}

	Vector2 FindPointOnRectangleEdge (float a, float b, float rot) {
		float theta = rot * Mathf.Deg2Rad;

		while (theta < -Mathf.PI) {
			theta += Mathf.PI * 2;
		}

		while (theta > Mathf.PI) {
			theta -= Mathf.PI * 2;
		}

		float rectAtan = Mathf.Atan2 (b, a);
		float thetaTan = Mathf.Tan (theta);

		int region = CalculateRectangleRegion (rectAtan, theta);
		float xFactor = 1;
		float yFactor = 1;

		switch (region) {
		case 1: yFactor = -1; break;
		case 2: yFactor = -1; break;
		case 3: xFactor = -1; break;
		case 4: xFactor = -1; break;
		}

		if (region == 1 || region == 3) {
			return new Vector2 (a/2f + xFactor * (a / 2f),
								b/2f + yFactor * (a / 2f) * thetaTan);
		} else {
			return new Vector2 (a/2f + xFactor * (b / (2f * thetaTan)),
								b/2f + yFactor * (b / 2f));
		}
	}

	int CalculateRectangleRegion (float rectAtan, float theta) {
		if ((theta > -rectAtan) &&
		    (theta <= rectAtan))
			return 1;
		else if ((theta > rectAtan) &&
		         (theta <= (Mathf.PI - rectAtan)))
			return 2;
		else if ((theta > (Mathf.PI - rectAtan)) ||
		         (theta <= -(Mathf.PI - rectAtan)))
			return 3;
		else
			return 4;
	}

	void AddFracture (List<Vector2> last, List<Vector2> current, List<Vector2> outerPoints) {
		for (int j = 0; j < Mathf.Min(current.Count, last.Count) - 1; j++) {
			Vector2 p1 = last [j];
			Vector2 p2 = last [j+1];
			Vector2 p3 = current [j];
			Vector2 p4 = current [j+1];
			List<Vector2> fracture;
			float rand = UnityEngine.Random.value;
			if (rand > 2f / 3f) {				
				fracture = new List<Vector2> ();
				fracture.Add (p1);
				fracture.Add (p2);
				fracture.Add (p4);
				fractures.Add (fracture);
				fracture = new List<Vector2> ();
				fracture.Add (p1);
				fracture.Add (p3);
				fracture.Add (p4);
				fractures.Add (fracture);
			} else if (rand > 1f / 3f) {
				fracture = new List<Vector2> ();
				fracture.Add (p1);
				fracture.Add (p2);
				fracture.Add (p3);
				fractures.Add (fracture);
				fracture = new List<Vector2> ();
				fracture.Add (p2);
				fracture.Add (p3);
				fracture.Add (p4);
				fractures.Add (fracture);
			} else {				
				fracture = new List<Vector2> ();
				fracture.Add (p1);
				fracture.Add (p2);
				fracture.Add (p3);
				fracture.Add (p4);
				fractures.Add (fracture);
			}
		}
	}
	
	void Update () {
		if (fractures != null && fractures.Count > 0 && !hasShatterd) {
			foreach (List<Vector2> frac in fractures) {
				GameObject newFracChild = new GameObject ();
				Fragment fragment = newFracChild.AddComponent<Fragment> ();
				fragment.InstantiateShatter (height, frac, mat);
				newFracChild.transform.parent = this.transform;/*
				if (frac.Count > 0) {
					Vector3 last = frac [0];
					for (int i = 1; i < frac.Count; i++) {
						Vector3 current = frac [i];
						Debug.DrawLine (last, current);
						last = current;
					}
				} else {
					Debug.LogWarning ("frac verts count is zero!");
				}*/
				//Debug.DrawLine (last, frac [0]);
			}
			hasShatterd = true;
		}
		/*	
		for (int i = 0; i < 360; i += 10) {
			Vector3 point = FindPointOnRectangleEdge (width, length, i);
			Debug.DrawLine (point, point + Vector3.up * 0.03f, new Color(Mathf.Lerp(0, 1, Mathf.InverseLerp(0, 360, i)), 0, 0));
		}
		*/
	}
}
