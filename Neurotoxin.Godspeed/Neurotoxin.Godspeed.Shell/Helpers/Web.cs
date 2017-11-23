using System;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Helpers
{
    public static class Web
    {
        public static bool Browse(string url)
        {
            try
            {
                Process.Start(url);
                return true;
            }
            catch (Exception ex)
            {
                var wm = UnityInstance.Container.Resolve<IWindowManager>();
                wm.ShowMessage(Resx.SystemError, string.Format(Resx.UrlCannotBeOpened, ex.Message));
                return false;
            }
        }
    }
}
