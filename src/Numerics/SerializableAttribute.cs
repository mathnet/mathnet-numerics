#if PORTABLE
using System;

namespace MathNet.Numerics
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializableAttribute : Attribute
    {
    }
}
#endif
