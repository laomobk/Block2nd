using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Database
{
	public class AtlasTextureDescriptor
	{
		public static AtlasTextureDescriptor Default = new AtlasTextureDescriptor(new Vector2(16, 16));
		public Vector2 shape;

		public AtlasTextureDescriptor(Vector2 shape)
		{
			this.shape = shape;
		}
		

		public float UStep
		{
			get
			{
				return 1 / shape.x;
			}
		}

		public float VStep
		{
			get
			{
				return 1 / shape.y;
			}
		}

		public Vector2 GetUVByIndex(int idx)
		{
			var uStep = 1f / shape.x;
			var vStep = 1f / shape.y;

			return new Vector2(uStep * (idx % (int)shape.x), vStep * (shape.y - 1 - idx / (int)shape.y));
		}
	}
}

