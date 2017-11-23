using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neurotoxin.Godspeed.Presentation.Bindings;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class ThumbnailConverter : MultiBindingConverterBase<FileSystemItemViewModel, ImageSource>
    {
        private static readonly Dictionary<string, ImageSource> SystemIconCache = new Dictionary<string, ImageSource>();

        private int _thumbnailSize = 16;
        public int ThumbnailSize
        {
            get { return _thumbnailSize; }
            set { _thumbnailSize = value; }
        }

        public ThumbnailConverter() : base(new [] { "Thumbnail", "Type", "IsRefreshing" })
        {
        }

        protected override ImageSource Convert(FileSystemItemViewModel viewModel, Type targetType)
        {
            if (viewModel == null) return null;
            var rm = viewModel.ResourceManager;
            var bytes =  viewModel.IsUpDirectory ? null : viewModel.Thumbnail;
            if (bytes == null)
            {
                string icon;
                switch (viewModel.Type)
                {
                    case ItemType.Directory:
                        if (viewModel.IsUpDirectory) icon = "up";
                        else if (viewModel.Name == "0000000000000000") icon = "games_folder";
                        else if (viewModel.Model.RecognitionState == RecognitionState.PartiallyRecognized) icon = "xbox_logo";
                        else if (viewModel.IsRefreshing) icon = "refresh_folder";
                        else icon = "folder";
                        break;
                    case ItemType.Link:
                        icon = "reparse_point";
                        break;
                    case ItemType.File:
                        if (viewModel.IsCompressedFile) icon = "package";
                        else if (viewModel.IsIso) icon = "iso";
                        else if (viewModel.IsXex) icon = "xex";
                        else
                        {
                            ImageSource image = null;
                            var extension = Path.GetExtension(viewModel.Name);
                            if (!string.IsNullOrEmpty(extension))
                            {
                                var key = ThumbnailSize + extension;
                                if (SystemIconCache.ContainsKey(key))
                                {
                                    image = SystemIconCache[key];
                                }
                                else
                                {
                                    try
                                    {
                                        var shinfo = new ShellFileInfo();
                                        var path = viewModel.Path;
                                        if (!File.Exists(path))
                                        {
                                            path = key; //creating a temporary dummy file, i.e. 16.png
                                            using (File.Create(path)) {}
                                        }
                                        var size = ThumbnailSize == 16 ? (uint) 1 : 0;
                                        IconExtensions.SHGetFileInfo(path, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), 0x100 | size);
                                        if (path != viewModel.Path) File.Delete(path);
                                        if (IntPtr.Zero != shinfo.hIcon)
                                        {
                                            image = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    SystemIconCache.Add(key, image);
                                }
                                if (image != null) return image;
                            }
                            icon = "file";
                        }
                        break;
                    case ItemType.Drive:
                        switch (viewModel.Model.DriveType)
                        {
                            case DriveType.CDRom:
                                icon = "drive_cd";
                                break;
                            case DriveType.Network:
                                icon = "drive_network";
                                break;
                            case DriveType.Removable:
                                icon = "drive_flash";
                                break;
                            default:
                                icon = "drive";
                                break;
                        }
                        break;
                    default:
                        throw new NotSupportedException("Invalid item type: " + viewModel.Type);
                }
                bytes = rm.GetContentByteArray(string.Format("/Resources/Items/{0}x{0}/{1}.png", ThumbnailSize, icon));
            }
            return bytes[0] == 0 ? null : StfsPackageExtensions.GetBitmapFromByteArray(bytes);
        }
    }
}