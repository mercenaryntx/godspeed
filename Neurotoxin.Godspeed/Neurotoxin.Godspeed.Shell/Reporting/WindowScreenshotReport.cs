using System.IO;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Extensions;

namespace Neurotoxin.Godspeed.Shell.Reporting
{
    public class WindowScreenshotReport : FormDataBase
    {
        public Window Window { get; set; }

        public WindowScreenshotReport()
        {
            ContentType = "image/png";
            FileName = Name + ".png";
        }

        public override void Write(StreamWriter sw)
        {
            base.Write(sw);
            sw.Flush();
            var ms = Window.CaptureVisual();
            ms.Position = 0;
            ms.CopyTo(sw.BaseStream);
            sw.BaseStream.Flush();
            ms.Dispose();
            sw.WriteLine();
        }
    }
}