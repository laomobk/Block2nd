namespace Block2nd.World
{
    public class BiomePlain : IBiome
    {
        public void Decorate(Level level, int worldX, int worldZ)
        {
            int nTree = level.random.Next(2, 4);

            for (int i = 0; i < nTree; ++i)
            {
                int treeX = worldX + level.random.Next(1, 12);
                int treeZ = worldZ + level.random.Next(1, 12);
                
                level.GrowTree(treeX, treeZ);
            }
        }
    }
}