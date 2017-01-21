#if NETSTANDARD10

namespace MathNet.Numerics.Providers
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class SuppressUnmanagedCodeSecurityAttribute : Attribute
    {
    }  
}

#endif