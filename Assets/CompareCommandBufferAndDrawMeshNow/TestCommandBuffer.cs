using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TestCommandBuffer : MonoBehaviour {

	Mesh mesh;
	Material material;

	public Camera cam;

	void AddDrawMeshCommand (CommandBuffer buf, float x, float y, float z) {
		buf.DrawMesh (
			mesh,
			Matrix4x4.TRS (new Vector3 (x, y, z), Quaternion.identity, Vector3.one),
			material
		);
	}

	CommandBuffer CreateCommandBuffer () {
		var buf = new CommandBuffer ();

		buf.name = "My Command Buffer";

		for (var x = 0; x < 100; x++) {
			for (var y = 0; y < 10; y++) {
				for (var z = 0; z < 10; z++) {
					AddDrawMeshCommand (buf, x, y, z);
				}
			}
		}

		return buf;
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

		var buf = CreateCommandBuffer ();
		cam.AddCommandBuffer (CameraEvent.AfterSkybox, buf);
	}
}
