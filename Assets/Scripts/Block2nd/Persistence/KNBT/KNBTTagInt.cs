using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagInt : KNBTBase
    {
        public int value;

        public KNBTTagInt(string tagName, int value = 0) : base(tagName)
        {
            this.value = value;
        }

        public override byte GetId()
        {
            return 8;
        }

        public override void Read(BinaryReader reader)
        {
            value = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}