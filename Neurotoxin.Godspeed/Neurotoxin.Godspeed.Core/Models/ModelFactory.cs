using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Castle.DynamicProxy;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Exceptions;
using Neurotoxin.Godspeed.Core.Extensions;

namespace Neurotoxin.Godspeed.Core.Models
{
    public static class ModelFactory
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();
        private static readonly IInterceptor[] ModelInterceptor = new IInterceptor[] {new ModelInterceptor()};
        private static readonly Dictionary<Type, OffsetTable> OffsetTables = new Dictionary<Type, OffsetTable>(); 

        public static OffsetTable GetOffsetTable(Type type)
        {
            if (!OffsetTables.ContainsKey(type))
                OffsetTables.Add(type, CalculateOffsetTable(type));

            return OffsetTables[type];
        }

        private static OffsetTable CalculateOffsetTable(Type type)
        {
            var offsetTable = new OffsetTable();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var declaredOnly = type.HasAttribute<DeclaredOnlyAttribute>();
            if (declaredOnly) flags |= BindingFlags.DeclaredOnly;
            var properties = type.GetProperties(flags);
            var offset = 0;
            foreach (var property in properties)
            {
                var attribute = property.GetAttribute<BinaryDataAttribute>(!declaredOnly);
                if (attribute == null) continue;

                if (attribute.Offset != -1) offset = attribute.Offset;
                var location = new BinaryLocation(offset);
                offsetTable.Add(property.Name, location);

                var propertyType = property.PropertyType;
                int length;

                if (attribute.Length.HasValue)
                {
                    if (propertyType.IsArray && BinaryModelBase.BaseType.IsAssignableFrom(propertyType.GetElementType()))
                    {
                        var size = GetOffsetTable(propertyType.GetElementType()).Size;
                        length = attribute.Length.Value * size;
                    }
                    else
                    {
                        length = attribute.Length.Value;
                    }
                }
                else if (BinaryModelBase.BaseType.IsAssignableFrom(propertyType))
                {
                    length = GetOffsetTable(propertyType).Size;
                }
                else if (attribute.StringReadOptions == StringReadOptions.NullTerminated)
                {
                    offsetTable.IsDynamic = true;
                    length = 1;
                }
                else if (propertyType == typeof(DateTime))
                {
                    length = 8;
                }
                else
                {
                    var valueType = propertyType.IsEnum ? Enum.GetUnderlyingType(propertyType) : propertyType;
                    length = valueType.IsValueType ? Marshal.SizeOf(valueType) : 0;
                }
                offset += length;
                location.Length = length;
            }
            return offsetTable;
        }

        public static T GetModel<T>(string filePath)
        {
            return (T)GetModel(typeof(T), new BinaryContainer(File.ReadAllBytes(filePath)), 0);
        }

        public static T GetModel<T>(byte[] binary = null, int startOffset = 0) where T : IBinaryModel
        {
            return (T)GetModel(typeof(T), new BinaryContainer(binary), startOffset);
        }

        public static T GetModel<T>(BinaryContainer binary, int startOffset = 0) where T : IBinaryModel
        {
            return (T) GetModel(typeof (T), binary, startOffset);
        }

        public static object GetModel(Type type, BinaryContainer binary, int startOffset)
        {
            try
            {
                var offsetTable = GetOffsetTable(type);
                binary.EnsureBinarySize(offsetTable.Size);
                return ProxyGenerator.CreateClassProxy(type, new object[] { offsetTable.Clone(startOffset) , binary, startOffset }, ModelInterceptor);
            }
            catch (TargetInvocationException ex)
            {
                throw new ModelFactoryException(ex.InnerException, "Binary model instantiation failed. See inner exception for details.");
            }
        }
    }
}