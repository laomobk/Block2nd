using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagByteArray : KNBTBase
    {
        public byte[] value;
        
        public KNBTTagByteArray(string tagName, byte[] value = null) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            int n = reader.ReadInt32();
            value = new byte[n];

            for (int i = 0; i < n; ++i)
            {
                value[i] = reader.ReadByte();
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value.Length);
            
            for (int i = 0; i < value.Length; ++i)
            {
                writer.Write(value[i]);
            }
        }

        public override byte GetId()
        {
            return 7;
        }
    }
}