using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class OpenIsoPayload
    {
        public LocalFileSystemContentViewModel ParentPane { get; private set; }
        public string Path { get; private set; }

        public OpenIsoPayload(LocalFileSystemContentViewModel parentPane, string path)
        {
            ParentPane = parentPane;
            Path = path;
        }
    }
}
