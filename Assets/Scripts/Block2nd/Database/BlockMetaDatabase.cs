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
            }); // Null

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
                blockId = "b2nd:block/black_wool",
                blockName = "Black",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(113)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/gray_wool",
                blockName = "Gray",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(114)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/red_wool",
                blockName = "Red",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(129)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/pink_wool",
                blockName = "Pink",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(130)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/dark_green_wool",
                blockName = "Dark Green",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(145)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/green_wool",
                blockName = "Green",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(146)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/brown_wool",
                blockName = "Brown",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(161)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/yello_wool",
                blockName = "Yello",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(162)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/blue_wool",
                blockName = "Blue",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(177)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/light_blue_wool",
                blockName = "Light Blue",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(178)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/purple_wool",
                blockName = "Purple",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(193)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/light_purple_wool",
                blockName = "Light Purple",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(194)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/cyan_wool",
                blockName = "Cyan",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(209)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/orange_wool",
                blockName = "Orange",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(210)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/light_gray_wool",
                blockName = "Light Gray",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(225)),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/ork",
                blockName = "ork",
                shape = CubeBlockShape.NewWithTexIdx(20, 20, 20, 20, 21, 21),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/leaves",
                blockName = "Leaves",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(52)),
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
                shape = new WaterBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(207)),
                transparent = true,
                behavior = new WaterBlockBehavior(),
                liquid = true,
                initState = 8
            });
        }
    }
}