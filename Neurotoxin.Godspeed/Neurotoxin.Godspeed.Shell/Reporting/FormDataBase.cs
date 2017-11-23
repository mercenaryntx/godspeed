using System.IO;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Reporting
{
    public class FormDataBase : IFormData
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public virtual void Write(StreamWriter sw)
        {
            var fileName = !string.IsNullOrEmpty(FileName) ? string.Format(" filename=\"{0}\";", FileName) : string.Empty;
            sw.WriteLine("Content-Disposition: form-data; name=\"{0}\"; {1}", Name, fileName);
            if (!string.IsNullOrEmpty(ContentType)) sw.WriteLine("Content-Type: {0}", ContentType);
            sw.WriteLine();
        }
    }
}