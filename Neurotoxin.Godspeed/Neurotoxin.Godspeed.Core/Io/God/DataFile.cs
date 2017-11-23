using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.God
{
    public class DataFile
    {
        public const int BlockLength = 0x1000;
        public static readonly byte[] BlankBlock = new byte[BlockLength];
        private readonly FileStream _fileStream;
        public HashTable MasterHashTable { get; private set; }

        public long Position
        {
            get { return _fileStream.Position; }
        }

        public long Length
        {
            get { return _fileStream.Length; }
        }

        public DataFile(string path)
        {
            _fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            WriteBlankBlock();
            MasterHashTable = ModelFactory.GetModel<HashTable>(new byte[BlockLength]);
        }

        public void WriteBlankBlock()
        {
            _fileStream.Write(BlankBlock, 0, BlockLength);
        }

        public void Write(byte[] buffer)
        {
            _fileStream.Write(buffer, 0, buffer.Length);
        }

        public void Seek(long offset, SeekOrigin seekOrigin)
        {
            _fileStream.Seek(offset, seekOrigin);
        }

        public void SetBlockHash(int index, byte[] hash)
        {
            MasterHashTable.Entries[index].BlockHash = hash;
        }

        public void WriteMasterHashBlock()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            var masterBytes = MasterHashTable.Binary.ReadAll();
            _fileStream.Write(masterBytes, 0, masterBytes.Length);
        }

        public void Close()
        {
            _fileStream.Close();
        }
    }
}