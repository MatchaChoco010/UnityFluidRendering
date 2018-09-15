using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent (typeof (Camera))]
public class DrawWithCommandBuffer : MonoBehaviour {

	CommandBuffer CreateCommandBuffer () {
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

		var material = new Material (Shader.Find ("Standard"));

		var buf = new CommandBuffer ();

		buf.name = "My Command Buffer";

		buf.DrawMesh (
			mesh,
			Matrix4x4.TRS (new Vector3 (0, 0, 0), Quaternion.identity, Vector3.one),
			material
		);
		buf.DrawMesh (
			mesh,
			Matrix4x4.TRS (new Vector3 (2f, 0, 0), Quaternion.identity, Vector3.one),
			material
		);
		buf.DrawMesh (
			mesh,
			Matrix4x4.TRS (new Vector3 (-2f, 0, 0), Quaternion.identity, Vector3.one),
			material
		);

		return buf;
	}

	void Start () {
		var buf = CreateCommandBuffer ();

		var camera = GetComponent<Camera> ();
		camera.AddCommandBuffer (CameraEvent.AfterSkybox, buf);
	}

}
