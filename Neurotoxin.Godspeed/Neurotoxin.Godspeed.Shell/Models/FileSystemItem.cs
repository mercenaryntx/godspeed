using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class FileSystemItem : INamed
    {
        public string Title { get; set; }
        public byte[] Thumbnail { get; set; }
        public ItemType Type { get; set; }
        public TitleType TitleType { get; set; }
        public ContentType ContentType { get; set; }
        public RecognitionState RecognitionState { get; set; }

        [IgnoreDataMember]
        public string Name { get; set; }
        [IgnoreDataMember]
        public string Path { get; set; }
        [IgnoreDataMember]
        public string FullPath { get; set; }
        [IgnoreDataMember]
        public long? Size { get; set; }
        [IgnoreDataMember]
        public DateTime Date { get; set; }
        [IgnoreDataMember]
        public bool IsCached { get; set; }
        [IgnoreDataMember]
        public bool IsLocked { get; set; }
        [IgnoreDataMember]
        public string LockMessage { get; set; }
        [IgnoreDataMember]
        public DriveType DriveType { get; set; }

        public FileSystemItem Clone()
        {
            return new FileSystemItem
                {
                    Title = Title,
                    Name = Name,
                    Thumbnail = Thumbnail,
                    Type = Type,
                    TitleType = TitleType,
                    ContentType = ContentType,
                    Path = Path,
                    FullPath = FullPath,
                    Size = Size,
                    Date = Date,
                    RecognitionState = RecognitionState
                };
        }

        public string GetRelativePath(string parent)
        {
            if (string.IsNullOrEmpty(parent)) return Path;
            var r = new Regex(Regex.Escape(parent));
            return r.Replace(Path, string.Empty, 1);
        }
    }
}