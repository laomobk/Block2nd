using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public abstract class KNBTBase
    {
        public string tagName;

        protected KNBTBase(string tagName)
        {
            this.tagName = tagName;
        }

        public abstract void Read(BinaryReader reader);

        public abstract void Write(BinaryWriter writer);

        public abstract byte GetId();

        public KNBTBase NewTagFromTagId(byte id, string name)
        {
            switch (id)
            {
                case 1:
                    return new KNBTTagByte(name);
                case 3:
                    return new KNBTTagInt(name);
            }

            return null;
        }
    }
}