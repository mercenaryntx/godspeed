using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Extensions;

namespace Neurotoxin.Godspeed.Core.Tests
{
    public static class BinaryAssert
    {
        public static void Assert<T>(T a, T b)
        {
            var type = typeof (T);
            foreach (var property in type.GetProperties())
            {
                var attribute = property.GetAttribute<BinaryDataAttribute>();
                if (attribute != null)
                {
                    var aValue = property.GetValue(a, null);
                    var bValue = property.GetValue(b, null);
                    if (property.PropertyType == typeof(byte[]))
                    {
                        aValue = ((byte[]) aValue).ToHex();
                        bValue = ((byte[]) bValue).ToHex();
                    }

                    var ok = aValue.Equals(bValue);
                    Debug.WriteLine("[{0}] - [{1}]", property.Name, ok);
                    Debug.WriteLine("A: {0}", aValue);
                    Debug.WriteLine("B: {0}", bValue);
                    Debug.WriteLine("-------------------\n");
                }
            }
        }
    }
}