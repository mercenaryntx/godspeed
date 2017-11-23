using System;
using System.IO;

namespace XeXtractor
{
    public class FileEntry
    {
        public string fileName = "";

        public string folder = "";

        public string type = "";

        public byte[] Data;

        public long id = (long)-1;

        public int iconId;

        public FileEntry()
        {
        }

        public void SaveAs(string file)
        {
            if ((int)this.Data.Length > 0 && this.type != "XACH")
            {
                string directoryName = Path.GetDirectoryName(file);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                File.WriteAllBytes(file, this.Data);
            }
        }

        public override string ToString()
        {
            return this.fileName.ToString();
        }
    }
}