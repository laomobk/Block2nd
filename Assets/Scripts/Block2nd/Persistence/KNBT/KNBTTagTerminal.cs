using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagTerminal : KNBTBase
    {
        public KNBTTagTerminal(string tagName) : base(tagName)
        {
        }

        public override byte GetId()
        {
            return 0;
        }

        public override void Read(BinaryReader reader)
        {
            
        }

        public override void Write(BinaryWriter writer)
        {
            
        }
    }
}