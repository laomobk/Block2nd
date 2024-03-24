using System;
using System.IO;
using System.Reflection;

namespace Block2nd.Persistence.KNBT
{
    public class KNBTSerializer
    {
        private bool CheckSerializable(Type type)
        {
            MemberInfo info = type;
            return info.GetCustomAttribute<SerializableAttribute>() != null;
        }
    }
}