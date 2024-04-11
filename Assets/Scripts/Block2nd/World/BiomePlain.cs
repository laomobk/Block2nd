using Block2nd.Database;

namespace Block2nd.World
{
    public class BiomePlain : IBiome
    {
        public void Decorate(Level level, int worldX, int worldZ)
        {
            int flowerId = BlockMetaDatabase.GetBlockCodeById("b2nd:block/small_flower_poppy");
            int grassId = BlockMetaDatabase.GetBlockCodeById("b2nd:block/plant_low_grass");
            
            int nTree = level.random.Next(0, 4);
            int nGrass = level.random.Next(10, 40);
            int nFlower = level.random.Next(10, 30);
            
            if (nTree <= 0 || nFlower <= 0)
                return;

            for (int i = 0; i < nFlower; ++i)
            {
                int x = worldX + level.random.Next(0, 15);
                int z = worldZ + level.random.Next(0, 15);
                int height = level.GetHeight(x, z);

                if (level.GetBlock(x, height, z).blockCode != BlockMetaDatabase.BuiltinBlockCode.Grass)
                {
                    continue;
                }
                
                level.SetBlockFast(flowerId, x, height + 1, z);
            }

            for (int i = 0; i < nGrass; ++i)
            {
                int x = worldX + level.random.Next(0, 15);
                int z = worldZ + level.random.Next(0, 15);
                int height = level.GetHeight(x, z);

                if (level.GetBlock(x, height, z).blockCode != BlockMetaDatabase.BuiltinBlockCode.Grass)
                {
                    continue;
                }
                
                level.SetBlockFast(grassId, x, height + 1, z);
            }

            for (int i = 0; i < nTree; ++i)
            {
                int treeX = worldX + level.random.Next(1, 12);
                int treeZ = worldZ + level.random.Next(1, 12);
                
                level.GrowTree(treeX, treeZ);
            }
        }
    }
}