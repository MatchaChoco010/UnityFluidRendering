using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GBufferSphereParticle : MonoBehaviour {

	private Mesh quad;
	private Material material;
	private int gBuffer0ColorPropertyID;
	private int gBuffer1ColorPropertyID;
	private int gBuffer3ColorPropertyID;
	private int radiusID;
	private Dictionary<Camera, CommandBuffer> dict = new Dictionary<Camera, CommandBuffer> ();
	private ParticleSystem.Particle[] particles;
	private Matrix4x4[] matrices;
	private float[] radiuses;

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
	public new ParticleSystem particleSystem;

	private void Start () {
		quad = new Mesh ();
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
		var uvs = new List<Vector2> {
			new Vector2 (1, 1),
			new Vector2 (0, 1),
			new Vector2 (1, 0),
			new Vector2 (0, 0),
		};
		quad.SetVertices (vertices);
		quad.SetTriangles (triangles, 0);
		quad.SetUVs (0, uvs);

		material = new Material (Shader.Find ("GBuffer/GBufferSphereInstanced"));
		material.enableInstancing = true;

		gBuffer0ColorPropertyID = Shader.PropertyToID ("_GBuffer0Color");
		gBuffer1ColorPropertyID = Shader.PropertyToID ("_GBuffer1Color");
		gBuffer3ColorPropertyID = Shader.PropertyToID ("_GBuffer3Color");
		radiusID = Shader.PropertyToID ("_Radius");

		particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
		matrices = new Matrix4x4[particleSystem.main.maxParticles];
		for (var i = 0; i < particleSystem.main.maxParticles; i++) {
			matrices[i] = new Matrix4x4 ();
		}
		radiuses = new float[particleSystem.main.maxParticles];
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

		var numParticleAlive = particleSystem.GetParticles (particles);
		for (var i = 0; i < numParticleAlive; i++) {
			var particleSize = particles[i].GetCurrentSize (particleSystem);
			matrices[i].SetTRS (
				particles[i].position,
				camera.transform.rotation,
				Vector3.one * particleSize
			);
			radiuses[i] = particleSize / 2;
		}

		var properties = new MaterialPropertyBlock ();
		properties.SetFloatArray (radiusID, radiuses);

		buf.DrawMeshInstanced (quad, 0, material, 0, matrices, numParticleAlive, properties);
	}
}
