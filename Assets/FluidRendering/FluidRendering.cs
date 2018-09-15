using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FluidRendering : MonoBehaviour {

	private Mesh quad;
	private Mesh picturePlane;
	private Material material;
	private int gBuffer0ColorPropertyID;
	private int gBuffer1ColorPropertyID;
	private int gBuffer3ColorPropertyID;
	private int frustumCornersID;
	private int radiusID;
	private int depth0RT;
	private int depth1RT;
	private Dictionary<Camera, CommandBuffer> dict = new Dictionary<Camera, CommandBuffer> ();
	private ParticleSystem.Particle[] particles;
	private Matrix4x4[] matrices;

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
	public ParticleSystem particleSystem;
	public float particleSize = 1f;

	private void Start () {
		quad = new Mesh ();
		var quadVertices = new List<Vector3> {
			new Vector3 (0.5f, 0.5f, 0),
			new Vector3 (-0.5f, 0.5f, 0),
			new Vector3 (0.5f, -0.5f, 0),
			new Vector3 (-0.5f, -0.5f, 0),
		};
		var quadTriangles = new List<int> {
			1,
			0,
			2,
			1,
			2,
			3
		};
		var quadUVs = new List<Vector2> {
			new Vector2 (1, 1),
			new Vector2 (0, 1),
			new Vector2 (1, 0),
			new Vector2 (0, 0),
		};
		quad.SetVertices (quadVertices);
		quad.SetTriangles (quadTriangles, 0);
		quad.SetUVs (0, quadUVs);

		picturePlane = new Mesh ();
		var ppVertices = new List<Vector3> {
			new Vector3 (1.0f, 1.0f, 0.0f),
			new Vector3 (-1.0f, 1.0f, 0.0f),
			new Vector3 (-1.0f, -1.0f, 0.0f),
			new Vector3 (1.0f, -1.0f, 0.0f),
		};
		var ppTriangles = new List<int> { 0, 1, 2, 2, 3, 0 };
		var ppUVs = new List<Vector2> {
			new Vector2 (1f, 0),
			new Vector2 (0, 0),
			new Vector2 (0, 1f),
			new Vector2 (1f, 1f),
		};
		picturePlane.SetVertices (ppVertices);
		picturePlane.SetTriangles (ppTriangles, 0);
		picturePlane.SetUVs (0, ppUVs);

		material = new Material (Shader.Find ("Fluid/FluidParticle"));
		material.enableInstancing = true;

		gBuffer0ColorPropertyID = Shader.PropertyToID ("_GBuffer0Color");
		gBuffer1ColorPropertyID = Shader.PropertyToID ("_GBuffer1Color");
		gBuffer3ColorPropertyID = Shader.PropertyToID ("_GBuffer3Color");
		frustumCornersID = Shader.PropertyToID ("_FrustumCorner");
		radiusID = Shader.PropertyToID ("_Radius");
		depth0RT = Shader.PropertyToID ("_Depth0RT");
		depth1RT = Shader.PropertyToID ("_Depth1RT");

		particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
		matrices = new Matrix4x4[particleSystem.main.maxParticles];
		for (var i = 0; i < particleSystem.main.maxParticles; i++) {
			matrices[i] = new Matrix4x4 ();
		}
	}

	private void OnWillRenderObject () {
		var camera = Camera.current;
		if (camera == null) return;

		CommandBuffer buf;
		if (dict.ContainsKey (camera)) {
			buf = dict[camera];
		} else {
			buf = new CommandBuffer ();
			buf.name = "GBuffer Fluid";
			camera.AddCommandBuffer (CameraEvent.AfterGBuffer, buf);
			dict[camera] = buf;
		}

		buf.Clear ();

		buf.GetTemporaryRT (
			depth0RT,
			width: -1,
			height: -1,
			depthBuffer : 0,
			filter : FilterMode.Point,
			format : RenderTextureFormat.RFloat
		);

		buf.SetRenderTarget (
			new RenderTargetIdentifier (depth0RT),
			new RenderTargetIdentifier (BuiltinRenderTextureType.CameraTarget)
		);
		buf.ClearRenderTarget (false, true, new Color (1, 1, 1, 1));

		material.SetFloat (radiusID, particleSize);

		var numParticleAlive = particleSystem.GetParticles (particles);
		for (var i = 0; i < numParticleAlive; i++) {
			matrices[i].SetTRS (
				particles[i].position,
				camera.transform.rotation,
				Vector3.one * particleSize
			);
		}

		buf.DrawMeshInstanced (quad, 0, material, 0, matrices);

		buf.GetTemporaryRT (
			depth1RT,
			width: -1,
			height: -1,
			depthBuffer : 0,
			filter : FilterMode.Point,
			format : RenderTextureFormat.RFloat
		);

		buf.SetRenderTarget (
			new RenderTargetIdentifier (depth1RT),
			new RenderTargetIdentifier (BuiltinRenderTextureType.CameraTarget)
		);
		buf.ClearRenderTarget (false, true, new Color (1, 1, 1, 1));

		buf.DrawMesh (
			picturePlane,
			Matrix4x4.identity,
			material,
			submeshIndex : 0,
			shaderPass : 1
		);

		buf.SetRenderTarget (
			new RenderTargetIdentifier (depth0RT),
			new RenderTargetIdentifier (BuiltinRenderTextureType.CameraTarget)
		);
		buf.ClearRenderTarget (false, true, new Color (1, 1, 1, 1));

		buf.DrawMesh (
			picturePlane,
			Matrix4x4.identity,
			material,
			submeshIndex : 0,
			shaderPass : 2
		);

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

		var right = camera.farClipPlane * Mathf.Tan (camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * camera.aspect;
		var left = -camera.farClipPlane * Mathf.Tan (camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * camera.aspect;
		var top = camera.farClipPlane * Mathf.Tan (camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
		var bottom = -camera.farClipPlane * Mathf.Tan (camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
		var corner = new Vector4 (left, right, top, bottom);
		material.SetVector (frustumCornersID, corner);

		buf.DrawMesh (
			picturePlane,
			Matrix4x4.identity,
			material,
			submeshIndex : 0,
			shaderPass : 3
		);
	}
}
