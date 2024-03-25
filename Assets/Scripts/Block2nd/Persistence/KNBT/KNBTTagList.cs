using System.Collections.Generic;
using System.IO;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagList<T> : KNBTBase where T : KNBTBase
    {
        public List<T> list;
        
        public KNBTTagList(string tagName, List<T> list = null) : base(tagName)
        {
            if (list == null)
                list = new List<T>();
            this.list = list;
        }

        public override void Read(BinaryReader reader)
        {
            list.Clear();
            
            byte tagType = reader.ReadByte();
            int n = reader.ReadInt32();

            for (int i = 0; i < n; ++i)
            {
                var tag = NewTagFromTagId(tagType, "");
                tag.Read(reader);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            byte tagType;
            if (list.Count > 0)
            {
                tagType = list[0].GetId();
            }
            else
            {
                tagType = 1;
            }
            
            writer.Write(tagType);
            writer.Write(list.Count);

            foreach (var tag in list)
            {
                tag.Write(writer);
            }
        }

        public override byte GetId()
        {
            return 9;
        }
    }
}