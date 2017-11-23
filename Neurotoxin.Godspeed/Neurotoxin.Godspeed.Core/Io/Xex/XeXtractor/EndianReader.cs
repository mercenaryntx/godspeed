using System;
using System.IO;
using System.Text;

namespace XeXtractor
{
    public class EndianReader : BinaryReader
    {
        private readonly EndianType endianStyle;

        public EndianReader(Stream stream, EndianType endianStyle)
            : base(stream)
        {
            this.endianStyle = endianStyle;
        }

        public string ReadAsciiString(int length)
        {
            return this.ReadAsciiString(length, this.endianStyle);
        }

        public string ReadAsciiString(int length, EndianType endianType)
        {
            byte[] numArray = this.ReadBytes(length);
            int num = 0;
            while (num < length && numArray[num] != 0)
            {
                num++;
            }
            return Encoding.UTF8.GetString(numArray, 0, num);
        }

        public override double ReadDouble()
        {
            return this.ReadDouble(this.endianStyle);
        }

        public double ReadDouble(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(8);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToDouble(numArray, 0);
        }

        public override short ReadInt16()
        {
            return this.ReadInt16(this.endianStyle);
        }

        public short ReadInt16(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(2);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToInt16(numArray, 0);
        }

        public int ReadInt24()
        {
            return this.ReadInt24(this.endianStyle);
        }

        public int ReadInt24(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(3);
            if (endianType == EndianType.BigEndian)
            {
                return numArray[0] << 16 | numArray[1] << 8 | numArray[2];
            }
            return numArray[2] << 16 | numArray[1] << 8 | numArray[0];
        }

        public override int ReadInt32()
        {
            return this.ReadInt32(this.endianStyle);
        }

        public int ReadInt32(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(4);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToInt32(numArray, 0);
        }

        public override long ReadInt64()
        {
            return this.ReadInt64(this.endianStyle);
        }

        public long ReadInt64(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(8);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToInt64(numArray, 0);
        }

        public string ReadNullTerminatedString()
        {
            string empty = string.Empty;
            while (true)
            {
                byte num = this.ReadByte();
                byte num1 = num;
                if (num == 0 || num1 == 0)
                {
                    break;
                }
                empty = string.Concat(empty, (char)num1);
            }
            return empty;
        }

        public override float ReadSingle()
        {
            return this.ReadSingle(this.endianStyle);
        }

        public float ReadSingle(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(4);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToSingle(numArray, 0);
        }

        public override ushort ReadUInt16()
        {
            return this.ReadUInt16(this.endianStyle);
        }

        public ushort ReadUInt16(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(2);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToUInt16(numArray, 0);
        }

        public override uint ReadUInt32()
        {
            return this.ReadUInt32(this.endianStyle);
        }

        public uint ReadUInt32(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(4);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToUInt32(numArray, 0);
        }

        public override ulong ReadUInt64()
        {
            return this.ReadUInt64(this.endianStyle);
        }

        public ulong ReadUInt64(EndianType endianType)
        {
            byte[] numArray = base.ReadBytes(8);
            if (endianType == EndianType.BigEndian)
            {
                Array.Reverse(numArray);
            }
            return BitConverter.ToUInt64(numArray, 0);
        }

        public string ReadUnicodeNullTermString()
        {
            return this.ReadUnicodeNullTermString(this.endianStyle);
        }

        public string ReadUnicodeNullTermString(EndianType endianType)
        {
            string empty = string.Empty;
            while (true)
            {
                ushort num = this.ReadUInt16(endianType);
                if (num == 0)
                {
                    break;
                }
                empty = string.Concat(empty, (char)num);
            }
            return empty;
        }

        public string ReadUnicodeString(int length)
        {
            return this.ReadUnicodeString(length, this.endianStyle);
        }

        public string ReadUnicodeString(int length, EndianType endianType)
        {
            string empty = string.Empty;
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                ushort num1 = this.ReadUInt16(endianType);
                num++;
                if (num1 == 0)
                {
                    break;
                }
                empty = string.Concat(empty, (char)num1);
            }
            int num2 = (length - num) * 2;
            this.BaseStream.Seek((long)num2, SeekOrigin.Current);
            return empty;
        }

        public string ReadUTF16String(int length)
        {
            return this.ReadUTF16String(length, this.endianStyle);
        }

        public string ReadUTF16String(int length, EndianType endianType)
        {
            length = length * 2;
            byte[] numArray = this.ReadBytes(length);
            if (endianType != EndianType.LittleEndian)
            {
                for (int i = 0; i < length / 2; i++)
                {
                    byte num = numArray[2 * i];
                    numArray[2 * i] = numArray[2 * i + 1];
                    numArray[2 * i + 1] = num;
                }
            }
            return Encoding.Unicode.GetString(numArray, 0, length);
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
    }
}