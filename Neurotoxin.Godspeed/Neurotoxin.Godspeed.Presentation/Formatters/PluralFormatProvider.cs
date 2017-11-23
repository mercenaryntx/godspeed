using System;
using System.Globalization;

namespace Neurotoxin.Godspeed.Presentation.Formatters
{
    public class PluralFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            return this;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!string.IsNullOrEmpty(format) && format.Contains(";"))
            {
                var value = (int) arg;
                var forms = format.Split(';');
                return value + " " + forms[value > 1 ? 1 : 0];
            }
            return null;
        }

    }
}