using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWithGL : MonoBehaviour {

	Material material;

	void Start () {
		material = new Material (Shader.Find ("Unlit/TestShader"));
	}

	void OnRenderObject () {
		GL.PushMatrix ();

		GL.MultMatrix (transform.localToWorldMatrix);

		material.SetPass (0);

		GL.Begin (GL.TRIANGLES);

		GL.Vertex (new Vector3 (-0.5f, 0.5f, 0));
		GL.Vertex (new Vector3 (0.5f, 0.5f, 0));
		GL.Vertex (new Vector3 (0.5f, -0.5f, 0));

		GL.Vertex (new Vector3 (-0.5f, 0.5f, 0));
		GL.Vertex (new Vector3 (0.5f, -0.5f, 0));
		GL.Vertex (new Vector3 (-0.5f, -0.5f, 0));

		GL.End ();

		GL.PopMatrix ();
	}
}
