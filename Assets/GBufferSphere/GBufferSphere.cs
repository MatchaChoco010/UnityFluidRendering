using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GBufferSphere : MonoBehaviour {

	private Mesh quad;
	private Material material;
	private int gBuffer0ColorPropertyID;
	private int gBuffer1ColorPropertyID;
	private int gBuffer3ColorPropertyID;
	private Dictionary<Camera, CommandBuffer> dict = new Dictionary<Camera, CommandBuffer> ();

	[ColorUsage (showAlpha: false)]
	public Color albedo;
	[Range (0, 1)]
	public float occlusion;
	[ColorUsage (showAlpha: false)]
	public Color specular;
	[Range (0, 1)]
	public float smoothness;
	[ColorUsage (showAlpha: false, hdr: true)]
	public Color emission;

	private void Start () {
		quad = new Mesh ();
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
		var uvs = new List<Vector2> {
			new Vector2 (1, 1),
			new Vector2 (0, 1),
			new Vector2 (1, 0),
			new Vector2 (0, 0),
		};
		quad.SetVertices (vertices);
		quad.SetTriangles (triangles, 0);
		quad.SetUVs (0, uvs);

		material = new Material (Shader.Find ("GBuffer/GBufferSphere"));

		gBuffer0ColorPropertyID = Shader.PropertyToID ("_GBuffer0Color");
		gBuffer1ColorPropertyID = Shader.PropertyToID ("_GBuffer1Color");
		gBuffer3ColorPropertyID = Shader.PropertyToID ("_GBuffer3Color");
	}

	private void OnWillRenderObject () {
		var camera = Camera.current;
		if (camera == null) return;

		CommandBuffer buf;
		if (dict.ContainsKey (camera)) {
			buf = dict[camera];
		} else {
			buf = new CommandBuffer ();
			buf.name = "GBuffer Sphere";
			camera.AddCommandBuffer (CameraEvent.AfterGBuffer, buf);
			dict[camera] = buf;
		}

		buf.Clear ();

		var gBufferTarget = new [] {
			new RenderTargetIdentifier (BuiltinRenderTextureType.GBuffer0),
				new RenderTargetIdentifier (BuiltinRenderTextureType.GBuffer1),
				new RenderTargetIdentifier (BuiltinRenderTextureType.GBuffer2),
				new RenderTargetIdentifier (BuiltinRenderTextureType.CameraTarget),
		};

		buf.SetRenderTarget (gBufferTarget, BuiltinRenderTextureType.CameraTarget);

		material.SetVector (
			gBuffer0ColorPropertyID,
			new Vector4 (albedo.r, albedo.g, albedo.b, occlusion)
		);
		material.SetVector (
			gBuffer1ColorPropertyID,
			new Vector4 (specular.r, specular.g, specular.b, smoothness)
		);
		material.SetVector (
			gBuffer3ColorPropertyID,
			new Vector4 (emission.r, emission.g, emission.b, 0)
		);

		buf.DrawMesh (
			quad,
			Matrix4x4.Rotate (camera.transform.rotation),
			material,
			submeshIndex : 0,
			shaderPass : 0,
			properties : null
		);
	}
}
