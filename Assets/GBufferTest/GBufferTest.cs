using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GBufferTest : MonoBehaviour {

	private Mesh quad;
	private Material material;
	private int gBuffer0ColorPropertyID;
	private int gBuffer1ColorPropertyID;
	private int gBuffer3ColorPropertyID;
	private CommandBuffer buf;

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
		quad.SetVertices (vertices);
		quad.SetTriangles (triangles, 0);
		quad.RecalculateNormals ();

		material = new Material (Shader.Find ("GBuffer/GBufferTest"));

		gBuffer0ColorPropertyID = Shader.PropertyToID ("_GBuffer0Color");
		gBuffer1ColorPropertyID = Shader.PropertyToID ("_GBuffer1Color");
		gBuffer3ColorPropertyID = Shader.PropertyToID ("_GBuffer3Color");

		buf = new CommandBuffer ();
		buf.name = "GBuffer Test";
		foreach (var cam in Camera.allCameras) {
			if (!cam) {
				break;
			}
			cam.AddCommandBuffer (CameraEvent.AfterGBuffer, buf);
		}

#if UNITY_EDITOR
		var sceneViewCameras = SceneView.GetAllSceneCameras ();
		foreach (var cam in sceneViewCameras) {
			if (!cam) {
				break;
			}
			cam.AddCommandBuffer (CameraEvent.AfterGBuffer, buf);
		}
#endif
	}

	private void Update () {
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

		var matrix = Matrix4x4.TRS (
			transform.position,
			transform.rotation,
			transform.localScale
		);

		buf.DrawMesh (
			quad,
			matrix,
			material,
			submeshIndex : 0,
			shaderPass : 0,
			properties : null
		);
	}
}
