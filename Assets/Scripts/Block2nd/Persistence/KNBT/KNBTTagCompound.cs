using System;
using System.Collections.Generic;
using System.IO;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagCompound : KNBTBase
    {
        private Dictionary<string, KNBTBase> dict;

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
        
        public override byte GetId()
        {
            return 10;
        }

        public override void Read(BinaryReader reader)
        {
            KNBTBase tag;

            for (;
                (tag = NewTagFromTagId(reader.ReadByte(), reader.ReadString())).GetId() != 0;
                tag.Read(reader)) {}
        }

        public override void Write(BinaryWriter writer)
        {
            foreach (var pair in dict)
            {
                writer.Write(pair.Value.GetId());
                writer.Write(pair.Key);
                pair.Value.Write(writer);
            }
        }
    }
}