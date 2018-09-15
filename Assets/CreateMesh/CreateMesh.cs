using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour {
	void Start () {
		var mesh = new Mesh ();
		mesh.name = "TestMesh";
		var vertices = new List<Vector3> {
			new Vector3 (0.5f, 0.5f, 0),
			new Vector3 (-0.5f, 0.5f, 0),
			new Vector3 (0.5f, -0.5f, 0),
			new Vector3 (-0.5f, -0.5f, 0),
		};
		var triangles = new List<int> {
			1,
			0,
			2,
			1,
			2,
			3
		};
		mesh.SetVertices (vertices);
		mesh.SetTriangles (triangles, 0);

		var go = this.gameObject;

		var meshRenderer = go.AddComponent<MeshRenderer> () as MeshRenderer;
		meshRenderer.material = new Material (Shader.Find ("Standard"));

		var meshFilter = go.AddComponent<MeshFilter> () as MeshFilter;

		meshFilter.mesh = mesh;
	}
}
