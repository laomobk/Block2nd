using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagDouble : KNBTBase
    {
        public double value;
        
        public KNBTTagDouble(string tagName, double value = 0) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            value = reader.ReadDouble();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }

        public override byte GetId()
        {
            return 6;
        }
    }
}