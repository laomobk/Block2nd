using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Database.Meta;
using Block2nd.Model;
using Block2nd.Phys;
using Block2nd.Utils;
using UnityEngine;

namespace Block2nd.Render
{
	public class SelectBox : MonoBehaviour
	{
		private MeshFilter meshFilter;
		private Mesh mesh;

		private void Awake()
		{
			mesh = new Mesh();
			meshFilter = GetComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;
		}

		public void UpdateDetectBox(AABB aabb, Vector3 position, int exposedFace)
		{
			var meshBuilder = new MeshBuilder();

			var width = aabb.maxX - aabb.minX;
			var height = aabb.maxY - aabb.minY;
			var depth = aabb.maxZ - aabb.minZ;
			
			meshBuilder.SetQuadUV(Vector2.zero, 1, 1);
			
			meshBuilder.AddQuad(new Vector3(aabb.maxX, aabb.minY, aabb.maxZ), 
				Vector3.left, Vector3.up, width, height);
			meshBuilder.AddQuad(new Vector3(aabb.minX, aabb.minY, aabb.minZ), 
				Vector3.right, Vector3.up, width, height);
			meshBuilder.AddQuad(new Vector3(aabb.maxX, aabb.minY, aabb.minZ), 
				Vector3.forward, Vector3.up, depth, height);
			meshBuilder.AddQuad(new Vector3(aabb.minX, aabb.minY, aabb.maxZ), 
				Vector3.back, Vector3.up, depth, height);
			meshBuilder.AddQuad(new Vector3(aabb.maxX, aabb.maxY, aabb.maxZ), 
				Vector3.left, Vector3.back, width, depth);
			meshBuilder.AddQuad(new Vector3(aabb.minX, aabb.minY, aabb.minZ), 
				Vector3.right, Vector3.forward, width, depth);
			
			mesh.Clear();
			mesh.vertices = meshBuilder.vertices.ToArray();
			mesh.triangles = meshBuilder.indices.ToArray();
			mesh.uv = meshBuilder.texcoords.ToArray();

			transform.position = position;
		}
	}
}
