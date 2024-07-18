using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.GUI
{
	public class HoldingBlockPreview : MonoBehaviour
	{
		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;
		private Animator animator;

		private Material material;

		private GameClient gameClient;

		private void Awake()
		{
			meshRenderer = GetComponent<MeshRenderer>();
			animator = GetComponent<Animator>();
			meshFilter = GetComponent<MeshFilter>();
			
			gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
		}

		private void Start()
		{
			material = meshRenderer.material;
		}

		private void Update()
		{
			// transform.eulerAngles = gameClient.player.playerCamera.transform.eulerAngles;
		}

		public void SetEnvLight(float skyLight, float blockLight)
		{
			material.SetVector("_EnvLight", new Vector4(skyLight, blockLight));
		}

		public void SetMeshFromShape(BlockShape shape, int lightAttenuation)
		{
			var shapeMesh = shape.GetShapeMesh(255, 0);
			var mesh = new Mesh();
			
			mesh.vertices = shapeMesh.positions;
			mesh.triangles = shapeMesh.triangles;
			mesh.uv = shapeMesh.texcoords;
			mesh.colors = shapeMesh.colors;
			mesh.RecalculateNormals();
			
			DestroyImmediate(meshFilter.sharedMesh, true);
			meshFilter.sharedMesh = mesh;
		}

		public void PlayUseBlockAnimation()
		{
			animator.Play("HandUse");
		}
	}
}