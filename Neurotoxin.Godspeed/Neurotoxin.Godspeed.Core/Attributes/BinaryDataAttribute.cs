using System;
using System.Text;
using Neurotoxin.Godspeed.Core.Constants;

namespace Neurotoxin.Godspeed.Core.Attributes
{
    public class BinaryDataAttribute : Attribute
    {
        public int? Length { get; set; }
        public EndianType EndianType { get; private set; }
        public Encoding Encoding { get; private set; }
        public StringReadOptions StringReadOptions { get; private set; }
        public int Offset { get; set; }

        public BinaryDataAttribute() : this(null, EndianType.BigEndian, null, StringReadOptions.Default) { }
        public BinaryDataAttribute(int length) : this(length, EndianType.BigEndian, null, StringReadOptions.Default) { }
        public BinaryDataAttribute(string encoding) : this(null, EndianType.BigEndian, encoding, StringReadOptions.Default) { }
        public BinaryDataAttribute(EndianType endianType) : this(null, endianType, null, StringReadOptions.Default) { }
        public BinaryDataAttribute(StringReadOptions stringReadOptions) : this(null, EndianType.BigEndian, null, stringReadOptions) { }
        public BinaryDataAttribute(int length, EndianType endianType) : this(length, endianType, null, StringReadOptions.Default) { }
        public BinaryDataAttribute(int length, string encoding) : this(length, EndianType.BigEndian, encoding, StringReadOptions.Default) { }
        public BinaryDataAttribute(int length, StringReadOptions stringReadOptions) : this(length, EndianType.BigEndian, null, stringReadOptions) { }
        public BinaryDataAttribute(int length, string encoding, StringReadOptions stringReadOptions) : this(length, EndianType.BigEndian, encoding, stringReadOptions) { }

        public BinaryDataAttribute(int? length, EndianType endianType, string encoding, StringReadOptions stringReadOptions)
        {
            Offset = -1;
            Length = length;
            EndianType = endianType;
            Encoding = String.IsNullOrEmpty(encoding) ? Encoding.BigEndianUnicode : Encoding.GetEncoding(encoding);
            StringReadOptions = stringReadOptions;
        }
    }
}