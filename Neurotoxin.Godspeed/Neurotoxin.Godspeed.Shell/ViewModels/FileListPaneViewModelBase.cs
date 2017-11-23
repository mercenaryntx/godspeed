using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Presentation.Formatters;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Constants.Comparers;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Core.Extensions;
using ServiceStack.OrmLite;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public abstract class FileListPaneViewModelBase<T> : PaneViewModelBase, IFileListPaneViewModel where T : IFileManager
    {
        public static string COMPUTEDNAME = "ComputedName";
        public static string COMPUTEDSIZE = "ComputedSize";
        public static string NAME = "Name";
        public static string DATE = "Date";

        private readonly object _queueLock = new object();
        private Queue<FileSystemItem> _queue;
        private readonly ItemTypeComparer _itemTypeComparer = new ItemTypeComparer();
        protected readonly T FileManager;
        protected readonly IUserSettingsProvider UserSettingsProvider;
        protected readonly Dictionary<FileSystemItemViewModel, string> PathCache = new Dictionary<FileSystemItemViewModel, string>();
        public readonly ITitleRecognizer TitleRecognizer;
        private readonly IDbContext _dbContext;

        #region Properties

        private const string ISINEDITMODE = "IsInEditMode";
        private bool _isInEditMode;
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                _isInEditMode = value; 
                NotifyPropertyChanged(ISINEDITMODE);
                ChangeDirectoryCommand.RaiseCanExecuteChanged();
            }
        }

        private const string ISINPATHEDITMODE = "IsInPathEditMode";
        private bool _isInPathEditMode;
        public bool IsInPathEditMode
        {
            get { return _isInPathEditMode; }
            set
            {
                _isInPathEditMode = value;
                NotifyPropertyChanged(ISINPATHEDITMODE);
            }
        }

        private const string DRIVES = "Drives";
        private ObservableCollection<FileSystemItemViewModel> _drives;
        public ObservableCollection<FileSystemItemViewModel> Drives
        {
            get { return _drives; }
            set { _drives = value; NotifyPropertyChanged(DRIVES); }
        }

        private const string DRIVE = "Drive";
        private FileSystemItemViewModel _drive;
        public FileSystemItemViewModel Drive
        {
            get { return _drive; }
            set
            {
                //TODO: refactor this setter

                if (IsDriveAccessible(value))
                {
                    if (CurrentFolder != null && _drive != value) PathCache[_drive] = CurrentFolder.Path;
                    _drive = value;
                }
                else
                {
                    var name = string.IsNullOrEmpty(value.Title)
                                   ? value.Name
                                   : string.Format("{0} ({1})", value.Title, value.Name);
                    WindowManager.ShowMessage(Resx.DriveChangeFailed, string.Format(Resx.DriveIsNotAccessible, name));
                    if (_drive == null) _drive = Drives.FirstOrDefault();
                }
                ChangeDrive();
                NotifyPropertyChanged(DRIVE);
            }
        }

        private const string DRIVELABEL = "DriveLabel";
        private string _driveLabel;
        public string DriveLabel
        {
            get { return _driveLabel; }
            set { _driveLabel = value; NotifyPropertyChanged(DRIVELABEL); }
        }

        private const string FREESPACETEXT = "FreeSpaceText";
        private string _freeSpaceText;
        public string FreeSpaceText
        {
            get { return _freeSpaceText; }
            set { _freeSpaceText = value; NotifyPropertyChanged(FREESPACETEXT); }
        }

        private const string CURRENTFOLDER = "CurrentFolder";
        private FileSystemItemViewModel _currentFolder;
        public FileSystemItemViewModel CurrentFolder
        {
            get { return _currentFolder; }
            set
            {
                _currentFolder = value; 
                NotifyPropertyChanged(CURRENTFOLDER);
                NotifyPropertyChanged(CURRENTPATH);
            }
        }

        private const string CURRENTPATH = "CurrentPath";
        public string CurrentPath
        {
            get { return CurrentFolder.Path; }
            set
            {
                //TODO
                
            }
        }

        private const string ITEMS = "Items";
        private ObservableCollection<FileSystemItemViewModel> _items;
        public ObservableCollection<FileSystemItemViewModel> Items
        {
            get { return _items; }
            private set { _items = value; NotifyPropertyChanged(ITEMS); }
        }

        public IEnumerable<FileSystemItemViewModel> SelectedItems
        {
            get { return Items.Where(item => item.IsSelected); }
        }

        private const string CURRENTROW = "CurrentRow";
        private FileSystemItemViewModel _currentRow;
        public FileSystemItemViewModel CurrentRow
        {
            get { return _currentRow; }
            set 
            {
                if (_currentRow == value) return;
                _currentRow = value; 
                NotifyPropertyChanged(CURRENTROW);
            }
        }

        private const string TITLECOLUMNHEADER = "TitleColumnHeader";
        public string TitleColumnHeader
        {
            get { return Resx.ResourceManager.GetString(Settings.DisplayColumnMode.ToString()); }
        }

        protected const string SIZEINFO = "SizeInfo";
        public string SizeInfo
        {
            get { return Items == null ? null : GetSizeInfo(); }
        }

        public long? FreeSpace
        {
            get { return FileManager.GetFreeSpace(Drive.Path); }
        }

        public ResumeCapability ResumeCapability { get; protected set; }

        public bool HasValidSelection
        {
            get { return SelectedItems.Any() || CurrentRow != null && !CurrentRow.IsUpDirectory; }
        }

        public ColumnMode DisplayColumnMode
        {
            get { return Settings.DisplayColumnMode; }
        }

        public ColumnMode EditColumnMode { get; set; }

        private const string VIEWMODE = "ViewMode";
        public FileListPaneViewMode ViewMode
        {
            get { return Settings.ViewMode; }
            set
            {
                Settings.ViewMode = value; 
                NotifyPropertyChanged(VIEWMODE);
                NotifyPropertyChanged(ISLISTVIEW);
                NotifyPropertyChanged(ISCONTENTVIEW);
            }
        }

        private const string ISLISTVIEW = "IsListView";
        public bool IsListView
        {
            get { return Settings.ViewMode == FileListPaneViewMode.List; }
            set
            {
                if (!value) return;
                ViewMode = FileListPaneViewMode.List;
            }
        }

        private const string ISCONTENTVIEW = "IsContentView";
        public bool IsContentView
        {
            get { return Settings.ViewMode == FileListPaneViewMode.Content; }
            set
            {
                if (!value) return;
                ViewMode = FileListPaneViewMode.Content;
            }
        }

        private const string ISSORTEDBYNAME = "IsSortedByName";
        public bool IsSortedByName
        {
            get { return Settings.DisplayColumnMode == ColumnMode.Name && Settings.SortByField == COMPUTEDNAME; }
            set
            {
                Settings.DisplayColumnMode = ColumnMode.Name;
                Settings.SortByField = COMPUTEDNAME;
                SortContent(true);
                NotifyPropertyChanged(TITLECOLUMNHEADER);
            }
        }

        private const string ISSORTEDBYTITLE = "IsSortedByTitle";
        public bool IsSortedByTitle
        {
            get { return Settings.DisplayColumnMode == ColumnMode.Title && Settings.SortByField == COMPUTEDNAME; }
            set
            {
                Settings.DisplayColumnMode = ColumnMode.Title;
                Settings.SortByField = COMPUTEDNAME;
                SortContent(true);
                NotifyPropertyChanged(TITLECOLUMNHEADER);
            }
        }

        private const string ISSORTEDBYDATE = "IsSortedByDate";
        public bool IsSortedByDate
        {
            get { return Settings.SortByField == DATE; }
            set
            {
                Settings.SortByField = DATE;
                SortContent(true);
            }
        }

        private const string ISSORTEDBYSIZE = "IsSortedBySize";
        public bool IsSortedBySize
        {
            get { return Settings.SortByField == COMPUTEDSIZE; }
            set
            {
                Settings.SortByField = COMPUTEDSIZE;
                SortContent(true);
            }
        }

        private const string ISINASCENDINGORDER = "IsInAscendingOrder";
        public bool IsInAscendingOrder
        {
            get { return Settings.SortDirection == ListSortDirection.Ascending; }
            set
            {
                Settings.SortDirection = ListSortDirection.Ascending;
                SortContent(true);
            }
        }

        private const string ISINDESCENDINGORDER = "IsInDescendingOrder";
        public bool IsInDescendingOrder
        {
            get { return Settings.SortDirection == ListSortDirection.Descending; }
            set
            {
                Settings.SortDirection = ListSortDirection.Descending;
                SortContent(true);
            }
        }

        private const string FAVORITES = "Favorites";
        private ObservableCollection<IMenuItemViewModel> _favorites;
        public ObservableCollection<IMenuItemViewModel> Favorites
        {
            get { return _favorites; }
            private set { _favorites = value; NotifyPropertyChanged(FAVORITES); }
        }

        public abstract bool IsReadOnly { get; }
        public abstract bool IsFSD { get; }
        public abstract bool IsVerificationEnabled { get; }
        public abstract bool IsFavoritesSupported { get; }

        public virtual int? ConnectionId
        {
            get { return null; }
        }

        #endregion

        #region ChangeDirectoryCommand

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

        private bool CanExecuteChangeDirectoryCommand(object cmdParam)
        {
            if (IsInEditMode || IsBusy) return false;
            return MouseEventArgsValidation(cmdParam) ?? KeyEventArgsValidation(cmdParam) ?? true;
        }

        private static bool? MouseEventArgsValidation(object cmdParam)
        {
            var mouseEvent = cmdParam as EventInformation<MouseEventArgs>;
            if (mouseEvent == null) return null;

            var e = mouseEvent.EventArgs;
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            return dataContext is FileSystemItemViewModel;
        }

        private static bool? KeyEventArgsValidation(object cmdParam)
        {
            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent == null) return null;

            var e = keyEvent.EventArgs;
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (!(dataContext is FileSystemItemViewModel)) return false;
            return e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None;
        }

        protected virtual void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null) keyEvent.EventArgs.Handled = true;
            try
            {
                Navigate(cmdParam as string);
            }
            catch (ApplicationException ex)
            {
                WindowManager.ShowMessage(Resx.IOError, ex.Message);
            }
        }

        private void Navigate(string path)
        {
            var canExecute = NavigateByGivenPath(path) ?? NavigateByCurrentRow();
            if (canExecute == false) return;
            ChangeDirectory();
        }

        private bool? NavigateByGivenPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            //FtpTrace.WriteLine("[NavigateByGivenPath] FOLDER EXISTS START");
            if (FileManager.FolderExists(path) == false)
            {
                throw new ApplicationException(Resx.PathNotFound + Strings.ColonSpace + path);

            }
            //FtpTrace.WriteLine("[NavigateByGivenPath] FOLDER EXISTS END");

            var drive = GetDriveFromPath(path);
            if (drive == null)
            {
                throw new ApplicationException(Resx.DriveNotFound);
            }

            var temp = PathCache.ContainsKey(drive) ? PathCache[drive] : null;
            PathCache[drive] = path;
            try
            {
                //FtpTrace.WriteLine("[NavigateByGivenPath] SET DRIVE START");
                Drive = drive;
                //FtpTrace.WriteLine("[NavigateByGivenPath] SET DRIVE END");
                return false;
            }
            catch
            {
                if (temp != null)
                {
                    PathCache[drive] = temp;
                }
                else
                {
                    PathCache.Remove(drive);
                }
                throw new ApplicationException(Resx.PathIsNotAccessible);
            }
        }

        private bool? NavigateByCurrentRow()
        {
            if (CurrentRow == null) return null;
            if (CurrentRow.Type == ItemType.File)
            {
                if (CurrentRow.IsCompressedFile) OpenCompressedFileCommand.Execute();
                //TODO: Do STFS check instead
                if (CurrentRow.TitleType == TitleType.Profile) OpenStfsPackageCommand.Execute(OpenStfsPackageMode.Browsing);
                return false;
            }
            var destination = CurrentRow.IsUpDirectory ? UpDirectory() : CurrentRow;
            if (destination == null) return false;
            if (destination.Name.Contains("?"))
            {
                WindowManager.ShowMessage(Resx.IOError, Resx.SpecialCharactersNotSupported);
                return false;
            }
            CurrentFolder = destination;
            return true;
        }

        protected abstract FileSystemItemViewModel GetDriveFromPath(string path);

        //public void ChangeDirectory(string path)
        //{
        //    var drive = Drives.First(d => d.Name == path.SubstringBefore(FileManager.Slash));
        //    Drive = drive;
            
        //    var model = FileManager.GetItemInfo(path);
        //    if (model == null) return;
        //    if (path == drive.Path) model.Type = ItemType.Drive;
        //    CurrentFolder = new FileSystemItemViewModel(model);
        //    ChangeDirectoryCommand.Execute(null);
        //}

        protected virtual void ChangeDirectory(string message = null, Action callback = null, bool refreshCache = false)
        {
            if (string.IsNullOrEmpty(message)) message = Resx.ChangingDirectory;
            ProgressMessage = message + Strings.DotDotDot;
            IsBusy = true;
            ExecuteCancelCommand();

            WorkHandler.Run(() => GetList(CurrentFolder.Path),
                result =>
                {
                    ChangeDirectoryCallback(result, refreshCache);
                    if (callback != null) callback.Invoke();
                },
                AsyncErrorCallback);
        }

        public virtual IList<FileSystemItem> GetList(string selectedPath)
        {
            //FtpTrace.WriteLine("[GetList] GET LIST START");
            var list = FileManager.GetList(selectedPath);
            //FtpTrace.WriteLine("[GetList] GET LIST END");
            list.ForEach(item => TitleRecognizer.RecognizeType(item));
            return list;
        }

        protected virtual void ChangeDirectoryCallback(IList<FileSystemItem> result, bool refreshCache)
        {
            //FtpTrace.WriteLine("[base.ChangeDirectoryCallback] START");
            IsBusy = false;
            SetAddRemoveCurrentDirName();
            var viewModels = result.Select(c => new FileSystemItemViewModel(c)).ToList();
            if (CurrentFolder.Type != ItemType.Drive)
            {
                var clone = CurrentFolder.Clone();
                clone.IsUpDirectory = true;
                viewModels.Insert(0, clone);
            }

            SortContent(viewModels);
            NotifyPropertyChanged(SIZEINFO);
            SetupTitleRecognition(result, refreshCache);
            //FtpTrace.WriteLine("[base.ChangeDirectoryCallback] END");
        }

        protected virtual void SetupTitleRecognition(IEnumerable<FileSystemItem> result, bool refreshCache)
        {
            lock (_queueLock)
            {
                _queue = new Queue<FileSystemItem>();
                if (!refreshCache) result = result.Where(item => item.TitleType != TitleType.Unknown && !TitleRecognizer.MergeWithCachedEntry(item));
                foreach (var item in result)
                {
                    _queue.Enqueue(item);
                }

                if (_queue.Count > 0)
                {
                    IsBusy = true;
                    RecognitionStart();
                }
                else
                {
                    RecognitionFinish();
                }
            }
        }

        #endregion

        #region OpenStfsPackageCommand

        public DelegateCommand<OpenStfsPackageMode> OpenStfsPackageCommand { get; private set; }

        private bool CanExecuteOpenStfsPackageCommand(OpenStfsPackageMode mode)
        {
            //TODO: Remove IsProfile once STFS detection is implemented
            return CurrentRow != null && CurrentRow.IsProfile && !CurrentRow.IsLocked;
        }

        private void ExecuteOpenStfsPackageCommand(OpenStfsPackageMode mode)
        {
            ProgressMessage = string.Format("{0} {1}...", Resx.OpeningProfile, CurrentRow.ComputedName);
            IsBusy = true;
            WorkHandler.Run(() => OpenStfsPackage(CurrentRow.Model), b => OpenStfsPackageCallback(b, mode), AsyncErrorCallback);
        }

        private BinaryContent OpenStfsPackage(FileSystemItem item)
        {
            if (item.IsLocked) throw new ApplicationException(item.LockMessage);

            var contentType = item.ContentType;
            var content = TitleRecognizer.GetBinaryContent(item);
            return new BinaryContent(item.Path, content, contentType);
        }

        private void OpenStfsPackageCallback(BinaryContent content, OpenStfsPackageMode mode)
        {
            PaneViewModelBase stfs;
            var data = new LoadDataAsyncParameters(Settings.Clone("/"), content);
            switch (mode)
            {
                case OpenStfsPackageMode.Browsing:
                    stfs = Container.Resolve<StfsPackageContentViewModel>();
                    break;
                case OpenStfsPackageMode.Repair:
                    stfs = Container.Resolve<ProfileRebuilderViewModel>();
                    break;
                default:
                    throw new NotSupportedException("Invalid mode: " + mode);
            }
            stfs.LoadDataAsync(LoadCommand.Load, data, OpenStfsPackageSuccess, OpenStfsPackageError);
        }

        private void OpenStfsPackageSuccess(PaneViewModelBase pane)
        {
            IsBusy = false;
            EventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
        }

        private void OpenStfsPackageError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            WindowManager.ShowMessage(Resx.OpenFailed, string.Format("{0}: {1}", string.Format(Resx.CantOpenFile, CurrentRow.ComputedName), exception.Message));
        }

        #endregion

        #region OpenCompressedFileCommand

        public DelegateCommand OpenCompressedFileCommand { get; private set; }

        protected virtual bool CanExecuteOpenCompressedFileCommand()
        {
            return CurrentRow != null && CurrentRow.IsCompressedFile;
        }

        private void ExecuteOpenCompressedFileCommand()
        {
            ProgressMessage = string.Format("{0} {1}...", Resx.OpeningArchive, CurrentRow.ComputedName);
            IsBusy = true;
            OpenCompressedFile(CurrentRow.Model.Path);
        }

        private void OpenCompressedFile(string path)
        {
            var archive = Container.Resolve<CompressedFileContentViewModel>();
            archive.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(Settings.Clone("/"), path), OpenCompressedFileSuccess, OpenCompressedFileError);
        }

        private void OpenCompressedFileSuccess(PaneViewModelBase pane)
        {
            IsBusy = false;
            EventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
        }

        private void OpenCompressedFileError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            WindowManager.ShowMessage(Resx.OpenFailed, string.Format("{0}: {1}", string.Format(Resx.CantOpenFile, CurrentRow.ComputedName), exception.Message));
        }

        #endregion

        #region CalculateSizeCommand

        public DelegateCommand<bool> CalculateSizeCommand { get; private set; }
        private Queue<FileSystemItemViewModel> _calculationQueue;
        private bool _calculationIsRunning;
        private bool _calculationIsAborted;

        private void ExecuteCalculateSizeCommand(bool calculateAll)
        {
            if (calculateAll)
            {
                if (_calculationQueue == null) _calculationQueue = new Queue<FileSystemItemViewModel>();
                foreach (var item in Items.Where(item => item.Type == ItemType.Directory && !item.IsUpDirectory && !_calculationQueue.Contains(item)))
                {
                    _calculationQueue.Enqueue(item);
                    item.IsRefreshing = true;
                }
            } 
            else if (CurrentRow.Type == ItemType.Directory)
            {
                if (_calculationQueue == null) _calculationQueue = new Queue<FileSystemItemViewModel>();
                if (!_calculationQueue.Contains(CurrentRow))
                {
                    _calculationQueue.Enqueue(CurrentRow);
                    CurrentRow.IsRefreshing = true;
                }
            }

            if (_calculationQueue == null || _calculationQueue.Count <= 0 || _calculationIsRunning) return;

            _calculationIsRunning = true;
            _calculationIsAborted = false;
            WorkHandler.Run(CalculateSize, CalculateSizeCallback, AsyncErrorCallback);
        }

        private bool CanExecuteCalculateSizeCommand(bool calculateAll)
        {
            return true;
        }

        private long CalculateSize()
        {
            return CalculateSize(_calculationQueue.Peek().Path);
        }

        public long CalculateSize(string path)
        {
            if (_calculationIsAborted) return 0;

            IList<FileSystemItem> list;
            try
            {
                list = FileManager.GetList(path);
            }
            catch
            {
                return 0;
            }
            return list.Where(item => item.Type == ItemType.File).Sum(fi => fi.Size.HasValue ? fi.Size.Value : 0)
                 + list.Where(item => item.Type == ItemType.Directory).Sum(di => CalculateSize(string.Format("{0}{1}/", path, di.Name)));
        }

        private void CalculateSizeCallback(long size)
        {
            lock (_calculationQueue)
            {
                var item = _calculationQueue.Dequeue();
                item.Size = size;
                item.IsRefreshing = false;
                if (!_calculationIsAborted && _calculationQueue.Count > 0)
                {
                    WorkHandler.Run(CalculateSize, CalculateSizeCallback, AsyncErrorCallback);
                }
                else
                {
                    _calculationQueue = null;
                    _calculationIsRunning = false;
                }
            }
            NotifyPropertyChanged(SIZEINFO);
        }

        private void CalculateSizeAbort()
        {
            if (!_calculationIsRunning) return;
            _calculationIsAborted = true;
            lock (_calculationQueue)
            {
                var item = _calculationQueue.Dequeue();
                _calculationQueue.ForEach(i => i.IsRefreshing = false);
                _calculationQueue.Clear();
                _calculationQueue.Enqueue(item);
            }
        }

        #endregion

        #region SortingCommand

        public DelegateCommand<EventInformation<DataGridSortingEventArgs>> SortingCommand { get; private set; }

        private void ExecuteSortingCommand(EventInformation<DataGridSortingEventArgs> cmdParam)
        {
            var e = cmdParam.EventArgs;
            var column = e.Column;
            e.Handled = true;
            Settings.SortByField = column.SortMemberPath;
            Settings.SortDirection = column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            SortContent();
            column.SortDirection = Settings.SortDirection;
        }

        public void SwitchTitleNameColumns(DataGridColumn column)
        {
            SwitchTitleNameColumns(column, Settings.DisplayColumnMode == ColumnMode.Title ? ColumnMode.Name : ColumnMode.Title);
        }

        public void SwitchTitleNameColumns(DataGridColumn column, ColumnMode mode)
        {
            if (Settings.DisplayColumnMode == mode) return;
            column.SortDirection = column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            Settings.DisplayColumnMode = mode;
            NotifyPropertyChanged(TITLECOLUMNHEADER);
        }

        private void SortContent(bool notifySort = false)
        {
            var selection = CurrentRow;
            SortContent(Items, notifySort);
            CurrentRow = selection;
        }

        private void SortContent(IEnumerable<FileSystemItemViewModel> content, bool notifySort = false)
        {
            if (content == null) return;

            var x = content.OrderBy(p => p.Type, _itemTypeComparer);
            if (Settings != null)
            {
                var sortBy = Settings.DisplayColumnMode == ColumnMode.Name && Settings.SortByField == COMPUTEDNAME ? "Name" : Settings.SortByField;
                x = x.ThenByProperty(sortBy, Settings.SortDirection);
            }
            var list = x.ToList();
            var up = list.FirstOrDefault(item => item.IsUpDirectory);
            if (up != null)
            {
                list.Remove(up);
                list.Insert(0, up);
            }

            var currentRow = CurrentRow;
            var oldItems = Items.ToList();
            Items.Clear();
            Items.AddRange(list);
            EventAggregator.GetEvent<FileListPaneViewModelItemsChangedEvent>().Publish(new FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction.Replace, list, oldItems, this));
            CurrentRow = currentRow;
            ResetCurrentRow();
            NotifyPropertyChanged(ISSORTEDBYNAME);
            NotifyPropertyChanged(ISSORTEDBYTITLE);
            NotifyPropertyChanged(ISSORTEDBYDATE);
            NotifyPropertyChanged(ISSORTEDBYSIZE);
            NotifyPropertyChanged(ISINASCENDINGORDER);
            NotifyPropertyChanged(ISINDESCENDINGORDER);
            if (notifySort) EventAggregator.GetEvent<FileListPaneViewModelContentSortedEvent>().Publish(new FileListPaneViewModelContentSortedEventArgs(this));
        }

        #endregion

        #region ToggleSelectionCommand

        public DelegateCommand<ToggleSelectionMode> ToggleSelectionCommand { get; private set; }

        private void ExecuteToggleSelectionCommand(ToggleSelectionMode mode)
        {
            CurrentRow.IsSelected = !CurrentRow.IsSelected;
            switch (mode)
            {
                case ToggleSelectionMode.Space:
                    if (CurrentRow.IsSelected) CalculateSizeCommand.Execute(false);    
                    break;
                case ToggleSelectionMode.Insert:
                case ToggleSelectionMode.ShiftDown:
                    {
                        var index = Items.IndexOf(CurrentRow);
                        if (index < Items.Count - 1) CurrentRow = Items[index + 1];
                    }
                    break;
                case ToggleSelectionMode.ShiftUp:
                    {
                        var index = Items.IndexOf(CurrentRow);
                        if (index > 0) CurrentRow = Items[index - 1];
                    }
                    break;
            }
            NotifyPropertyChanged(SIZEINFO);
        }

        #endregion

        #region SelectAllCommand

        public DelegateCommand<EventInformation<EventArgs>> SelectAllCommand { get; private set; }

        private void ExecuteSelectAllCommand(EventInformation<EventArgs> cmdParam)
        {
            Items.Where(row => !row.IsUpDirectory).ForEach(row => row.IsSelected = true);
            NotifyPropertyChanged(SIZEINFO);
        }

        #endregion

        #region InvertSelectionCommand

        public DelegateCommand<EventInformation<EventArgs>> InvertSelectionCommand { get; private set; }

        private void ExecuteInvertSelectionCommand(EventInformation<EventArgs> cmdParam)
        {
            Items.Where(row => !row.IsUpDirectory).ForEach(item => item.IsSelected = !item.IsSelected);
            NotifyPropertyChanged(SIZEINFO);
        }

        #endregion

        #region MouseSelectionCommand

        public DelegateCommand<EventInformation<MouseEventArgs>> MouseSelectionCommand { get; private set; }

        private void ExecuteMouseSelectionCommand(EventInformation<MouseEventArgs> eventInformation)
        {
            var e = eventInformation.EventArgs;
            var item = ((FrameworkElement)e.OriginalSource).DataContext as FileSystemItemViewModel;
            if (item == null) return;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                SelectIntervalOfItems(CurrentRow, item);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                item.IsSelected = true;
            }
            NotifyPropertyChanged(SIZEINFO);
        }

        private void SelectIntervalOfItems(FileSystemItemViewModel from, FileSystemItemViewModel to, bool value = true)
        {
            var start = Items.IndexOf(from);
            var end = Items.IndexOf(to);
            if (end < start)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }
            for (var i = start; i <= end; i++)
            {
                Items[i].IsSelected = value;
            }            
        }

        #endregion

        #region GoToFirstCommand

        public DelegateCommand<bool> GoToFirstCommand { get; private set; }

        private bool CanExecuteGoToFirstCommand(bool select)
        {
            return Items.Count > 0;
        }

        private void ExecuteGoToFirstCommand(bool select)
        {
            var first = Items.First();
            if (select) SelectIntervalOfItems(first, CurrentRow, !CurrentRow.IsSelected);
            CurrentRow = first;
        }

        #endregion

        #region GoToLastCommand

        public DelegateCommand<bool> GoToLastCommand { get; private set; }

        private bool CanExecuteGoToLastCommand(bool select)
        {
            return Items.Count > 0;
        }

        private void ExecuteGoToLastCommand(bool select)
        {
            var last = Items.Last();
            if (select) SelectIntervalOfItems(CurrentRow, last, !CurrentRow.IsSelected);
            CurrentRow = last;
        }

        #endregion

        #region RefreshTitleCommand

        public DelegateCommand RefreshTitleCommand { get; private set; }

        private bool CanExecuteRefreshTitleCommand()
        {
            return HasValidSelection;
        }

        private void ExecuteRefreshTitleCommand()
        {
            var selection = SelectedItems.Any() ? SelectedItems.Select(i => i.Model).ToList() : new List<FileSystemItem> { CurrentRow.Model };
            selection.ForEach(s =>
            {
                TitleRecognizer.ThrowCache(s);
                TitleRecognizer.RecognizeType(s);
            });

            lock (_queueLock)
            {
                if (_queue != null)
                {
                    selection.Where(i => !_queue.Contains(i)).ForEach(_queue.Enqueue);
                    var item = _queue.Peek();
                    ProgressMessage = string.Format(Resx.RecognizingItem, item.Name, _queue.Count - 1);
                }
                else
                {
                    _queue = new Queue<FileSystemItem>(selection);
                    IsBusy = true;
                    RecognitionStart();
                }
            }
        }

        #endregion

        #region RecognizeFromProfileCommand

        public DelegateCommand RecognizeFromProfileCommand { get; private set; }

        private bool CanExecuteRecognizeFromProfileCommand()
        {
            return CurrentRow != null && !CurrentRow.IsUpDirectory && CurrentRow.IsProfile && !CurrentRow.IsLocked;
        }

        private void ExecuteRecognizeFromProfileCommand()
        {
            IsBusy = true;
            ProgressMessage = Resx.ScanningProfile + Strings.DotDotDot;
            var vm = Container.Resolve<RecognizeFromProfileViewModel>(new ParameterOverride("titleRecognizer", TitleRecognizer));
            vm.Recognize(CurrentRow.Model, RecognizeFromProfileCallback, AsyncErrorCallback);
        }

        private void RecognizeFromProfileCallback(int count)
        {
            IsBusy = false;
            var message = count < 1 ? Resx.NoNewTitlesFound : string.Format(count > 1 ? Resx.NewTitleFoundPlural : Resx.NewTitleFoundSingular, count);
            WindowManager.ShowMessage(Resx.TitleRecognition, message);
            EventAggregator.GetEvent<RefreshPanesEvent>().Publish(new RefreshPanesEventArgs(this));
        }

        #endregion

        #region CopyTitleIdToClipboardCommand

        public DelegateCommand CopyTitleIdToClipboardCommand { get; private set; }

        private bool CanExecuteCopyTitleIdToClipboardCommand()
        {
            return CurrentRow != null && !CurrentRow.IsUpDirectory && CurrentRow.TitleType == TitleType.Game;
        }

        private void ExecuteCopyTitleIdToClipboardCommand()
        {
            Clipboard.SetData(DataFormats.Text, CurrentRow.Name);
        }

        #endregion

        #region SearchGoogleCommand

        public DelegateCommand SearchGoogleCommand { get; private set; }

        private bool CanExecuteSearchGoogleCommand()
        {
            return CurrentRow != null && !CurrentRow.IsUpDirectory && CurrentRow.TitleType == TitleType.Game;
        }

        private void ExecuteSearchGoogleCommand()
        {
            Web.Browse(string.Format("http://www.google.com/#q={0}", CurrentRow.Name));
        }

        #endregion

        #region SearchGoogleCommand

        public DelegateCommand SaveThumbnailCommand { get; private set; }

        private bool CanExecuteSaveThumbnailCommand()
        {
            return CurrentRow != null && CurrentRow.HasThumbnail;
        }

        private void ExecuteSaveThumbnailCommand()
        {
            var dialog = new SaveFileDialog
                {
                    Filter = "PNG (*.PNG)|*.png", 
                    FileName = CurrentRow.Name
                };
            if (dialog.ShowDialog() == true)
            {
                using (var stream = dialog.OpenFile())
                {
                    var bytes = CurrentRow.Thumbnail;
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            }
        }

        #endregion

        #region RefreshCommand

        public DelegateCommand RefreshCommand { get; private set; }

        private bool CanExecuteRefreshCommand()
        {
            return true;
        }

        protected void ExecuteRefreshCommand()
        {
            Refresh(true);
        }

        public override void Refresh(bool refreshCache)
        {
            Refresh(refreshCache, null);
        }

        public virtual void Refresh(bool refreshCache, Action callback)
        {
            ChangeDirectory(Resx.RefreshingDirectory, callback, refreshCache);
        }

        #endregion

        #region UpCommand

        public DelegateCommand UpCommand { get; private set; }

        private bool CanExecuteUpCommand()
        {
            return true;
        }

        private void ExecuteUpCommand()
        {
            if (CurrentFolder == Drive)
            {
                if (CloseCommand != null) CloseCommand.Execute();
            }
            else
            {
                var up = UpDirectory();
                if (up == null) return;
                CurrentFolder = up;
                ChangeDirectory();
            }
        }

        private FileSystemItemViewModel UpDirectory()
        {
            try
            {
                var parentPath = CurrentFolder.Path.GetParentPath();
                if (FileManager.FolderExists(parentPath))
                {
                    var type = parentPath == Drive.Path ? ItemType.Drive : ItemType.Directory;
                    var folder = FileManager.GetItemInfo(parentPath, type, false);
                    if (folder != null) return new FileSystemItemViewModel(folder);
                }

                WindowManager.ShowMessage(Resx.IOError, string.Format(Resx.ItemNotExistsOnPath, parentPath));
                return Drive;
            }
            catch (Exception ex)
            {
                AsyncErrorCallback(ex);
                return null;
            }
        }

        #endregion

        #region CancelCommand

        public DelegateCommand CancelCommand { get; private set; }

        private void ExecuteCancelCommand()
        {
            if (_calculationIsRunning) CalculateSizeAbort();
        }

        #endregion

        #region CloseCommand

        public DelegateCommand CloseCommand { get; protected set; }

        #endregion

        #region SelectDriveByInitialLetterCommand

        public DelegateCommand<EventInformation<KeyEventArgs>> SelectDriveByInitialLetterCommand { get; protected set; }

        public void ExecuteSelectDriveByInitialLetterCommand(EventInformation<KeyEventArgs> e)
        {
            if (e.EventArgs.Key < Key.A || e.EventArgs.Key > Key.Z) return;
            var key = e.EventArgs.Key.ToString();
            var drive = Drives.FirstOrDefault(d => d.Name.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
            if (drive == null) return;
            e.EventArgs.Handled = true;
            Drive = drive;
        }

        #endregion

        #region FileOperationCommand

        public DelegateCommand<FileOperation> FileOperationCommand { get; private set; }

        private bool CanExecuteFileOperationCommand(FileOperation action)
        {
            var e = new CanExecuteFileOperationEventArgs(this, action);
            EventAggregator.GetEvent<CanExecuteFileOperationEvent>().Publish(e);
            return !e.Cancelled && CurrentRow != null;
        }

        private void ExecuteFileOperationCommand(FileOperation action)
        {
            EventAggregator.GetEvent<ExecuteFileOperationEvent>().Publish(new ExecuteFileOperationEventArgs(action, this, null, new List<FileSystemItem> { CurrentRow.Model }));
        }

        #endregion

        #region SetViewCommand

        public DelegateCommand<FileListPaneViewMode> SetViewCommand { get; private set; }
 
        private void ExecuteSetViewCommand(FileListPaneViewMode viewMode)
        {
            ViewMode = viewMode;
        }

        #endregion

        #region FavoriteFolderMenuItemClickCommand

        public DelegateCommand<IMenuItemViewModel> FavoriteFolderMenuItemClickCommand { get; private set; }

        private void ExecuteFavoriteFolderMenuItemClickCommand(IMenuItemViewModel item)
        {
            if (item is AddRemoveCurrentDirViewModel)
            {
                var favorite = Favorites.OfType<FavoriteFolderViewModel>().FirstOrDefault(ff => ff.Path == CurrentPath);
                using (var db = _dbContext.Open())
                {
                    if (favorite != null)
                    {
                        Favorites.Remove(favorite);
                        if (Favorites.Count == 2) Favorites.RemoveAt(0);
                        db.DeleteById<FavoriteFolder>(favorite.Model.Id);
                        SetAddRemoveCurrentDirName();
                    }
                    else
                    {
                        var name = WindowManager.ShowTextInputDialog(Resx.Favorites, Resx.Name, CurrentFolder.Title ?? CurrentFolder.Name, null);
                        if (name == null) return;
                        if (name.Trim() == string.Empty)
                        {
                            WindowManager.ShowMessage(Resx.Error, Resx.CannotCreateFavoriteWithNoName);
                        }
                        else if (Favorites.OfType<FavoriteFolderViewModel>().Any(i => i.Name == name))
                        {
                            WindowManager.ShowMessage(Resx.Error, Resx.FavoriteWithNameAlreadyExists);
                        }
                        else
                        {
                            var fav = new FavoriteFolder
                            {
                                ConnectionId = ConnectionId,
                                Name = name,
                                Path = CurrentPath
                            };
                            db.Insert(fav);
                            SetFavorites(db);
                        }
                    }
                }
            }

            var favoriteFolder = item as FavoriteFolderViewModel;
            if (favoriteFolder != null)
            {
                if (favoriteFolder.IsCurrent) return;
                try
                {
                    Navigate(favoriteFolder.Path);
                }
                catch (ApplicationException ex)
                {
                    if (WindowManager.Confirm(Resx.IOError, ex.Message + Environment.NewLine + Resx.DoYouWantToRemoveItFromTheFavorites))
                    {
                        using (var db = _dbContext.Open())
                        {
                            Favorites.Remove(favoriteFolder);
                            if (Favorites.Count == 2) Favorites.RemoveAt(0);
                            db.DeleteById<FavoriteFolder>(favoriteFolder.Model.Id);
                            SetAddRemoveCurrentDirName();
                        }
                    }
                }
            }
        }

        #endregion

        protected FileListPaneViewModelBase()
        {
            FileManager = Container.Resolve<T>();
            UserSettingsProvider = Container.Resolve<IUserSettingsProvider>();
            TitleRecognizer = Container.Resolve<ITitleRecognizer>(new ParameterOverride("fileManager", FileManager));
            _dbContext = Container.Resolve<IDbContext>();

            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand, CanExecuteChangeDirectoryCommand);
            OpenStfsPackageCommand = new DelegateCommand<OpenStfsPackageMode>(ExecuteOpenStfsPackageCommand, CanExecuteOpenStfsPackageCommand);
            OpenCompressedFileCommand = new DelegateCommand(ExecuteOpenCompressedFileCommand, CanExecuteOpenCompressedFileCommand);
            CalculateSizeCommand = new DelegateCommand<bool>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            ToggleSelectionCommand = new DelegateCommand<ToggleSelectionMode>(ExecuteToggleSelectionCommand);
            SelectAllCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteSelectAllCommand);
            InvertSelectionCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteInvertSelectionCommand);
            MouseSelectionCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteMouseSelectionCommand);
            GoToFirstCommand = new DelegateCommand<bool>(ExecuteGoToFirstCommand, CanExecuteGoToFirstCommand);
            GoToLastCommand = new DelegateCommand<bool>(ExecuteGoToLastCommand, CanExecuteGoToLastCommand);
            RefreshTitleCommand = new DelegateCommand(ExecuteRefreshTitleCommand, CanExecuteRefreshTitleCommand);
            RecognizeFromProfileCommand = new DelegateCommand(ExecuteRecognizeFromProfileCommand, CanExecuteRecognizeFromProfileCommand);
            CopyTitleIdToClipboardCommand = new DelegateCommand(ExecuteCopyTitleIdToClipboardCommand, CanExecuteCopyTitleIdToClipboardCommand);
            SearchGoogleCommand = new DelegateCommand(ExecuteSearchGoogleCommand, CanExecuteSearchGoogleCommand);
            SaveThumbnailCommand = new DelegateCommand(ExecuteSaveThumbnailCommand, CanExecuteSaveThumbnailCommand);
            RefreshCommand = new DelegateCommand(ExecuteRefreshCommand, CanExecuteRefreshCommand);
            UpCommand = new DelegateCommand(ExecuteUpCommand, CanExecuteUpCommand);
            CancelCommand = new DelegateCommand(ExecuteCancelCommand);
            SelectDriveByInitialLetterCommand = new DelegateCommand<EventInformation<KeyEventArgs>>(ExecuteSelectDriveByInitialLetterCommand);
            FileOperationCommand = new DelegateCommand<FileOperation>(ExecuteFileOperationCommand, CanExecuteFileOperationCommand);
            SetViewCommand = new DelegateCommand<FileListPaneViewMode>(ExecuteSetViewCommand);
            FavoriteFolderMenuItemClickCommand = new DelegateCommand<IMenuItemViewModel>(ExecuteFavoriteFolderMenuItemClickCommand);

            Items = new ObservableCollection<FileSystemItemViewModel>();
            ResumeCapability = ResumeCapability.Both;

            EventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnTransferProgressChanged);
        }

        public abstract string GetTargetPath(string path);
        public void Abort()
        {
            FileManager.Abort();
        }

        protected void Initialize()
        {
            //FtpTrace.WriteLine("[Initialize]");
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
            //FtpTrace.WriteLine("[/Initialize]");
        }

        protected void SetFavorites()
        {
            using (var db = _dbContext.Open())
            {
                SetFavorites(db);
            }
        }

        private void SetFavorites(IDbConnection db)
        {
            Favorites = db.Get<FavoriteFolder>()
                          .Where(ff => ff.ConnectionId == ConnectionId)
                          .Select(ff => new FavoriteFolderViewModel(ff))
                          .ToObservableCollection<IMenuItemViewModel>();
            if (Favorites.Count > 0) Favorites.Add(null);
            Favorites.Add(new AddRemoveCurrentDirViewModel());
            SetAddRemoveCurrentDirName();
        }

        private void SetAddRemoveCurrentDirName()
        {
            if (Favorites == null || CurrentFolder == null) return;
            FavoriteFolderViewModel fav = null;
            foreach (var favorite in Favorites.OfType<FavoriteFolderViewModel>())
            {
                if (favorite.Path == CurrentPath)
                {
                    fav = favorite;
                    fav.IsCurrent = true;
                }
                else
                {
                    favorite.IsCurrent = false;
                }
            }
            Favorites.Last().Name = fav != null ? Resx.RemoveCurrentDir : Resx.AddCurrentDir;
        }

        public override void Dispose()
        {
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Unsubscribe(OnTransferProgressChanged);
            if (CurrentFolder != null) Settings.Directory = CurrentFolder.FullPath;
            base.Dispose();
        }

        public override void SetActive()
        {
            base.SetActive();
            ResetCurrentRow();
        }

        protected void ResetCurrentRow()
        {
            FileSystemItemViewModel currentRow = null;
            if (CurrentRow != null) currentRow = Items.FirstOrDefault(item => item.Path == CurrentRow.Path);
            if (currentRow == null) currentRow = Items.FirstOrDefault();
            CurrentRow = currentRow;
            RaiseCanExecuteChanges();
        }

        protected virtual string GetSizeInfo()
        {
                var selectedSize = Items.Where(item => item.Size != null && item.IsSelected).Sum(item => item.Size.Value);
                var totalSize = Items.Where(item => item.Size != null).Sum(item => item.Size.Value);
                var selectedFileCount = Items.Count(item => item.Type == ItemType.File && item.IsSelected);
                var totalFileCount = Items.Count(item => item.Type == ItemType.File);
                var selectedDirCount = Items.Count(item => item.Type == ItemType.Directory && item.IsSelected);
                var totalDirCount = Items.Count(item => item.Type == ItemType.Directory && !item.IsUpDirectory);

                return string.Format(new PluralFormatProvider(), Resx.SizeInfo, selectedSize, totalSize, selectedFileCount, totalFileCount, selectedDirCount, totalDirCount);
        }

        public FileExistenceInfo FileExists(string path)
        {
            return FileManager.FileExists(path);
        }

        public virtual TransferResult Delete(FileSystemItem item)
        {
            if (item.Type == ItemType.File)
            {
                FileManager.DeleteFile(item.Path);
            }
            else
            {
                FileManager.DeleteFolder(item.Path);
            }
            return TransferResult.Ok;
        }

        public virtual TransferResult CreateFolder(string path)
        {
            if (FileManager.FolderExists(path)) return TransferResult.Skipped;
            FileManager.CreateFolder(path);
            return TransferResult.Ok;
        }

        private bool IsDriveAccessible(FileSystemItemViewModel drive)
        {
            try
            {
                //FtpTrace.WriteLine("[IsDriveAccessible: DriveIsReady] START");
                var result = FileManager.DriveIsReady(drive.Path);
                //FtpTrace.WriteLine("[IsDriveAccessible: DriveIsReady] END");
                return result;
            }
            catch (Exception ex)
            {
                AsyncErrorCallback(ex);
                return false;
            }
        }

        protected virtual void ChangeDrive()
        {
            CurrentRow = null;
            if (PathCache.ContainsKey(Drive))
            {
                var path = PathCache[Drive];
                var clearPath = new Regex(@"^(.*)[\\/].*(:[\\/]).*$");
                path = clearPath.Replace(path, "$1");
                //FtpTrace.WriteLine("[PathCache hit]");
                var model = FileManager.GetItemInfo(path);
                if (model != null)
                {
                    TitleRecognizer.RecognizeType(model);
                    if (path == Drive.Path) model.Type = ItemType.Drive;
                    CurrentFolder = new FileSystemItemViewModel(model);
                }
                else
                {
                    CurrentFolder = Drive;
                }
            }
            else
            {
                CurrentFolder = Drive;
            }
            //FtpTrace.WriteLine("[ChangeDrive] " + CurrentFolder.Path);
            ChangeDirectoryCommand.Execute(null);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            OpenStfsPackageCommand.RaiseCanExecuteChanged();
            CalculateSizeCommand.RaiseCanExecuteChanged();
            RefreshTitleCommand.RaiseCanExecuteChanged();
            RecognizeFromProfileCommand.RaiseCanExecuteChanged();
            CopyTitleIdToClipboardCommand.RaiseCanExecuteChanged();
            SearchGoogleCommand.RaiseCanExecuteChanged();
            SaveThumbnailCommand.RaiseCanExecuteChanged();
            FileOperationCommand.RaiseCanExecuteChanged();
        }

        public void GetItemViewModel(string itemPath)
        {
            var listedItem = Items.FirstOrDefault(item => item.Path == itemPath);
            if (listedItem != null)
            {
                PublishItemViewModel(listedItem);
                return;
            }

            WorkHandler.Run(() =>
            {
                var item = FileManager.GetItemInfo(itemPath, ItemType.File);
                if (item != null) TitleRecognizer.RecognizeType(item);
                return item;
            }, 
            item =>
                {
                    if (item == null) throw new ApplicationException(string.Format(Resx.ItemNotExistsOnPath, itemPath));
                    var vm = new FileSystemItemViewModel(item);
                    RecognitionAsync(item, i => PublishItemViewModel(vm), null);
                });
        }

        public void Recognize(FileSystemItemViewModel item)
        {
            try
            {
                TitleRecognizer.RecognizeTitle(item.Model);
                item.NotifyModelChanges();
            }
            catch {}
        }

        private void RecognitionStart()
        {
            lock (_queueLock)
            {
                if (_queue.Count > 0)
                {
                    var item = _queue.Peek();
                    ProgressMessage = string.Format(Resx.RecognizingItem + Strings.DotDotDot, item.Name, _queue.Count - 1);
                    RecognitionAsync(item, RecognitionSuccess, RecognitionError);
                }
                else
                {
                    RecognitionFinish();
                }
            }
        }

        private void RecognitionAsync(FileSystemItem item, Action<FileSystemItem> success, Action<Exception> error)
        {
            WorkHandler.Run(() => RecognitionInner(item), success, error);
        }

        protected virtual FileSystemItem RecognitionInner(FileSystemItem item)
        {
            TitleRecognizer.RecognizeTitle(item);
            return item;
        }

        private void RecognitionSuccess(FileSystemItem item)
        {
            var vm = Items.FirstOrDefault(i => i.Model == item);
            //if vm not exists it means a directory change has been occured so this recognition loop is unnecessary anymore 
            //(the queue has been emptied and a new loop has been started on a different thread)
            if (vm == null) return;
            
            vm.NotifyModelChanges();
            lock (_queueLock)
            {
                _queue.Dequeue();
            }
            RecognitionStart();
        }

        private void RecognitionError(Exception exception)
        {
            lock (_queueLock)
            {
                _queue.Dequeue();
            }
            RecognitionStart();
        }

        protected virtual void RecognitionFinish()
        {
            lock (_queueLock)
            {
                _queue = null;
            }
            SortContent();
            IsBusy = false;
        }

        private void PublishItemViewModel(ViewModelBase vm)
        {
            EventAggregator.GetEvent<ViewModelGeneratedEvent>().Publish(new ViewModelGeneratedEventArgs(vm));
        }

        public virtual Queue<QueueItem> PopulateQueue(FileOperation action, IEnumerable<FileSystemItem> selection)
        {
            var notify = false;
            if (selection == null)
            {
                if (!SelectedItems.Any()) CurrentRow.IsSelected = true;
                selection = SelectedItems.Select(vm => vm.Model);
                notify = true;
            }
            var direction = action == FileOperation.Delete ? TreeTraversalDirection.Upward : TreeTraversalDirection.Downward;
            var res = PopulateQueue(selection, direction, action, true);
            if (notify) SelectedItems.ForEach(item => item.NotifyModelChanges());
            var queue = new Queue<QueueItem>();
            res.ForEach(queue.Enqueue);
            return queue;
        }

        private List<QueueItem> PopulateQueue(IEnumerable<FileSystemItem> items, TreeTraversalDirection direction, FileOperation action, bool topLevel)
        {
            var result = new List<QueueItem>();
            foreach (var item in items)
            {
                if (direction == TreeTraversalDirection.Downward) result.Add(new QueueItem(item, action));
                List<QueueItem> sub = null;
                if (item.Type == ItemType.Directory && ValidateDirectory(item, action)) //TODO: Link?
                {
                    sub = PopulateQueue(GetList(item.Path), direction, action, false);
                    item.Size = sub.Where(i => i.FileSystemItem.Type == ItemType.File).Sum(i => i.FileSystemItem.Size ?? 0);
                    result.AddRange(sub);
                }
                if (direction != TreeTraversalDirection.Upward) continue;
                if (topLevel && action == FileOperation.Delete && sub != null && sub.Any())
                {
                    var s = sub[0];
                    s.Confirmation = true;
                    s.Payload = item;
                }
                result.Add(new QueueItem(item, action));
            }
            return result;
        }

        protected virtual bool ValidateDirectory(FileSystemItem item, FileOperation action)
        {
            return true;
        }

        protected virtual void AsyncErrorCallback(Exception ex)
        {
            _calculationIsRunning = false;
            IsBusy = false;
            EventAggregator.GetEvent<ShowCorrespondingErrorEvent>().Publish(new ShowCorrespondingErrorEventArgs(ex, false));
        }

        public virtual Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            return FileManager.GetStream(path, mode, access, startPosition);
        }

        public virtual bool CopyStream(FileSystemItem item, Stream stream, long startPosition = 0, long? byteLimit = null)
        {
            return FileManager.CopyStream(item, stream, startPosition, byteLimit);
        }

        private void OnTransferProgressChanged(TransferProgressChangedEventArgs args)
        {
            UIThread.Run(() =>
                {
                    if (string.IsNullOrEmpty(ProgressMessage) || ProgressMessage.StartsWith(Resx.ChangingDirectory)) return;
                    var r = new Regex(@" \([0-9]+%\)$");
                    ProgressMessage = r.Replace(ProgressMessage, string.Empty);
                    ProgressMessage += string.Format(" ({0}%)", args.Percentage);
                });
        }

        public void Rename(ColumnMode column)
        {
            string oldValue;
            string header;
            switch (column)
            {
                case ColumnMode.Title:
                    header = Resx.NewTitle;
                    oldValue = CurrentRow.Title;
                    break;
                case ColumnMode.Name:
                    header = Resx.NewName;
                    oldValue = CurrentRow.Name;
                    break;
                default:
                    throw new NotSupportedException("Not supported column: " + column);
            }            
            var newValue = WindowManager.ShowTextInputDialog(Resx.Rename, header + Strings.Colon, oldValue, null);
            if (!string.IsNullOrWhiteSpace(newValue)) Rename(column, newValue);
        }

        public void Rename(ColumnMode column, string newValue)
        {
            switch (column)
            {
                case ColumnMode.Title:
                    RenameItemTitle(CurrentRow, newValue);
                    break;
                case ColumnMode.Name:
                    CurrentRow = RenameItemName(CurrentRow, newValue);
                    break;
                default:
                    throw new NotSupportedException("Something went wrong, property change not supported: " + column);
            }
            SortContent();
        }

        protected virtual void RenameItemTitle(FileSystemItemViewModel item, string title)
        {
            if (item.Title == title) return;
            if (string.IsNullOrEmpty(title))
            {
                item.Title = item.Title;
            }
            else
            {
                item.Title = title;
                TitleRecognizer.UpdateTitle(item.Model);
            }
        }

        private FileSystemItemViewModel RenameItemName(FileSystemItemViewModel item, string name)
        {
            var refresh = false;
            FileSystemItem newModel = null;
            do
            {
                if (string.IsNullOrEmpty(name) || item.Name == name)
                {
                    item.Name = item.Name;
                    newModel = item.Model;
                }
                else
                {
                    try
                    {
                        newModel = RenameItemName(item.Model, name);
                    }
                    catch (TransferException ex)
                    {
                        var vm = WindowManager.GetWriteErrorDialogViewModel(ex);
                        vm.CompactMode = true;
                        if (!string.IsNullOrEmpty(vm.SourceFilePath)) GetItemViewModel(vm.SourceFilePath);
                        if (!string.IsNullOrEmpty(vm.TargetFilePath)) GetItemViewModel(vm.TargetFilePath);
                        var result = WindowManager.ShowWriteErrorDialog(vm);
                        switch (result.Behavior)
                        {
                            case ErrorResolutionBehavior.Retry:
                                FileManager.DeleteFile(vm.TargetFilePath);
                                refresh = true;
                                break;
                            case ErrorResolutionBehavior.Rename:
                                name = WindowManager.ShowTextInputDialog(Resx.Rename, Resx.NewName + Strings.Colon, name, null);
                                break;
                            case ErrorResolutionBehavior.Cancel:
                                item.Name = item.Name;
                                return item;
                        }
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowMessage(Resx.IOError, ex.Message);
                        item.Name = item.Name;
                        return item;
                    }
                }
            }
            while (newModel == null);

            if (refresh)
            {
                Refresh(false, () =>
                {
                    var renamedItem = Items.SingleOrDefault(i => i.Name == name);
                    if (renamedItem != null) CurrentRow = renamedItem;
                });
                return null;
            }

            if (newModel != item.Model)
            {
                TitleRecognizer.RecognizeType(newModel);
                TitleRecognizer.RecognizeTitle(newModel);
                var newItem = new FileSystemItemViewModel(newModel);
                Items.Replace(item, newItem);
                EventAggregator.GetEvent<FileListPaneViewModelItemsChangedEvent>().Publish(new FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, item, this));
                return newItem;
            }

            item.NotifyModelChanges();
            return item;
        }

        protected virtual FileSystemItem RenameItemName(FileSystemItem item, string name)
        {
            var targetPath = item.Path.Substring(0, item.Path.LastIndexOf(FileManager.Slash) + 1) + name;
            var exists = FileManager.FileExists(targetPath);
            if (exists)
                throw new TransferException(TransferErrorType.WriteAccessError, Resx.TargetAlreadyExists)
                {
                    SourceFile = item.Path,
                    TargetFile = targetPath,
                    TargetFileSize = exists.Size
                };
            return FileManager.Rename(item.Path, name);
        }
    }
}