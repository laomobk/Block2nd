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

		private void Awake()
		{
			meshFilter = GetComponent<MeshFilter>();
		}

		public void UpdateDetectBoxByShape(BlockShape shape, Vector3 position, int exposedFace)
		{
			var shapeMesh = shape.GetShapeMesh(exposedFace, 0);

			var mesh = new Mesh();
			mesh.vertices = shapeMesh.positions;
			mesh.triangles = shapeMesh.triangles;
			mesh.uv = shapeMesh.texcoords;

			transform.position = position;
			
			DestroyImmediate(meshFilter.sharedMesh, true);
			meshFilter.sharedMesh = mesh;
		}
	}
}
