using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagByte : KNBTBase
    {
        public byte value;
        
        public KNBTTagByte(string tagName, byte value = 0) : base(tagName)
        {
            this.value = value;
        }

        public override byte GetId()
        {
            return 1;
        }

        public override void Read(BinaryReader reader)
        {
            value = reader.ReadByte();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}