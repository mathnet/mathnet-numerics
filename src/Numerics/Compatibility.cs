using System.Globalization;

#if NET40
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
        private readonly string format;
        private readonly object[] args;

        public FormattableString(string format, object[] args)
        {
            this.format = format;
            this.args = args;
        }

        public static string Invariant(FormattableString messageFormat)
        {
            return messageFormat.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format, args);
        }

        public override string ToString()
        {
            return string.Format(format, args);
        }
    }
}
#endif
