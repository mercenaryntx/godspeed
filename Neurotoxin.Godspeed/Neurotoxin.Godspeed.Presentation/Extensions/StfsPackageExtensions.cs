using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Presentation.ViewModels;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class StfsPackageExtensions
    {
        public static ObservableCollection<TreeItemViewModel> BuildTreeFromFileListing(this StfsPackage package)
        {
            return new ObservableCollection<TreeItemViewModel> {BuildTree(package.FileStructure)};
        }

        public static BitmapImage GetThumbnailImage(this StfsPackage package)
        {
            return GetBitmapFromByteArray(package.ThumbnailImage);
        }

        public static BitmapImage GetTitleThumbnailImage(this StfsPackage package)
        {
            return GetBitmapFromByteArray(package.TitleThumbnailImage);
        }

        private static TreeItemViewModel BuildTree(FileEntry parent)
        {
            var treeItem = new TreeItemViewModel
                               {
                                   Name = parent.Name, 
                                   Children = new ObservableCollection<TreeItemViewModel>(),
                                   IsDirectory = parent.IsDirectory
                               };
            foreach (var folder in parent.Folders)
            {
                treeItem.Children.Add(BuildTree(folder));
            }
            foreach (var file in parent.Files)
            {
                treeItem.Children.Add(new TreeItemViewModel { Name = file.Name });
            }
            return treeItem;
        }

        public static BitmapImage GetBitmapFromByteArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            return bitmap;
        }


    }
}