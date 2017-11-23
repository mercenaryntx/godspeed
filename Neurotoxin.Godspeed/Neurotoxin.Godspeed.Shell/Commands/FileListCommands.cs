using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.Views;

namespace Neurotoxin.Godspeed.Shell.Commands
{
    public static class FileListCommands
    {
        public static readonly RoutedUICommand RenameTitleCommand = new RoutedUICommand("Rename Title", "RenameTitle", typeof(FileListPane), new InputGestureCollection
            {
                new KeyGesture(Key.F6, ModifierKeys.Shift)
            });

        public static readonly RoutedUICommand RenameFileSystemItemCommand = new RoutedUICommand("Rename File System Item", "RenameFileSystemItem", typeof(FileListPane), new InputGestureCollection
            {
                new KeyGesture(Key.F6, ModifierKeys.Shift | ModifierKeys.Control)
            });
    }
}