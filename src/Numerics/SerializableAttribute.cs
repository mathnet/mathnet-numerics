using System;

namespace MathNet.Numerics
{
#if PORTABLE
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializableAttribute : Attribute
    {
    }
#endif
}
