using System;
using System.Text;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd.Entries
{
    public class SettingEntry : EntryBase
    {
        [BinaryData(4)]
        public virtual SettingId Id { get; set; }

        [BinaryData]
        public virtual ushort Time { get; set; }

        [BinaryData]
        public virtual ushort Unknown1 { get; set; }

        [BinaryData(1)]
        public virtual SettingEntryType Type { get; set; }

        [BinaryData(7)]
        public virtual byte[] Unknown2 { get; set; }

        [BinaryData(8)]
        protected virtual byte[] NumericValue { get; set; }

        [BinaryData]
        protected virtual byte[] BinaryValue { get; set; }

        //HACK: temporary
        private readonly SettingEntryType _typeCache;

        public SettingEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
            _typeCache = Type;
            if (_typeCache == SettingEntryType.Binary || _typeCache == SettingEntryType.UnicodeString)
                offsetTable["BinaryValue"].Length = GetValue<int>();
        }

        public T GetValue<T>()
        {
            return (T)GetValue(GetSettingTypeFromType(typeof(T)));
        }

        private object GetValue(SettingEntryType type)
        {
            switch (type)
            {
                case SettingEntryType.UnicodeString:
                    return ByteArrayExtensions.ToTrimmedString(BinaryValue, Encoding.BigEndianUnicode);
                case SettingEntryType.Binary:
                    return BinaryValue;
                case SettingEntryType.DateTime:
                    var buffer = new byte[8];
                    Buffer.BlockCopy(NumericValue, 0, buffer, 0, 8);
                    return ByteArrayExtensions.ToDateTime(buffer);
                case SettingEntryType.Int32:
                    return BitConverter.ToInt32(GetNumericValue(0, 4), 0);
                case SettingEntryType.Int64:
                    return BitConverter.ToUInt64(GetNumericValue(0, 8), 0);
                case SettingEntryType.Float:
                    return BitConverter.ToSingle(GetNumericValue(0, 4), 0);
                case SettingEntryType.Double:
                    return BitConverter.ToDouble(GetNumericValue(0, 8), 0);
                default:
                    throw new NotSupportedException("Invalid Type: " + type);
            }
        }

        private byte[] GetNumericValue(int offset, int bytes)
        {
            var buffer = new byte[bytes];
            Buffer.BlockCopy(NumericValue, offset, buffer, 0, bytes);
            Array.Reverse(buffer);
            return buffer;
        }

        public void SetValue<T>(T value)
        {
            SetValue(GetSettingTypeFromType(typeof(T)), value);
        }

        private void SetValue(SettingEntryType type, object value)
        {
            //TODO: what if new length != old
            byte[] buffer;
            switch (type)
            {
                case SettingEntryType.UnicodeString:
                case SettingEntryType.Binary:
                    buffer = type == SettingEntryType.UnicodeString
                                     ? Encoding.BigEndianUnicode.GetBytes((string) value)
                                     : (byte[]) value;
                    BinaryValue = buffer;
                    SetValue(BinaryValue.Length);
                    break;
                case SettingEntryType.DateTime:
                    buffer = ByteArrayExtensions.FromDateTime((DateTime)value);
                    Buffer.BlockCopy(buffer, 0, NumericValue, 0, 8);
                    break;
                case SettingEntryType.Int32:
                    SetNumericValue(BitConverter.GetBytes((int)value));
                    break;
                case SettingEntryType.Int64:
                    SetNumericValue(BitConverter.GetBytes((long)value));
                    break;
                case SettingEntryType.Float:
                    SetNumericValue(BitConverter.GetBytes((float)value));
                    break;
                case SettingEntryType.Double:
                    SetNumericValue(BitConverter.GetBytes((double)value));
                    break;
                default:
                    throw new NotSupportedException("Invalid Type: " + type);
            }
        }

        private void SetNumericValue(byte[] bytes)
        {
            Array.Reverse(bytes);
            Array.Resize(ref bytes, 8);
            NumericValue = bytes;
        }

        public static SettingEntryType GetSettingTypeFromType(Type type)
        {
            if (type == typeof(int)) return SettingEntryType.Int32;
            if (type == typeof(long)) return SettingEntryType.Int64;
            if (type == typeof(float)) return SettingEntryType.Float;
            if (type == typeof(double)) return SettingEntryType.Double;
            if (type == typeof(string)) return SettingEntryType.UnicodeString;
            if (type == typeof(byte[])) return SettingEntryType.Binary;
            if (type == typeof(DateTime)) return SettingEntryType.DateTime;
            throw new NotSupportedException("Unknown type: " + type.Name);
        }
    }
}