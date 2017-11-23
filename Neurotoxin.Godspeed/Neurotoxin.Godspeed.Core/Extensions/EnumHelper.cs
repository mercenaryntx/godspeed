using System;
using System.Text.RegularExpressions;
using System.Linq;
using Neurotoxin.Godspeed.Core.Attributes;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    /// <summary>
    /// Enum helper class works with enums that use StringValue attribute on their fields.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the value from the instance of an enum based on it's StringValue attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum">The @enum.</param>
        /// <returns></returns>
        public static string GetStringValue<T>(T @enum)
        {
            var enumField = @enum.GetType().GetField(@enum.ToString());
            var stringValueAttribute = (StringValueAttribute)enumField.GetCustomAttributes(typeof(StringValueAttribute), false).FirstOrDefault();
            return stringValueAttribute == null ? @enum.ToString() : stringValueAttribute.Value;
        }

        /// <summary>
        /// Gets the field based on the string value and type of enum. It will try to match value against 
        /// StringValue attribute, if it doesn't find a match, it will look for a field with the name
        /// specified as a string. Before it does comparison, it strips whitespaces from the string, and 
        /// converts it to lowercase.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringValue">The string value.</param>
        /// <returns></returns>
        public static T GetField<T>(string stringValue)
        {
            T value;
            if (TryGetField(stringValue, out value)) return value;
            var exceptionString = string.Format("None of the fields in '{0}' match search value '{1}'", typeof(T).FullName, stringValue);
            throw new ArgumentException(exceptionString);
        }

        public static bool TryGetField<T>(string stringValue, out T value)
        {
            var type = typeof(T);
            var fields = type.GetFields();

            foreach (var fieldInfo in fields)
            {
                var stringValueAttribute = (StringValueAttribute)fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false).FirstOrDefault();

                if ((stringValueAttribute != null && stringValueAttribute.Value == stringValue) ||
                    (fieldInfo.Name.ToLower().Equals(SanitiseString(stringValue).ToLower())))
                {
                    value = (T) fieldInfo.GetValue(type);
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        private static string SanitiseString(string text)
        {
            var returnText = new Regex("[^a-zA-Z0-9]+").Replace(text, "");
            return returnText;
        }

        public static T SetFlag<T>(T @enum, T flag, bool value) where T : struct, IConvertible
        {
            var currentValue = Convert.ToInt32(@enum);
            var flagValue = Convert.ToInt32(flag);

            if (value)
                currentValue |= flagValue;
            else
                currentValue &= ~flagValue;

            return (T)Enum.ToObject(typeof(T), currentValue);
        }

        public static T AllEnabled<T>()
        {
            var type = typeof (T);
            var value = Enum.GetValues(type).Cast<int>().Aggregate(0, (current, field) => current | field);
            return (T)Enum.ToObject(type, value);
        }
    }
}