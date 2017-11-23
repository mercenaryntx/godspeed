using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class ProfileItemWrapper
    {
        public string Path { get; private set; }
        public FileSystemItem Item { get; private set; }
        public string ErrorMessage { get; private set; }

        public ProfileItemWrapper(string path, FileSystemItem item, string errorMessage)
        {
            Path = path;
            Item = item;
            ErrorMessage = errorMessage;
        }
    }
}
