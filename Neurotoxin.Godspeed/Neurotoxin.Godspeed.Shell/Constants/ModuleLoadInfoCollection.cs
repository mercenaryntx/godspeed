using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    public static class ModuleLoadInfoCollection
    {
        public static ModuleLoadInfo ProfileEditor = new ModuleLoadInfo
        {
            ModuleName = Modules.ProfileEditor,
            Title = "Profile Editor",
            LoadCommand = LoadCommand.Load,
            Singleton = true
        };

        public static ModuleLoadInfo HexViewer = new ModuleLoadInfo
        {
            ModuleName = Modules.HexViewer,
            Title = "Hex Viewer",
            LoadCommand = LoadCommand.Load,
            Singleton = true
        };
    }
}