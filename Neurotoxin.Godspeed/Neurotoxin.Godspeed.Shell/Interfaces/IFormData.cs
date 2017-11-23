using System.IO;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFormData
    {
        string Name { get; set; }
        string FileName { get; set; }
        string ContentType { get; set; }

        void Write(StreamWriter sw);
    }
}