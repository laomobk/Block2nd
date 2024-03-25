using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagLong : KNBTBase
    {
        public long value;
        
        public KNBTTagLong(string tagName, long value = 0) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            value = reader.ReadInt64();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }

        public override byte GetId()
        {
            return 4;
        }
    }
}