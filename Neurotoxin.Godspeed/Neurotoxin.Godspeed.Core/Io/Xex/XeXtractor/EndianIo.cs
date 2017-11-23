using System;
using System.IO;

namespace XeXtractor
{
    public class EndianIo
    {
        private readonly bool isFile;

        private bool isOpen;

        private Stream stream;

        private readonly string filepath = string.Empty;

        private readonly EndianType endiantype = EndianType.LittleEndian;

        private EndianReader @in;

        private EndianWriter @out;

        public bool Closed
        {
            get
            {
                return !this.isOpen;
            }
        }

        public EndianReader In
        {
            get
            {
                return this.@in;
            }
        }

        public bool Opened
        {
            get
            {
                return this.isOpen;
            }
        }

        public EndianWriter Out
        {
            get
            {
                return this.@out;
            }
        }

        public long Position
        {
            get
            {
                return this.stream.Position;
            }
        }

        public Stream Stream
        {
            get
            {
                return this.stream;
            }
        }

        public EndianIo(string filePath, EndianType endianStyle)
        {
            this.endiantype = endianStyle;
            this.filepath = filePath;
            this.isFile = true;
        }

        public EndianIo(Stream stream, EndianType endianStyle)
        {
            this.endiantype = endianStyle;
            this.stream = stream;
            this.isFile = false;
        }

        public EndianIo(byte[] buffer, EndianType endianStyle)
        {
            this.endiantype = endianStyle;
            this.stream = new MemoryStream(buffer);
            this.isFile = false;
        }

        public void Close()
        {
            if (!this.isOpen)
            {
                return;
            }
            this.stream.Close();
            this.@in.Close();
            this.@out.Close();
            this.isOpen = false;
        }

        public void Open()
        {
            this.Open(FileMode.OpenOrCreate);
        }

        public void Open(FileMode fileMode)
        {
            if (this.isOpen)
            {
                return;
            }
            if (this.isFile)
            {
                this.stream = new FileStream(this.filepath, fileMode, FileAccess.ReadWrite);
            }
            this.@in = new EndianReader(this.stream, this.endiantype);
            this.@out = new EndianWriter(this.stream, this.endiantype);
            this.isOpen = true;
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
            this.stream.Seek(offset, seekOrigin);
        }

        public byte[] ToArray()
        {
            return ((MemoryStream)this.stream).ToArray();
        }
    }
}