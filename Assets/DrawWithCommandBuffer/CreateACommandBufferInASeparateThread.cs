using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateACommandBufferInASeparateThread : MonoBehaviour {

	Mesh CreateMesh () {
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
		return mesh;
	}

	CommandBuffer CreateCommandBuffer (Mesh mesh, Material material) {
		Debug.Log (Thread.CurrentThread.ManagedThreadId);
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
		Debug.Log (Thread.CurrentThread.ManagedThreadId);

		var mesh = CreateMesh ();
		var material = new Material (Shader.Find ("Unlit/TestShader"));

		var task = Task.Run (() => CreateCommandBuffer (mesh, material));
		task.Wait ();
		var buf = task.Result;

		var camera = GetComponent<Camera> ();
		camera.AddCommandBuffer (CameraEvent.AfterSkybox, buf);
	}
}
