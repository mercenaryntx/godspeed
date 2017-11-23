using System;
using System.IO;

namespace XeXtractor
{
    public class EndianWriter : BinaryWriter
    {
        private readonly EndianType endianStyle;

        public EndianWriter(Stream stream, EndianType endianStyle)
            : base(stream)
        {
            this.endianStyle = endianStyle;
        }

        public void SeekTo(int offset)
        {
            this.SeekTo((long)offset, SeekOrigin.Begin);
        }

        public void SeekTo(long offset)
        {
            this.SeekTo(offset, SeekOrigin.Begin);
        }

        public void SeekTo(long offset, SeekOrigin seekOrigin)
        {
            this.BaseStream.Seek(offset, seekOrigin);
        }

        public override void Write(short value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(short value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public override void Write(ushort value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(ushort value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public override void Write(int value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(int value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public override void Write(uint value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(uint value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public override void Write(long value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(long value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public override void Write(ulong value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(ulong value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public override void Write(float value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(float value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public override void Write(double value)
        {
            this.Write(value, this.endianStyle);
        }

        public void Write(double value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public void WriteAsciiString(string @string, int length)
        {
            this.WriteAsciiString(@string, length, this.endianStyle);
        }

        public void WriteAsciiString(string @string, int length, EndianType endianType)
        {
            int num = @string.Length;
            for (int i = 0; i < num && i <= length; i++)
            {
                this.Write((byte)@string[i]);
            }
            int num1 = length - num;
            if (num1 > 0)
            {
                this.Write(new byte[num1]);
            }
        }

        public void WriteInt24(int value)
        {
            this.WriteInt24(value, this.endianStyle);
        }

        public void WriteInt24(int value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Resize<byte>(ref bytes, 3);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(bytes);
            }
            base.Write(bytes);
        }

        public void WriteNullTermString(string @string)
        {
            this.WriteNullTermString(@string, this.endianStyle);
        }

        public void WriteNullTermString(string @string, EndianType endianType)
        {
            int length = @string.Length;
            for (int i = 0; i < length; i++)
            {
                this.Write((byte)@string[i]);
            }
            this.Write((byte)0);
        }

        public void WriteUnicodeNullTermString(string @string)
        {
            this.WriteUnicodeNullTermString(@string, this.endianStyle);
        }

        public void WriteUnicodeNullTermString(string @string, EndianType endianType)
        {
            int length = @string.Length;
            for (int i = 0; i < length; i++)
            {
                this.Write(@string[i], endianType);
            }
            this.Write((ushort)0, endianType);
        }

        public void WriteUnicodeString(string @string, int length)
        {
            this.WriteUnicodeString(@string, length, this.endianStyle);
        }

        public void WriteUnicodeString(string @string, int length, EndianType endianType)
        {
            int num = @string.Length;
            for (int i = 0; i < num && i <= length; i++)
            {
                this.Write(@string[i], endianType);
            }
            int num1 = (length - num) * 2;
            if (num1 > 0)
            {
                this.Write(new byte[num1]);
            }
        }
    }
}