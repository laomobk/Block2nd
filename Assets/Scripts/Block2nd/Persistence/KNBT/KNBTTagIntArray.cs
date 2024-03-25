using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagIntArray : KNBTBase
    {
        public int[] value;
        
        public KNBTTagIntArray(string tagName, int[] value = null) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            int n = reader.ReadInt32();
            value = new int[n];

            for (int i = 0; i < n; ++i)
            {
                value[i] = reader.ReadInt32();
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
            return 11;
        }
    }
}