using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public struct FileExistenceInfo
    {
        public readonly bool Exists;
        public readonly long Size;

        private FileExistenceInfo(bool exists, long size)
        {
            Exists = exists;
            Size = size;
        }

        public static implicit operator bool(FileExistenceInfo info)
        {
            return info.Exists;
        }

        public static implicit operator FileExistenceInfo(bool exists)
        {
            return new FileExistenceInfo(exists, -1);
        }

        public static implicit operator long(FileExistenceInfo info)
        {
            return info.Size;
        }

        public static implicit operator FileExistenceInfo(long size)
        {
            return new FileExistenceInfo(size != -1, size);
        }

        public static implicit operator FileExistenceInfo(long? size)
        {
            return new FileExistenceInfo(size.HasValue, size ?? -1);
        }
    }
}
