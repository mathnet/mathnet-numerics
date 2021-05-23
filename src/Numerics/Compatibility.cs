#if NET40
using System.Globalization;

namespace System.Runtime.CompilerServices
{
    internal class FormattableStringFactory
    {
        public static FormattableString Create(string format, params object[] args)
        {
            return new FormattableString(format, args);
        }
    }
}

namespace System
{
    internal class FormattableString
    {
        readonly string _format;
        readonly object[] _args;

        public FormattableString(string format, object[] args)
        {
            _format = format;
            _args = args;
        }

        public static string Invariant(FormattableString messageFormat)
        {
            return messageFormat.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, _format, _args);
        }

        public override string ToString()
        {
            return string.Format(_format, _args);
        }
    }
}
#endif
