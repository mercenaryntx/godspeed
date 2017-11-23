using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FileEntryViewModel : ViewModelBase
    {
        private FileEntry _model;

        public string Name
        {
            get { return _model.Name; }
        }

        public bool IsDirectory
        {
            get { return _model.IsDirectory; }
        }

        public int FileSize
        {
            get { return _model.FileSize; }
        }

        public DateTime Date
        {
            get { return DateTimeExtensions.FromFatFileTime(_model.AccessTimeStamp); }
        }

        private readonly ObservableCollection<FileBlockViewModel> _blocks;
        public ObservableCollection<FileBlockViewModel> Blocks
        {
            get { return _blocks; }
        }

        public StfsPackage Parent { get; private set; }

        public FileEntryViewModel(FileEntry model, IEnumerable<KeyValuePair<int?, BlockStatus>> blockList, StfsPackage parent)
        {
            Parent = parent;
            _model = model;
            _blocks = new ObservableCollection<FileBlockViewModel>();
            foreach (var block in blockList)
            {
                FileBlockHealthStatus status;
                if (!block.Key.HasValue) status = FileBlockHealthStatus.Missing;
                else
                {
                    switch (block.Value)
                    {
                        case BlockStatus.Allocated:
                        case BlockStatus.NewlyAllocated:
                            status = FileBlockHealthStatus.Ok;
                            break;
                        default:
                            status = FileBlockHealthStatus.Unallocated;
                            break;
                    }
                }
                var vm = new FileBlockViewModel(block.Key, status);
                _blocks.Add(vm);
            }
        }
    }
}