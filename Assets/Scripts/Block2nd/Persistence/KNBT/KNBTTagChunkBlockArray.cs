using System.IO;
using Block2nd.World;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagChunkBlockArray : KNBTBase
    {
        public ChunkBlockData[] value;
        
        public KNBTTagChunkBlockArray(string tagName, ChunkBlockData[] value = null) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            int n = reader.ReadInt32();
            
            value = new ChunkBlockData[n];
            
            for (int i = 0; i < n; ++i)
            {
                value[i].blockCode = reader.ReadInt32();
                value[i].blockState = reader.ReadByte();
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value.Length);

            for (int i = 0; i < value.Length; ++i)
            {
                writer.Write(value[i].blockCode);
                writer.Write(value[i].blockState);
            }
        }

        public override byte GetId()
        {
            return 12;
        }
    }
}