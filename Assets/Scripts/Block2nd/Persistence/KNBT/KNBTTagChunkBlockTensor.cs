using System.IO;
using Block2nd.World;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagChunkBlockTensor : KNBTBase
    {
        public ChunkBlockData[,,] value;
        
        public KNBTTagChunkBlockTensor(string tagName, ChunkBlockData[,,] value = null) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            int a = reader.ReadInt32();
            int b = reader.ReadInt32();
            int c = reader.ReadInt32();
            
            value = new ChunkBlockData[a, b, c];

            for (int i = 0; i < a; ++i)
            {
                for (int j = 0; j < b; ++j)
                {
                    for (int k = 0; k < c; ++k)
                    {
                        value[i, j, k].blockCode = reader.ReadInt32();
                        value[i, j, k].blockState = reader.ReadByte();
                    }
                }
            }
        }

        public override void Write(BinaryWriter writer)
        {
            int dim1 = value.GetLength(0);
            int dim2 = value.GetLength(1);
            int dim3 = value.GetLength(2);
            
            writer.Write(dim1);
            writer.Write(dim2);
            writer.Write(dim3);
            
            for (int i = 0; i < dim1; ++i)
            {
                for (int j = 0; j < dim2; ++j)
                {
                    for (int k = 0; k < dim3; ++k)
                    {
                        writer.Write(value[i, j, k].blockCode);
                        writer.Write(value[i, j, k].blockState);
                    }
                }
            }
        }

        public override byte GetId()
        {
            return 13;
        }
    }
}