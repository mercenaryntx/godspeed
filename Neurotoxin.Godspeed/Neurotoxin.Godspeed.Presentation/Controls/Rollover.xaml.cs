using System.Windows;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for Rollover.xaml
    /// </summary>
    public partial class Rollover : UserControl
    {
        public Rollover()
        {
            InitializeComponent();
            this.IsEnabledChanged += OnEnabledChanged;
            OnEnabledChanged(this, new DependencyPropertyChangedEventArgs());
        }

        void OnEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsEnabled)
            {
                Visibility = Visibility.Visible;
                Messages.Children.Clear();
                ProgressIndicator.Start();
            } 
            else
            {
                Visibility = Visibility.Collapsed;
                ProgressIndicator.Stop();
            }
        }

        public void SetMessage(string section, string msg)
        {
            this.IsEnabled = true;
            int i = 0;
            while (i < Messages.Children.Count && ((TextBlock)Messages.Children[i]).Name != section) i++;
            if (i == Messages.Children.Count)
            {
                Messages.Children.Add(new TextBlock {Text = msg, Name = section});

            } else
            {
                ((TextBlock)Messages.Children[i]).Text = msg;
            }
        }
    }
}
