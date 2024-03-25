using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTTagCompound : KNBTBase
    {
        private Dictionary<string, KNBTBase> dict;

        public KNBTTagCompound(string tagName) : base(tagName) {}

        public void SetInt(string key, int value)
        {
            dict.Add(key, new KNBTTagInt(key, value));
        }

        public void SetByte(string key, byte value)
        {
            dict.Add(key, new KNBTTagByte(key, value));
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