using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Shell.Tests.Dummies;

namespace Neurotoxin.Godspeed.Shell.Tests.Helpers
{
    public static class C
    {
        private static readonly Random RandomObject = new Random();

        public static T Fake<T>(object fakingRules = null)
        {
            var fake = A.Fake<T>();
            var type = typeof (T);
            var fakingType = fakingRules != null ? fakingRules.GetType() : null;
            foreach (var pi in type.GetProperties())
            {
                var value = GetFakingRuleValue(fakingRules, fakingType, pi.Name);
                if (value == null || value is Range)
                {
                    var generator = GetGenerator(pi.PropertyType);
                    int? min = null;
                    int? max = null;
                    if (value != null)
                    {
                        var r = (Range) value;
                        min = r.Min;
                        max = r.Max;
                    }
                    if (generator != null) value = generator(pi.PropertyType, min, max);
                }
                if (value != null && value.GetType() == pi.PropertyType) pi.SetValue(fake, value, null);
            }
            return fake;
        }

        public static IList<T> CollectionOfFake<T>(int? itemCount = null, object fakingRules = null)
        {
            var fakes = A.CollectionOfFake<T>(itemCount ?? Random<int>(2, 100));
            var type = typeof(T);
            var fakingType = fakingRules != null ? fakingRules.GetType() : null;
            foreach (var pi in type.GetProperties())
            {
                var value = GetFakingRuleValue(fakingRules, fakingType, pi.Name);
                var generator = GetGenerator(pi.PropertyType);
                if (value != null || generator != null)
                    fakes.ForEach(fake =>
                                      {
                                          int? min = null;
                                          int? max = null;
                                          if (value is Range)
                                          {
                                              var r = (Range) value;
                                              min = r.Min;
                                              max = r.Max;
                                          }
                                          pi.SetValue(fake, value != null && value.GetType() == pi.PropertyType ? value : generator(pi.PropertyType, min, max), null);
                                      });
            }
            return fakes;
        } 

        private static object GetFakingRuleValue(object fakingRules, Type fakingType, string propertyName)
        {
            var fakingProperty = fakingType != null ? fakingType.GetProperty(propertyName) : null;
            if (fakingProperty != null)
            {
                if (fakingProperty.PropertyType == typeof(Range))
                    return fakingProperty.GetValue(fakingRules, null);

                if (fakingProperty.PropertyType.IsArray)
                {
                    var a = (Array)fakingProperty.GetValue(fakingRules, null);
                    if (a.Length == 1)
                    {
                        var e = a.GetEnumerator();
                        e.MoveNext();
                        return e.Current;
                    } 
                    if (fakingProperty.PropertyType.GetElementType().IsEnum)
                    {
                        return RandomEnum(a, null, null);
                    }
                }
                throw new NotSupportedException(fakingProperty.PropertyType.Name + " is a not supported type for faking.");
            }
            return null;
        }

        public static T Random<T>()
        {
            return (T)Random(typeof(T), null, null);
        }

        public static T Random<T>(int max)
        {
            return (T)Random(typeof(T), null, max);
        }

        public static T Random<T>(int min, int max)
        {
            return (T) Random(typeof (T), min, max);
        }

        public static T Random<T>(Range range)
        {
            return (T)Random(typeof(T), range.Min, range.Max);
        }

        private static Func<Type, int?, int?, object> GetGenerator(Type type)
        {
            if (type.IsEnum) return RandomEnum;
            if (type == typeof(byte[])) return RandomByteArray;
            if (type == typeof(string)) return RandomString;

            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null) type = underlying;
            if (type == typeof(DateTime)) return RandomDateTime;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return RandomInt;
                case TypeCode.Decimal:
                case TypeCode.Double:
                    return RandomDouble;
                default:
                    return null;
            }
        }

        private static object Random(Type type, int? min, int? max)
        {
            var generator = GetGenerator(type);
            if (generator != null) return generator(type, min, max);

            throw new NotSupportedException("Not supported type parameter: " + type.Name);
        }

        private static object RandomInt(Type type, int? min, int? max)
        {
            if (min == null && max == null) return RandomObject.Next();
            if (min == null) return RandomObject.Next(max.Value);
            if (max == null) return RandomObject.Next(min.Value, Int32.MaxValue);
            return RandomObject.Next(min.Value, max.Value);
        }

        private static object RandomDouble(Type type, int? min, int? max)
        {
            var v = RandomObject.NextDouble();
            if (max.HasValue)
            {
                var minValue = min ?? 0;
                v = minValue + v * (max.Value - minValue);
            }
            return v;
        }

        private static object RandomByteArray(Type type, int? min, int? max)
        {
            var minValue = min ?? 10;
            var maxValue = max ?? minValue * 10;
            var b = new byte[Random<int>(minValue, maxValue)];
            RandomObject.NextBytes(b);
            return b;
        }

        private static object RandomString(Type type, int? min, int? max)
        {
            var returnString = new StringBuilder();
            var minValue = min ?? 1;
            var maxValue = max ?? minValue + 10;
            var length = Random<int>(minValue, maxValue);
            for (var i = 0; i < length; i++)
            {
                // 65 - 90 A-Z (26)
                // 97 - 122 a-Z (26)
                var randomIndex = RandomObject.Next(0, 51);
                returnString.Append(Convert.ToChar(randomIndex < 26 ? 65 + randomIndex : 97 + randomIndex - 26));
            }
            return returnString.ToString();
        }

        private static object RandomDateTime(Type type, int? min, int? max)
        {
            var d = DateTime.Now;
            var minValue = min ?? 0;
            var maxValue = max ?? minValue + 7 * 24 * 60 * 60;
            return d.AddSeconds(Random<int>(minValue, maxValue));
        }

        private static object RandomEnum(Type type, int? min, int? max)
        {
            return RandomEnum(Enum.GetValues(type), min, max);
        }

        private static object RandomEnum(Array values, int? min, int? max)
        {
            var minValue = min ?? 0;
            var maxValue = max ?? values.Length;
            if (minValue < 0 || minValue > values.Length - 1 || maxValue < 0 || maxValue > values.Length)
                throw new ArgumentException("Index was outside of bounds of Array");

            var index = Random<int>(minValue, maxValue);
            return values.GetValue(index);
        }

    }
}