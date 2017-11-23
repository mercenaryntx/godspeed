using System;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Core.Constants;

namespace Neurotoxin.Godspeed.Core.Models
{
    public class BinaryContainer
    {
        private byte[] Bytes { get; set; }

        public int Length
        {
            get { return Bytes.Length; }
        }

        public BinaryContainer(byte[] bytes)
        {
            Bytes = bytes ?? new byte[0];
        }

        public BinaryContainer(int size) : this(new byte[size])
        {
        }

        public byte this[int index]
        {
            get { return Bytes[index]; }
        }

        public byte this[uint index]
        {
            get { return Bytes[index]; }
        }

        public byte[] ReadAll()
        {
            return ReadBytes(0, Bytes.Length);
        }

        public byte[] ReadBytes(int pos)
        {
            return ReadBytes(pos, Bytes.Length - pos);
        }

        public byte[] ReadBytes(int pos, int length)
        {
            var buffer = new byte[length];
            Buffer.BlockCopy(Bytes, pos, buffer, 0, length);
            return buffer;
        }

        public void ReadBytes(int pos, byte[] dest, int destPos, int length)
        {
            Buffer.BlockCopy(Bytes, pos, dest, destPos, length);
        }

        public void WriteBytes(int pos, byte[] src)
        {
            WriteBytes(pos, src, 0, src.Length);
        }

        public void WriteBytes(int pos, byte[] src, int srcPos, int length)
        {
            EnsureBinarySize(pos + length);
            Buffer.BlockCopy(src, srcPos, Bytes, pos, length);
        }

        public void MoveBytes(int oldpos, int newpos, int length)
        {
            if (oldpos == newpos) return;

            EnsureBinarySize(newpos + length);
            Buffer.BlockCopy(Bytes, oldpos, Bytes, newpos, length);
            var diff = Math.Abs(newpos - oldpos);
            var zeros = new byte[diff];
            Buffer.BlockCopy(zeros, 0, Bytes, newpos > oldpos ? oldpos : newpos + length, diff);
        }

        public void Resize(int size)
        {
            var bytes = Bytes;
            Array.Resize(ref bytes, size);
            Bytes = bytes;
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, Bytes);
        }

        public void EnsureBinarySize(int size)
        {
            if (size <= Bytes.Length) return;
            Resize(size);
        }

        public int ReadInt(int pos, EndianType endianType = EndianType.LittleEndian)
        {
            var bytes = ReadBytes(pos, 4);
            if (endianType == EndianType.BigEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public uint ReadUInt(int pos, EndianType endianType = EndianType.LittleEndian)
        {
            var bytes = ReadBytes(pos, 4);
            if (endianType == EndianType.BigEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public MemoryStream GetStream()
        {
            return new MemoryStream(Bytes);
        }

    }
}