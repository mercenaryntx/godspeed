using System;
using System.Collections.Generic;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Io.Gpd.Entries
{
    public class SyncList : List<SyncEntry>
    {
        public XdbfEntry Entry { get; set; }

        public BinaryContainer Binary { get; set; }

        public byte[] AllBytes
        {
            get { return Binary.ReadAll(); }
        }
    }

}