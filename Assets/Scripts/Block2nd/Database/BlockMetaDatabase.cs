using System.Collections;
using System.Collections.Generic;
using Block2nd.Audio;
using Block2nd.Behavior;
using Block2nd.Behavior.Block;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.Database
{
    public class BlockTypeBits
    {
        public static readonly int CubeBit = 1;
        public static readonly int NonCubeBit = 2;
        public static readonly int PlantBit = 4;
        public static readonly int LiquidBit = 8;
    }
    
    public class BlockMetaDatabase
    {
        public static readonly List<BlockMeta> blocks = new List<BlockMeta>();
        public static readonly List<BlockBehavior> behaviors = new List<BlockBehavior>();
        public static readonly List<int> types = new List<int>();

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

        public static int GetBlockOpacityByCode(int code)
        {
            if (code >= 0 && code < blocks.Count)
                return blocks[code].opacity;
            return 0;
        }
        
        public static int GetBlockLightByCode(int code)
        {
            if (code >= 0 && code < blocks.Count)
                return blocks[code].light;
            return 0;
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
            public static readonly int Grass = GetBlockCodeById("b2nd:block/grass");
            public static readonly int Stone = GetBlockCodeById("b2nd:block/stone");
            public static readonly int Rock = GetBlockCodeById("b2nd:block/rock");
            public static readonly int Wood = GetBlockCodeById("b2nd:block/wood");
            public static readonly int Dirt = GetBlockCodeById("b2nd:block/dirt");
            public static readonly int Sand = GetBlockCodeById("b2nd:block/sand");
            public static readonly int Water = GetBlockCodeById("b2nd:block/water");
        }

        public static void AddBlock(BlockMeta meta)
        {
            blocks.Add(meta);
            behaviors.Add(meta.behavior);

            int bits = 0;

            if (meta.liquid)
            {
                bits |= BlockTypeBits.LiquidBit;
            }
            
            if (meta.nonCube)
            {
                bits |= BlockTypeBits.LiquidBit;
            }
            else
            {
                bits |= BlockTypeBits.CubeBit;
            }

            if (meta.plant)
            {
                bits |= BlockTypeBits.PlantBit;
            }
            
            types.Add(bits);
        }

        static BlockMetaDatabase()
        {
            AddBlock(new BlockMeta
            {
                behavior = new NullBlockBehavior(),
                opacity = 0
            }); // Null

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/grass",
                blockName = "Grass",
                shape = CubeBlockShape.NewWithTexIdx(3, 3, 3, 3, 40, 2),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/stone",
                blockName = "Stone",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(1)),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/rock",
                blockName = "Rock",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = CubeBlockShape.NewWithTexIdx(16, 16, 16, 16, 16, 16),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/wood",
                blockName = "Wood",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigWood),
                shape = CubeBlockShape.NewWithTexIdx(4, 4, 4, 4, 4, 4),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/dirt",
                blockName = "Dirt",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigGravel),
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
                blockName = "Black Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(113)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/gray_wool",
                blockName = "Gray Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(114)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/red_wool",
                blockName = "Red Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(129)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/pink_wool",
                blockName = "Pink Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(130)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/dark_green_wool",
                blockName = "Dark Green Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(145)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/green_wool",
                blockName = "Green Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(146)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/brown_wool",
                blockName = "Brown Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(161)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/yellow_wool",
                blockName = "Yellow Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(162)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/blue_wool",
                blockName = "Blue Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(177)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/light_blue_wool",
                blockName = "Light Blue Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(178)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/purple_wool",
                blockName = "Purple Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(193)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/light_purple_wool",
                blockName = "Light Purple Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(194)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/cyan_wool",
                blockName = "Cyan Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(209)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/orange_wool",
                blockName = "Orange Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(210)),
            });


            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/light_gray_wool",
                blockName = "Light Gray Wool",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigCloth),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(225)),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/ork",
                blockName = "Ork",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigWood),
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
                opacity = 2
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/red_brick",
                blockName = "Red Brick",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(7)),
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/grass",
                blockName = "Grass",
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(49)),
                transparent = true,
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
                initState = 8,
                opacity = 2,
                
            });
            
            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/small_flower_poppy",
                blockName = "poppy",
                shape = new PlantShape(12),
                transparent = true,
                behavior = new PlantBehavior(),
                nonCube = true,
                plant = true,
                opacity = 0
            });
            
            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/plant_low_grass",
                blockName = "Low Grass",
                shape = new PlantShape(39),
                transparent = true,
                behavior = new PlantBehavior(),
                nonCube = true,
                plant = true,
                opacity = 0
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/gravel",
                blockName = "Gravel",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigGravel),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(19)),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/iron_block",
                blockName = "Iron Block",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(22)),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/gold_block",
                blockName = "Gold Block",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(23)),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/diamond_block",
                blockName = "Diamond Block",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(24)),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/bookshelf",
                blockName = "Bookshelf",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigWood),
                shape = CubeBlockShape.NewWithTexIdx(35, 35, 35, 35, 4, 4),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/mossy_cobblestone",
                blockName = "Mossy Cobblestone",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(36)),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/obsidian",
                blockName = "Obsidian",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(37)),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/sandstone",
                blockName = "Sandstone",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = CubeBlockShape.NewWithTexIdx(
                    192, 192, 192, 192, 176, 208),
                transparent = false,
            });

            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/stone_bricks",
                blockName = "Stone Bricks",
                behavior = new StaticBlockBehavior(BlockSoundDescriptor.SoundDigStone),
                shape = new CubeBlockShape(CubeBlockShape.CubeAppearance.NewSameFace(54)),
                transparent = false,
            });
            
            AddBlock(new BlockMeta
            {
                blockCode = blocks.Count,
                blockId = "b2nd:block/torch",
                blockName = "Torch",
                behavior = new StickBlockBehavior(BlockSoundDescriptor.SoundDigWood),
                shape = new StickShape(AtlasTextureDescriptor.Default.GetUVByIndex(80), new Color(0, 1, 0, 0)),
                transparent = true,
                light = 14,
                opacity = 0,
            });
        }
    }
}