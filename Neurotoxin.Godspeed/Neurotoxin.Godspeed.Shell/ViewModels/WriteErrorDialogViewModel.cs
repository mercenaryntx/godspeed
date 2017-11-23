using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class WriteErrorDialogViewModel : DialogViewModelBase<TransferErrorDialogResult>
    {
        private static readonly TransferErrorDialogResult Overwrite = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.Current, CopyAction.Overwrite);
        private static readonly TransferErrorDialogResult OverwriteAll = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.All, CopyAction.Overwrite);
        private static readonly TransferErrorDialogResult OverwriteAllSmaller = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.All, CopyAction.OverwriteSmaller);
        private static readonly TransferErrorDialogResult Resume = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.Current, CopyAction.Resume);
        private static readonly TransferErrorDialogResult ResumeAll = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.All, CopyAction.Resume);
        private static readonly TransferErrorDialogResult Rename = new TransferErrorDialogResult(ErrorResolutionBehavior.Rename);
        private static readonly TransferErrorDialogResult Skip = new TransferErrorDialogResult(ErrorResolutionBehavior.Skip);
        private static readonly TransferErrorDialogResult SkipAll = new TransferErrorDialogResult(ErrorResolutionBehavior.Skip, CopyActionScope.All);
        private static readonly TransferErrorDialogResult Cancel = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);

        public TransferErrorDialogResult OverwriteOption { get { return Overwrite; } }
        public TransferErrorDialogResult OverwriteAllOption { get { return OverwriteAll; } }
        public TransferErrorDialogResult OverwriteAllSmallerOption { get { return OverwriteAllSmaller; } }
        public TransferErrorDialogResult ResumeOption { get { return Resume; } }
        public TransferErrorDialogResult ResumeAllOption { get { return ResumeAll; } }
        public TransferErrorDialogResult RenameOption { get { return Rename; } }
        public TransferErrorDialogResult SkipOption { get { return Skip; } }
        public TransferErrorDialogResult SkipAllOption { get { return SkipAll; } }
        public TransferErrorDialogResult CancelOption { get { return Cancel; } }

        public bool CompactMode { get; set; }

        private const string ISOVERWRITEENABLED = "IsOverwriteEnabled";
        private bool _isOverwriteEnabled;
        public bool IsOverwriteEnabled
        {
            get { return _isOverwriteEnabled; }
            set { _isOverwriteEnabled = value; NotifyPropertyChanged(ISOVERWRITEENABLED); }
        }

        private const string ISOVERWRITEALLENABLED = "IsOverwriteAllEnabled";
        private bool _isOverwriteAllEnabled;
        public bool IsOverwriteAllEnabled
        {
            get { return _isOverwriteAllEnabled; }
            set { _isOverwriteAllEnabled = value; NotifyPropertyChanged(ISOVERWRITEALLENABLED); }
        }

        private const string ISOVERWRITEALLSMALLERENABLED = "IsOverwriteAllSmallerEnabled";
        private bool _isOverwriteAllSmallerEnabled;
        public bool IsOverwriteAllSmallerEnabled
        {
            get { return _isOverwriteAllSmallerEnabled; }
            set { _isOverwriteAllSmallerEnabled = value; NotifyPropertyChanged(ISOVERWRITEALLSMALLERENABLED); }
        }

        private const string ISRESUMEENABLED = "IsResumeEnabled";
        private bool _isResumeEnabled;
        public bool IsResumeEnabled
        {
            get { return _isResumeEnabled; }
            set { _isResumeEnabled = value; NotifyPropertyChanged(ISRESUMEENABLED); }
        }

        private const string ISRESUMEALLENABLED = "IsResumeAllEnabled";
        private bool _isResumeAllEnabled;
        public bool IsResumeAllEnabled
        {
            get { return _isResumeAllEnabled; }
            set { _isResumeAllEnabled = value; NotifyPropertyChanged(ISRESUMEALLENABLED); }
        }

        private const string ISRENAMEENABLED = "IsRenameEnabled";
        private bool _isRenameEnabled;
        public bool IsRenameEnabled
        {
            get { return _isRenameEnabled; }
            set { _isRenameEnabled = value; NotifyPropertyChanged(ISRENAMEENABLED); }
        }

        private const string ISSKIPENABLED = "IsSkipEnabled";
        private bool _isSkipEnabled;
        public bool IsSkipEnabled
        {
            get { return _isSkipEnabled; }
            set { _isSkipEnabled = value; NotifyPropertyChanged(ISSKIPENABLED); }
        }

        private const string ISSKIPALLENABLED = "IsSkipAllEnabled";
        private bool _isSkipAllEnabled;
        public bool IsSkipAllEnabled
        {
            get { return _isSkipAllEnabled; }
            set { _isSkipAllEnabled = value; NotifyPropertyChanged(ISSKIPALLENABLED); }
        }

        private const string ISCANCELENABLED = "IsCancelEnabled";
        private bool _isCancelEnabled;
        public bool IsCancelEnabled
        {
            get { return _isCancelEnabled; }
            set { _isCancelEnabled = value; NotifyPropertyChanged(ISCANCELENABLED); }
        }

        private const string SOURCEFILEPATH = "SourceFilePath";
        private string _sourceFilePath;
        public string SourceFilePath
        {
            get { return _sourceFilePath; }
            set { _sourceFilePath = value; NotifyPropertyChanged(SOURCEFILEPATH); }
        }

        private const string SOURCEFILE = "SourceFile";
        private FileSystemItemViewModel _sourceFile;
        public FileSystemItemViewModel SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; NotifyPropertyChanged(SOURCEFILE); }
        }

        private const string SOURCEFILEHEADER = "SourceFileHeader";
        private string _sourceFileHeader;
        public string SourceFileHeader
        {
            get { return _sourceFileHeader; }
            set { _sourceFileHeader = value; NotifyPropertyChanged(SOURCEFILEHEADER); }
        }

        private const string TARGETFILEPATH = "TargetFilePath";
        private string _targetFilePath;
        public string TargetFilePath
        {
            get { return _targetFilePath; }
            set { _targetFilePath = value; NotifyPropertyChanged(TARGETFILEPATH); }
        }

        private const string TARGETFILE = "TargetFile";
        private FileSystemItemViewModel _targetFile;
        public FileSystemItemViewModel TargetFile
        {
            get { return _targetFile; }
            set { _targetFile = value; NotifyPropertyChanged(TARGETFILE); }
        }

        private const string TARGETFILEHEADER = "TargetFileHeader";
        private string _targetFileHeader;
        public string TargetFileHeader
        {
            get { return _targetFileHeader; }
            set { _targetFileHeader = value; NotifyPropertyChanged(TARGETFILEHEADER); }
        }

        private const string EXCEPTIONTYPE = "ExceptionType";
        private TransferErrorType _exceptionType;
        public TransferErrorType ExceptionType
        {
            get { return _exceptionType; }
            set
            {
                _exceptionType = value; 
                NotifyPropertyChanged(EXCEPTIONTYPE);
                switch (value)
                {
                    case TransferErrorType.WriteAccessError:
                        Title = Resx.TargetAlreadyExists;
                        SourceFileHeader = Resx.SourceFileHeader + Strings.Colon;
                        TargetFileHeader = Resx.TargetFileHeader + Strings.Colon;
                        //TODO: Message
                        break;
                    case TransferErrorType.NotSupporterCharactersInPath:
                        Title = Resx.InvalidFileName;
                        Message = Resx.SpecialCharactersNotSupported;
                        SourceFileHeader = Resx.SourceFile + Strings.Colon;
                        break;
                    case TransferErrorType.NameIsTooLong:
                        Title = Resx.InvalidFileName;
                        Message = Resx.NameIsTooLong;
                        SourceFileHeader = Resx.SourceFile + Strings.Colon;
                        break;
                    case TransferErrorType.PathIsTooLong:
                        Title = Resx.InvalidTargetPath;
                        Message = Resx.PathIsTooLong;
                        SourceFileHeader = Resx.SourceFile + Strings.Colon;
                        break;
                }
            }
        }

        protected override TransferErrorDialogResult DefaultResult
        {
            get { return Cancel; }
        }

        public WriteErrorDialogViewModel()
        {
            EventAggregator.GetEvent<ViewModelGeneratedEvent>().Subscribe(ViewModelGenerated);
        }

        private void ViewModelGenerated(ViewModelGeneratedEventArgs args)
        {
            var vm = args.ViewModel as FileSystemItemViewModel;
            if (vm == null) return;
            if (vm.Path == SourceFilePath) SourceFile = vm;
            if (vm.Path == TargetFilePath) TargetFile = vm;
        }

        public override void Close()
        {
            EventAggregator.GetEvent<ViewModelGeneratedEvent>().Unsubscribe(ViewModelGenerated);
            base.Close();
        }

    }
}