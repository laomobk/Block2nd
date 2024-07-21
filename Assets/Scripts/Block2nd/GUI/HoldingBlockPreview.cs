using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.GUI.GameGUI;
using Block2nd.Render;
using Block2nd.Utils;
using UnityEngine;

namespace Block2nd.GUI
{
	public class HoldingBlockPreview : MonoBehaviour
	{
		private static TransformSettings _cubeModeTransformSettings = new TransformSettings
		{
			localPos = Vector3.zero,
			localEulerAngels = Vector3.zero,
			localScale = Vector3.one,
		};

		private static TransformSettings _planeModeTransformSettings = new TransformSettings
		{
			localPos = new Vector3(0.22f, 0.32f, 0.603f),
			localEulerAngels = new Vector3(1.7f, 131.1f, 23.55f),
			localScale = new Vector3(1.256337f, 1.256336f, 1.256336f)
		};

		private Transform renderedTransform;
		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;
		private Animator animator;

		private Material material;

		private GameClient gameClient;

		private Mesh mesh;

		private void Awake()
		{
			renderedTransform = transform.GetChild(0);
			
			mesh = new Mesh();
			meshRenderer = renderedTransform.GetComponent<MeshRenderer>();
			meshFilter = renderedTransform.GetComponent<MeshFilter>();
			
			animator = GetComponent<Animator>();
			
			gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
			
			meshFilter.sharedMesh = mesh;
		}

		private void Start()
		{
			material = meshRenderer.material;
			
			_cubeModeTransformSettings.Apply(renderedTransform);
		}

		private void Update()
		{
			// transform.eulerAngles = gameClient.player.playerCamera.transform.eulerAngles;
		}

		public void SetEnvLight(float skyLight, float blockLight)
		{
			material.SetVector("_EnvLight", new Vector4(skyLight, blockLight));
		}

		public void SetMeshFromMeta(BlockMeta meta)
		{
			var shapeMesh = meta.shape.GetGuiShapeMesh(out bool isCube, out int id, out int uvIdx);

			if (isCube)
			{
				_cubeModeTransformSettings.Apply(renderedTransform);
			}
			else
			{
				_planeModeTransformSettings.Apply(renderedTransform);

				var task = new VoxelizeTask(
					ShaderUniformManager.Instance.GetAtlasTextureById(id), 
					AtlasTextureDescriptor.Default.GetUVByIndex(uvIdx),
					AtlasTextureDescriptor.Default.UStep,
					AtlasTextureDescriptor.Default.VStep);
				shapeMesh = PixelVoxelizer.Do(task);
			}
			
			mesh.Clear();
			
			mesh.vertices = shapeMesh.positions;
			mesh.triangles = shapeMesh.triangles;
			mesh.uv = shapeMesh.texcoords;
			mesh.colors = shapeMesh.colors;
			mesh.RecalculateNormals();
		}

		public void PlayUseBlockAnimation()
		{
			animator.Play("HandUse");
		}
	}
}