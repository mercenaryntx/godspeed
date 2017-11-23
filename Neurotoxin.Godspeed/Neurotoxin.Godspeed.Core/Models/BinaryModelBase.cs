using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Models
{
    public abstract class BinaryModelBase : IBinaryModel
    {
        public static readonly Type BaseType = typeof (BinaryModelBase);

        private readonly OffsetTable _offsetTable;
        private bool _cacheEnabled;

        public BinMap BinMap { get; set; }

        public BinaryContainer Binary { get; set; }
        public int StartOffset { get; set; }
        public virtual int BinarySize
        {
            get { return Binary.Length; }
        }

        public int OffsetTableSize
        {
            get { return _offsetTable.Size; }
        }

        public bool CacheEnabled
        {
            get { return _cacheEnabled; }
            set
            {
                _cacheEnabled = value;
                if (value) Cache = new Dictionary<string, object>();
            }
        }

        internal Dictionary<string, object> Cache { get; set; }

        protected BinaryModelBase(OffsetTable offsetTable, BinaryContainer binary, int startOffset)
        {
            CacheEnabled = true;
            _offsetTable = offsetTable;
            StartOffset = startOffset;
            BinMap = new BinMap();

            //HACK
            if (offsetTable == null) return;

            Binary = binary ?? new BinaryContainer(offsetTable.Size);

            EnsureNullTerminatedStrings();

            //Temp
            if (binary != null) offsetTable.MapOffsets(BinMap, GetType().BaseType);
        }

        public int GetPropertyOffset(string propertyName)
        {
            return _offsetTable[propertyName].Offset;
        }

        public T ReadPropertyValue<T>(string propertyName)
        {
            return ReadPropertyValue<T>(propertyName, null);
        }

        public T ReadPropertyValue<T>(string propertyName, Func<T> expression)
        {
            return (T)ReadPropertyValue(propertyName, expression.Method.GetAttribute<BinaryDataAttribute>() ?? new BinaryDataAttribute());
        }

        public object ReadPropertyValue(string propertyName, BinaryDataAttribute attribute = null)
        {
            var loc = _offsetTable[propertyName];
            //if (loc == null) return BypassCommand.Instance;

            var property = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            attribute = attribute ?? property.GetAttribute<BinaryDataAttribute>();

            byte[] buffer;
            if (attribute.StringReadOptions == StringReadOptions.NullTerminated && loc.Length == 1)
            {
                buffer = Binary.ReadBytes(loc.Offset);
                var encoded = attribute.Encoding.GetChars(buffer);
                var i = 0;
                while (i < encoded.Length && encoded[i] != 0) i++;
                if (i < encoded.Length)
                {
                    buffer = attribute.Encoding.GetBytes(encoded.Take(i).ToArray());
                    var diff = buffer.Length + 2 - loc.Length;
                    loc.Length = buffer.Length;
                    _offsetTable.ShiftOffsetsFrom(propertyName, diff);
                }
                else
                {
                    //throw new Exception("upsz, nem volt null");
                }
            }
            else
            {
                buffer = Binary.ReadBytes(loc.Offset, loc.Length);
            }

            return ReadValue(buffer, property.PropertyType, attribute);
        }

        public T ReadValue<T>(int offset, int length, BinaryDataAttribute attribute = null)
        {
            var buffer = Binary.ReadBytes(offset, length);
            return (T)ReadValue(buffer, typeof(T), attribute);
        }

        private static object ReadValue(byte[] buffer, Type propertyType, BinaryDataAttribute attribute = null)
        {
            var valueType = propertyType.IsEnum ? Enum.GetUnderlyingType(propertyType) : propertyType;

            if (propertyType.IsArray)
            {
                var elementType = propertyType.GetElementType();
                if (elementType == typeof(byte))
                {
                    if (attribute.EndianType == EndianType.SwapBytesBy4 ||
                        attribute.EndianType == EndianType.SwapBytesBy8)
                        buffer.SwapBytes((int)attribute.EndianType);
                    return buffer;
                }
                if (elementType.IsEnum)
                {
                    var array = Array.CreateInstance(propertyType.GetElementType(), buffer.Length);
                    for (var i = 0; i < buffer.Length; i++)
                        array.SetValue(Enum.ToObject(elementType, buffer[i]), i);
                    return array;
                }

                throw new NotSupportedException("Invalid array type: " + propertyType);
            }
            if (valueType == typeof(byte))
            {
                return buffer[0];
            }
            if (valueType == typeof(string))
            {
                switch (attribute.StringReadOptions)
                {
                    case StringReadOptions.AutoTrim:
                        return ByteArrayExtensions.ToTrimmedString(buffer, attribute.Encoding);
                    case StringReadOptions.ID:
                        return buffer.ToHex();
                    case StringReadOptions.NullTerminated:
                    case StringReadOptions.Default:
                        return attribute.Encoding.GetString(buffer);
                    default:
                        throw new NotSupportedException("Invalid StringReadOptions value: " + attribute.StringReadOptions);
                }
            }
            if (valueType == typeof(DateTime))
            {
                return ByteArrayExtensions.ToDateTime(buffer);
            }
            if (attribute.EndianType == EndianType.BigEndian)
                Array.Reverse(buffer);
            var valueSize = valueType.IsValueType ? Marshal.SizeOf(valueType) : (int?)null;
            if (valueSize.HasValue && buffer.Length < valueSize)
                Array.Resize(ref buffer, valueSize.Value);

            if (valueType == typeof (Version)) return ByteArrayExtensions.ToVersion(buffer);
            if (valueType == typeof(short)) return BitConverter.ToInt16(buffer, 0);
            if (valueType == typeof(ushort)) return BitConverter.ToUInt16(buffer, 0);
            if (valueType == typeof(int)) return BitConverter.ToInt32(buffer, 0);
            if (valueType == typeof(uint)) return BitConverter.ToUInt32(buffer, 0);
            if (valueType == typeof(long)) return BitConverter.ToInt64(buffer, 0);
            if (valueType == typeof(ulong)) return BitConverter.ToUInt64(buffer, 0);

            throw new NotSupportedException("Invalid value type: " + valueType);
        }

        private void EnsureNullTerminatedStrings()
        {
            var type = GetType();
            foreach (var key in _offsetTable.Keys)
            {
                var attribute = type.GetProperty(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetAttribute<BinaryDataAttribute>();
                if (attribute.StringReadOptions == StringReadOptions.NullTerminated && _offsetTable[key].Length == 1)
                {
                    ReadPropertyValue(key, attribute);
                }
            }
        }

        public void WritePropertyValue(string propertyName, object value)
        {
            var valueType = value.GetType();
            var loc = _offsetTable[propertyName];
            var property = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var attribute = property.GetAttribute<BinaryDataAttribute>();
            byte[] buffer;
            var endian = false;
            var noresize = false;
            if (attribute.StringReadOptions == StringReadOptions.NullTerminated)
            {
                buffer = attribute.Encoding.GetBytes((string)value + "\0");
                var length = buffer.Length - 2;
                var diff = length - loc.Length;

                _offsetTable.ShiftOffsetsFrom(propertyName, diff);
                var oldpos = loc.Offset + loc.Length;
                Binary.MoveBytes(oldpos, oldpos + diff, Binary.Length - oldpos);
                if (diff < 0) Binary.Resize(Binary.Length + diff);
                loc.Length = length;

                noresize = true;
            }
            else if (attribute.StringReadOptions == StringReadOptions.ID)
            {
                buffer = ((string)value).FromHex();
                Array.Reverse(buffer); //TODO: support endian in FromHex
            } 
            else if (value is string)
            {
                buffer = attribute.Encoding.GetBytes((string) value);
            }
            else if (value is DateTime)
            {
                buffer = ByteArrayExtensions.FromDateTime((DateTime) value);
            }
            else if (value is Version)
            {
                buffer = ByteArrayExtensions.FromVersion((Version)value);
            }
            else if (valueType.IsArray)
            {
                var elementType = valueType.GetElementType();
                if (elementType == typeof(byte)) buffer = (byte[])value;
                else if (elementType.IsEnum)
                {
                    buffer = new byte[loc.Length];
                    var array = (Array) value;
                    for(var i = 0; i < loc.Length; i++)
                        buffer[i] = Convert.ToByte(array.GetValue(i));
                }
                else
                    throw new NotSupportedException("Invalid array type: " + valueType);
            }
            else
            {
                if (value is Enum) valueType = Enum.GetUnderlyingType(valueType);

                if (valueType == typeof(byte)) buffer = new[] { (byte)value };
                else if (valueType == typeof(short)) buffer = BitConverter.GetBytes((short) value);
                else if (valueType == typeof(ushort)) buffer = BitConverter.GetBytes((ushort)value);
                else if (valueType == typeof(int)) buffer = BitConverter.GetBytes((int)value);
                else if (valueType == typeof(uint)) buffer = BitConverter.GetBytes((uint)value);
                else if (valueType == typeof(long)) buffer = BitConverter.GetBytes((long)value);
                else if (valueType == typeof(ulong)) buffer = BitConverter.GetBytes((ulong)value);
                else throw new NotSupportedException("Unknown value type: " + valueType.Name);
                endian = true;
            }

            if (!noresize) 
                Array.Resize(ref buffer, loc.Length);
            if (endian && attribute.EndianType == EndianType.BigEndian) 
                Array.Reverse(buffer);

            Binary.WriteBytes(loc.Offset, buffer, 0, loc.Length);
        }

        public void DebugOffsetTable()
        {
            foreach (var key in _offsetTable.Keys)
            {
                var entry = _offsetTable[key];
                Debug.WriteLine("{0}\t{1}\t{2}\t{3}", key, entry.Offset, entry.Length, ReadPropertyValue(key));
            }
        }

        public byte[] ReadAllBytes()
        {
            return Binary.ReadBytes(0, BinarySize);
        }
    }
}