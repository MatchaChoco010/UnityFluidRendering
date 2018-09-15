using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrawMeshNow : MonoBehaviour {

	Mesh mesh;
	Material material;

	void DrawMesh (float x, float y, float z) {
		Graphics.DrawMeshNow (mesh, new Vector3 (x, y, z), Quaternion.identity);
	}

	void Start () {
		mesh = new Mesh ();
		mesh.name = "TestMesh";
		var vertices = new List<Vector3> {
			new Vector3 (1f, 1f, 0),
			new Vector3 (-1f, 1f, 0),
			new Vector3 (1f, -1f, 0),
			new Vector3 (-1f, -1f, 0),
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
		for (var x = 0; x < 100; x++) {
			for (var y = 0; y < 10; y++) {
				for (var z = 0; z < 10; z++) {
					DrawMesh (x, y, z);
				}
			}
		}
	}
}
