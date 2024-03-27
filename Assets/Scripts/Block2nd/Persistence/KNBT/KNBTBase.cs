using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public enum KNBTTagType
    {
        TERMINAL = 0,
        BYTE = 1,
        SHORT = 2,
        STRING = 3,
        LONG = 4,
        FLOAT = 5,
        DOUBLE = 6,
        BYTE_ARRAY = 7,
        INT = 8,
        LIST = 9,
        COMPOUND = 10,
        INT_ARRAY = 11,
        CHUNK_BLOCK_ARRAY = 12,
        CHUNK_BLOCK_TENSOR = 13
    }
    
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
                case 0:
                    return new KNBTTagTerminal(name);
                case 1:
                    return new KNBTTagByte(name);
                case 2:
                    return new KNBTTagShort(name);
                case 3:
                    return new KNBTTagString(name);
                case 4:
                    return new KNBTTagLong(name);
                case 5:
                    return new KNBTTagFloat(name);
                case 6:
                    return new KNBTTagDouble(name);
                case 7:
                    return new KNBTTagByteArray(name);
                case 8:
                    return new KNBTTagInt(name);
                case 9:
                    return new KNBTTagList<KNBTBase>(name);
                case 10:
                    return new KNBTTagCompound(name);
                case 11:
                    return new KNBTTagIntArray(name);
                case 12:
                    return new KNBTTagChunkBlockArray(name);
                case 13:
                    return new KNBTTagChunkBlockTensor(name);
            }

            return null;
        }
    }
}