using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using Neurotoxin.Godspeed.Shell.Interfaces;
using SharpMik.Drivers;
using SharpMik.Player;
using Xceed.Wpf.Toolkit;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class AboutDialog
    {
        private List<Hyperlink> _links;
        private MikMod _modPlayer;
        private readonly IResourceManager _resourceManager;

        public AboutDialog(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            SetVersion();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void SetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            Version.Text = "v" + assemblyName.Version;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Ok.Focus();
            _links = this.FindDescendants<RichTextBox>().SelectMany(b => b.Document.GetElementsOfType<Hyperlink>()).ToList();
            _links.ForEach(l =>
            {
                l.RequestNavigate += OnHyperlinkRequestNavigate;
            });
            _modPlayer = new MikMod();
            _modPlayer.Init<NaudioDriver>("");
            _modPlayer.Play(new MemoryStream(_resourceManager.GetContentByteArray("/Resources/Sounds/credits.xm")));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _links.ForEach(l =>
            {
                l.RequestNavigate -= OnHyperlinkRequestNavigate;
            });
            _modPlayer.Stop();
        }

        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}