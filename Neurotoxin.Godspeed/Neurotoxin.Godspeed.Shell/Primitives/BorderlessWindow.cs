using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Primitives
{
    public class BorderlessWindow : Window
    {
        protected IUserSettingsProvider UserSettings { get; set; }

        public BorderlessWindow()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Neurotoxin.Godspeed.Shell;component/Resources/app.ico"));
            SnapsToDevicePixels = true;
            Background = (SolidColorBrush) Application.Current.Resources["ControlBackgroundBrush"];
            var useStyle = true;
            if (App.ShellInitialized)
            {
                UserSettings = UnityInstance.Container.Resolve<IUserSettingsProvider>();
                if (UserSettings.DisableCustomChrome) useStyle = false;
            }
            if (useStyle) Style = (Style)Application.Current.Resources["Window"];
        }
    }
}