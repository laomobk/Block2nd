using System.IO;
using UnityEngine;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagString : KNBTBase
    {
        public string value;

        public KNBTTagString(string tagName, string value = "") : base(tagName)
        {
            this.value = value;
        }

        public override byte GetId()
        {
            return 3;
        }

        public override void Read(BinaryReader reader)
        {
            value = reader.ReadString();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}