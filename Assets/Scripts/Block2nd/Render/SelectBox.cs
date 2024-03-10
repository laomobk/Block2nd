using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.Render
{
	public class SelectBox : MonoBehaviour
	{
		private MeshFilter meshFilter;

		private List<Vector3> posList = new List<Vector3>();
		private List<Vector2> uvList = new List<Vector2>();
		private List<int> triList = new List<int>();

		private void Awake()
		{
			meshFilter = GetComponent<MeshFilter>();
		}

		public void UpdateDetectBoxByShape(BlockShape shape, Vector3 position, int exposedFace)
		{
			posList.Clear();
			uvList.Clear();
			triList.Clear();

			var shapeMesh = shape.GetShapeMesh(exposedFace, 0);

			var mesh = new Mesh();

			for (int i = 0; i < shapeMesh.positionCount; ++i)
			{
				posList.Add(shapeMesh.positions[i]);
			}
			
			
			for (int i = 0; i < shapeMesh.texcoordCount; ++i)
			{
				uvList.Add(shapeMesh.texcoords[i]);
			}
			
			
			for (int i = 0; i < shapeMesh.triangleCount; ++i)
			{
				triList.Add(shapeMesh.triangles[i]);
			}

			mesh.vertices = posList.ToArray();
			mesh.triangles = triList.ToArray();
			mesh.uv = uvList.ToArray();

			transform.position = position;
			
			DestroyImmediate(meshFilter.sharedMesh, true);
			meshFilter.sharedMesh = mesh;
		}
	}
}
