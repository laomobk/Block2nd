using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagShort : KNBTBase
    {
        public short value;
        
        public KNBTTagShort(string tagName, short value = 0) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            value = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }

        public override byte GetId()
        {
            return 2;
        }
    }
}