using System.IO;

namespace Neurotoxin.Godspeed.Shell.Reporting
{
    public class RawPostData : FormDataBase
    {
        public string Content { get; private set; }

        public RawPostData(string name, object content)
        {
            Name = name;
            Content = content.ToString();
        }

        public override void Write(StreamWriter sw)
        {
            base.Write(sw);
            sw.WriteLine(Content);
            sw.WriteLine();
        }
    }
}