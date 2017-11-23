using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Reporting
{
    public class ErrorReport : FormDataBase
    {
        public string ClientId { get; set; }
        public string ApplicationVersion { get; set; }
        public string FrameworkVersion { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string Details { get; set; }
        public string FtpLog { get; set; }

        public override void Write(StreamWriter sw)
        {
            base.Write(sw);
            sw.WriteLine("Client ID: " + ClientId);
            sw.WriteLine("GODspeed version: " + ApplicationVersion);
            sw.WriteLine("Framework version: " + FrameworkVersion);
            sw.WriteLine("OS version: " + OperatingSystemVersion);
            sw.WriteLine();

            sw.WriteLine(Details);
            sw.WriteLine();

            if (FtpLog == null) return;
            sw.WriteLine(FtpLog);
        }
    }
}