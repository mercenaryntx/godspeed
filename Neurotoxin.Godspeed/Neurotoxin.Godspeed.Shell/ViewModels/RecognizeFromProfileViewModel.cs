using System;
using System.Diagnostics;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Events;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Neurotoxin.Godspeed.Shell.Extensions;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class RecognizeFromProfileViewModel : ViewModelBase, IProgressViewModel
    {
        private int _titleUpdated;
        private int _itemsCount;
        private int _itemsChecked;
        private readonly ITitleRecognizer _titleRecognizer;

        public string ProgressDialogTitle
        {
            get { return Resx.RecognizeTitlesFromProfile; }
        }

        private const string PROGRESSMESSAGE = "ProgressMessage";
        private string _progressMessage;
        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { _progressMessage = value; NotifyPropertyChanged(PROGRESSMESSAGE); }
        }

        private const string PROGRESSVALUE = "ProgressValue";
        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value; 
                NotifyPropertyChanged(PROGRESSVALUE); 
                NotifyPropertyChanged(PROGRESSVALUEDOUBLE);
            }
        }

        private const string PROGRESSVALUEDOUBLE = "ProgressValueDouble";
        public double ProgressValueDouble
        {
            get { return (double)ProgressValue / 100; }
        }

        private const string ISINDETERMINE = "IsIndetermine";
        private bool _isIndetermine;
        public bool IsIndetermine
        {
            get { return _isIndetermine; }
            set { _isIndetermine = value; NotifyPropertyChanged(ISINDETERMINE); }
        }

        public RecognizeFromProfileViewModel(ITitleRecognizer titleRecognizer)
        {
            _titleRecognizer = titleRecognizer;
        }

        public void Recognize(FileSystemItem item, Action<int> success, Action<Exception> error)
        {
            ProgressMessage = Resx.OpeningProfile;
            this.NotifyProgressStarted();
            WorkHandler.Run(() => RecognizeFromProfile(item), 
                result =>
                    {
                        this.NotifyProgressFinished();
                        if (success != null) success.Invoke(result);
                    }, 
                exception =>
                    {
                        this.NotifyProgressFinished();
                        if (error != null) error.Invoke(exception);
                    });
        }

        private int RecognizeFromProfile(FileSystemItem item)
        {
            _titleUpdated = -1;
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnGetBinaryContentProgressChanged);
            var content = _titleRecognizer.GetBinaryContent(item);
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Unsubscribe(OnGetBinaryContentProgressChanged);
            if (content != null)
            {
                var stfs = ModelFactory.GetModel<StfsPackage>(content);
                UIThread.Run(() =>
                {
                    ProgressMessage = Resx.ScanningProfile;
                    ProgressValue = 0;
                });
                stfs.ContentCountDetermined += OnStfsContentCountDetermined;
                stfs.ContentParsed += OnStfsContentParsed;
                stfs.ExtractGames();
                stfs.ContentCountDetermined -= OnStfsContentCountDetermined;
                stfs.ContentParsed -= OnStfsContentParsed;
            }
            return _titleUpdated;
        }

        private void OnGetBinaryContentProgressChanged(TransferProgressChangedEventArgs e)
        {
            UIThread.Run(() => ProgressValue = e.Percentage);
        }

        private void OnStfsContentCountDetermined(object sender, ContentCountEventArgs e)
        {
            _itemsCount = e.Count;
            UIThread.Run(() =>
            {
                IsIndetermine = false;
                ProgressValue = 0;
            });
        }

        private void OnStfsContentParsed(object sender, ContentParsedEventArgs e)
        {
            //TODO: determine whether update is necessary or not
            _titleUpdated++;
            var game = (GameFile)e.Content;
            _titleRecognizer.UpdateTitle(new FileSystemItem
            {
                Name = game.TitleId,
                Title = game.Title,
                Type = ItemType.Directory,
                TitleType = TitleType.Game,
                Thumbnail = game.Thumbnail,
                RecognitionState = RecognitionState.Recognized
            });
            _itemsChecked++;
            UIThread.Run(() => ProgressValue = _itemsCount == 0 ? 0 : _itemsChecked * 100 / _itemsCount);
        }
    }
}