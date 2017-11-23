namespace Neurotoxin.Godspeed.Core.Io
{
    public class PathInfo
    {
        public string Folder { get; private set; }
        public string Name { get; private set; }

        public PathInfo(string path, string slash = "\\")
        {
            if (path.EndsWith(slash)) path = path.Substring(0, path.Length - 1);
            var lastIndex = path.LastIndexOf(slash);
            if (lastIndex > -1)
            {
                Folder = path.Substring(0, lastIndex);
                if (!Folder.EndsWith(slash)) Folder += slash;
                Name = path.Substring(lastIndex + 1).TrimEnd();
            }
            else
            {
                Folder = slash;
                Name = path.TrimEnd();
            }
        }
    }
}