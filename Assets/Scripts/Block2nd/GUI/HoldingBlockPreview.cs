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
		private MeshFilter meshFilter;
		private Animator animator;

		private GameClient gameClient;

		private void Awake()
		{
			animator = GetComponent<Animator>();
			meshFilter = GetComponent<MeshFilter>();
			
			gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
		}

		private void Update()
		{
			// transform.eulerAngles = gameClient.player.playerCamera.transform.eulerAngles;
		}

		public void SetMeshFromShape(BlockShape shape)
		{
			var shapeMesh = shape.GetShapeMesh(255, 0);
			var mesh = new Mesh();
			
			mesh.vertices = shapeMesh.positions;
			mesh.triangles = shapeMesh.triangles;
			mesh.uv = shapeMesh.texcoords;
			mesh.RecalculateNormals();
			
			DestroyImmediate(meshFilter.sharedMesh, true);
			meshFilter.sharedMesh = mesh;
		}

		public void PlayUseBlockAnimation()
		{
			animator.SetTrigger("UseBlock");
		}
	}
}