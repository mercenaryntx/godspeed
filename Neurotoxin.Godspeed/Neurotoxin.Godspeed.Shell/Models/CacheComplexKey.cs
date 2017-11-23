using System;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class CacheComplexKey
    {
        public string Key { get; set; }
        public long? Size { get; set; }
        public DateTime? Date { get; set; }
        public FileSystemItem Item { get; set; }
        public string ErrorMessage { get; set; }
    }
}