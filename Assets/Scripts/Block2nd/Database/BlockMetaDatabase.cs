using System.Collections;
using System.Collections.Generic;
using Block2nd.Behavior;
using Block2nd.Behavior.Block;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.Database
{
	public class BlockMetaDatabase
	{
		public static List<BlockMeta> blocks = new List<BlockMeta>();
		public static List<BlockBehavior> behaviors = new List<BlockBehavior>();

		public static BlockMeta GetBlockMetaById(string id)
		{
			foreach (var block in blocks)
			{
				if (block.blockId == id)
					return block;
			}

			return null;
		}
		
		public static int GetBlockCodeById(string id)
		{
			for (int i = 0; i < blocks.Count; ++i)
			{
				if (blocks[i].blockId == id)
					return i;
			}

			return 0;
		}

		public static BlockMeta GetBlockMetaByCode(int code)
		{
			// 0 is null block.
			if (code > 0 && code < blocks.Count)
				return blocks[code];
			return null;
		}
		
		public static BlockBehavior GetBlockBehaviorByCode(int code)
		{
			// 0 is null block.
			if (code >= 0 && code < behaviors.Count)
				return behaviors[code];
			return NullBlockBehavior.Default;
		}

		public static BlockMeta GetNextBlockMeta(int code)
		{
			return blocks[code % (blocks.Count - 1) + 1];
		}
		
		public static BlockMeta GetPrevBlockMeta(int code)
		{
			code--;
			if (code < 1)
				code = blocks.Count - 1;
			return blocks[code];
		}

		public class BuiltinBlockCode
		{
			public static readonly int Grass = 1;
			public static readonly int Stone = 2;
			public static readonly int Rock = 3;
			public static readonly int Wood = 4;
			public static readonly int Dirt = 5;
			public static readonly int Sand = 6;
		}

		public static void AddBlock(BlockMeta meta)
		{
			blocks.Add(meta);
			behaviors.Add(meta.behavior);
		}

		static BlockMetaDatabase()
		{
			AddBlock(new BlockMeta
			{
				behavior = new NullBlockBehavior()
			});  // Null
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/grass",
				blockName = "Grass",
				shape = CubeBlockShape.NewWithTexIdx(3, 3, 3, 3, 0, 2),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/stone",
				blockName = "Stone",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(1)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/rock",
				blockName = "Rock",
				shape = CubeBlockShape.NewWithTexIdx(16, 16, 16, 16, 16, 16),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/wood",
				blockName = "Wood",
				shape = CubeBlockShape.NewWithTexIdx(4, 4, 4, 4, 4, 4),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/dirt",
				blockName = "Dirt",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(2)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/sand",
				blockName = "Sand",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(18)),
				transparent = false,
				behavior = new SandBlockBehavior()
			});
			
			// Wools
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/red_wool",
				blockName = "Red Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(64)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/orange_wool",
				blockName = "Orange Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(65)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/yellow_wool",
				blockName = "Yellow Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(66)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/lime_wool",
				blockName = "Lime Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(67)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/green_wool",
				blockName = "Green Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(68)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/light_cyan_wool",
				blockName = "Light Cyan Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(69)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/cyan_wool",
				blockName = "Cyan Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(70)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/light_blue_wool",
				blockName = "Light Blue Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(71)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/purple_wool",
				blockName = "Purple Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(72)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/light_purple_wool",
				blockName = "Light Purple Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(73)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/light_pink_wool",
				blockName = "Light Pink Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(74)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/pink_wool",
				blockName = "Pink Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(75)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/bright_pink_wool",
				blockName = "Bright Pink Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(76)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/dark_gray_wool",
				blockName = "Dark Gray Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(77)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/gray_wool",
				blockName = "Gray Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(78)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/white_wool",
				blockName = "White Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(79)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/white_wool",
				blockName = "White Wool",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(79)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/ork",
				blockName = "Ork",
				shape = CubeBlockShape.NewWithTexIdx(20, 20, 20, 20, 21, 21),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/leaves",
				blockName = "Leaves",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(22)),
				transparent = true,
				forceRenderAllFace = true,
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/red_brick",
				blockName = "Red Brick",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(7)),
			});

			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/red_brick",
				blockName = "Red Brick",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(7)),
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/grass",
				blockName = "Grass",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(49)),
				transparent = true
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/tnt",
				blockName = "TNT",
				shape = CubeBlockShape.NewWithTexIdx(8, 8, 8, 8, 9, 10),
				transparent = false,
				behavior = new TntBlockBehavior()
			});
			
			AddBlock(new BlockMeta
			{
				blockCode = blocks.Count,
				blockId = "b2nd:block/water",
				blockName = "Water",
				shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(14)),
				transparent = true,
				behavior = new WaterBlockBehavior(),
				liquid = true,
			});
		}
	}
}

