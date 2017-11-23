using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.Views;

namespace Neurotoxin.Godspeed.Shell.Commands
{
    public static class FileManagerCommands
    {
        public static readonly RoutedUICommand OpenDriveDropdownCommand = new RoutedUICommand("Open Drive Dropdown", "OpenDriveDropdown", typeof(FileManagerWindow));
        public static readonly RoutedUICommand SettingsCommand = new RoutedUICommand("Settings...", "Settings", typeof(FileManagerWindow));
        public static readonly RoutedUICommand StatisticsCommand = new RoutedUICommand("Statistics...", "Statistics", typeof(FileManagerWindow));
        public static readonly RoutedUICommand AboutCommand = new RoutedUICommand("About", "About", typeof(FileManagerWindow));
        public static readonly RoutedUICommand VisitWebsiteCommand = new RoutedUICommand(string.Empty, "VisitWebsite", typeof(FileManagerWindow));
        public static readonly RoutedUICommand UserStatisticsParticipationCommand = new RoutedUICommand("User Statistics Participation", "UserStatisticsParticipation", typeof(FileManagerWindow));
        public static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Quit", "Quit", typeof(FileManagerWindow), new InputGestureCollection
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            });

        public static readonly RoutedUICommand SplitterCommand = new RoutedUICommand("Splitter", "Splitter", typeof(FileManagerWindow));
    }
}