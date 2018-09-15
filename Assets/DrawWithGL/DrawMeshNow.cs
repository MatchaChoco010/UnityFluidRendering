using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMeshNow : MonoBehaviour {

	Mesh mesh;
	Material material;

	void Start () {
		mesh = new Mesh ();
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

		material = new Material (Shader.Find ("Unlit/TestShader"));
	}

	void OnRenderObject () {
		material.SetPass (0);
		Graphics.DrawMeshNow (mesh, transform.position, transform.rotation);
	}
}
