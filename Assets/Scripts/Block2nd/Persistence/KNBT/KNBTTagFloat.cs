using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagFloat : KNBTBase
    {
        public float value;
        
        public KNBTTagFloat(string tagName, float value = 0f) : base(tagName)
        {
            this.value = value;
        }

        public override void Read(BinaryReader reader)
        {
            value = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }

        public override byte GetId()
        {
            return 5;
        }
    }
}