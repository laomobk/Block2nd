using System;
using System.Collections.Generic;
using System.IO;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagCompound : KNBTBase
    {
        private Dictionary<string, KNBTBase> dict = new Dictionary<string, KNBTBase>();

        public KNBTTagCompound(string tagName) : base(tagName) {}
        
        public KNBTTagCompound SetByteArray(string key, byte[] value)
        {
            dict.Add(key, new KNBTTagByteArray(key, value));

            return this;
        }
        
        public KNBTTagCompound SetInt(string key, int value)
        {
            dict.Add(key, new KNBTTagInt(key, value));

            return this;
        }

        public KNBTTagCompound SetByte(string key, byte value)
        {
            dict.Add(key, new KNBTTagByte(key, value));

            return this;
        }
        
        public KNBTTagCompound SetString(string key, string value)
        {
            dict.Add(key, new KNBTTagString(key, value));

            return this;
        }

        public KNBTTagCompound SetFloat(string key, float value)
        {
            dict.Add(key, new KNBTTagFloat(key, value));

            return this;
        }

        public KNBTTagCompound SetChunkBlockDataArray(string key, ChunkBlockData[] value)
        {
            dict.Add(key, new KNBTTagChunkBlockArray(key, value));

            return this;
        }

        public KNBTTagCompound SetChunkBlockDataTensor(string key, ChunkBlockData[,,] value)
        {
            dict.Add(key, new KNBTTagChunkBlockTensor(key, value));

            return this;
        }

        public int GetInt(string key, int defaultVal = 0)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is KNBTTagInt tagInt)
                {
                    return tagInt.value;
                }
            }

            return defaultVal;
        }
        
        public float GetFloat(string key, float defaultVal = 0f)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is KNBTTagFloat tagFloat)
                {
                    return tagFloat.value;
                }
            }

            return defaultVal;
        }
        
        public string GetString(string key, string defaultVal = "")
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is KNBTTagString tagString)
                {
                    return tagString.value;
                }
            }

            return defaultVal;
        }

        public byte[] GetByteArray(string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is KNBTTagByteArray tagByteArray)
                {
                    return tagByteArray.value;
                }
            }

            return null;
        }
        
        public ChunkBlockData[,,] GetChunkBlockDataTensor(string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is KNBTTagChunkBlockTensor tagChunkBlockTensor)
                {
                    return tagChunkBlockTensor.value;
                }
            }

            return null;
        }
        
        public override byte GetId()
        {
            return 10;
        }

        public override void Read(BinaryReader reader)
        {
            int c;
            for (c = 0; c < 256; ++c)
            {
                var id = reader.ReadByte();
                if (id == 0)
                    break;
                var name = reader.ReadString();
                var tag = NewTagFromTagId(id, name);
                tag.Read(reader);
                dict.Add(name, tag);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            foreach (var pair in dict)
            {
                writer.Write(pair.Value.GetId());
                writer.Write(pair.Key);
                pair.Value.Write(writer);
            }
            writer.Write((byte) 0);  // terminal
        }
    }
}