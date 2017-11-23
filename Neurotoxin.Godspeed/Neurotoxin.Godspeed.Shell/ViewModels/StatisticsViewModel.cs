using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class StatisticsViewModel : ViewModelBase, IStatisticsViewModel
    {
        private readonly ICacheManager _cacheManager;
        private readonly IDbContext _dbContext;
        private readonly Statistics _statistics;

        public DateTime UsageStart { get; private set; }
        public Dictionary<string, int> CommandUsage { get; }
        public Dictionary<FtpServerType, int> ServerUsage { get; }

        private const string GAMESRECOGNIZEDFULLY = "GamesRecognizedFully";
        public int GamesRecognizedFully
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == (int)TitleType.Game && c.RecognitionState == (int)RecognitionState.Recognized); }
        }

        private const string GAMESRECOGNIZEDPARTIALLY = "GamesRecognizedPartially";
        public int GamesRecognizedPartially
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == (int)TitleType.Game && c.RecognitionState == (int)RecognitionState.PartiallyRecognized); }
        }

        private const string SVODPACKAGESRECOGNIZED = "SvodPackagesRecognized";
        public int SvodPackagesRecognized
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == (int)TitleType.Unknown && c.ContentType != (int)ContentType.Unknown); }
        }

        private const string STFSPACKAGESRECOGNIZED = "StfsPackagesRecognized";
        public int StfsPackagesRecognized
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == (int)TitleType.Profile); }
        }

        private const string FILESTRANSFERRED = "FilesTransferred";
        private int _filesTransferred;
        public int FilesTransferred
        {
            get { return _filesTransferred; }
            set { _filesTransferred = value; NotifyPropertyChanged(FILESTRANSFERRED); }
        }

        private const string TOTALFILESTRANSFERRED = "TotalFilesTransferred";
        public int TotalFilesTransferred
        {
            get { return _statistics.TotalFilesTransferred + FilesTransferred; }
            set { _statistics.TotalFilesTransferred = value; NotifyPropertyChanged(TOTALFILESTRANSFERRED); }
        }

        private const string BYTESTRANSFERRED = "BytesTransferred";
        private long _bytesTransferred;
        public long BytesTransferred
        {
            get { return _bytesTransferred; }
            set { _bytesTransferred = value; NotifyPropertyChanged(BYTESTRANSFERRED); }
        }

        private const string TOTALBYTESTRANSFERRED = "TotalBytesTransferred";
        public long TotalBytesTransferred
        {
            get { return _statistics.TotalBytesTransferred + BytesTransferred; }
            set { _statistics.TotalBytesTransferred = value; NotifyPropertyChanged(TOTALBYTESTRANSFERRED); }
        }

        private const string TIMESPENTWITHTRANSFER = "TimeSpentWithTransfer";
        private TimeSpan _timeSpentWithTransfer;
        public TimeSpan TimeSpentWithTransfer
        {
            get { return _timeSpentWithTransfer; }
            set { _timeSpentWithTransfer = value; NotifyPropertyChanged(TIMESPENTWITHTRANSFER); }
        }

        private const string TOTALTIMESPENTWITHTRANSFER = "TotalTimeSpentWithTransfer";
        public TimeSpan TotalTimeSpentWithTransfer
        {
            get { return _statistics.TotalTimeSpentWithTransfer + TimeSpentWithTransfer; }
            set { _statistics.TotalTimeSpentWithTransfer = value; NotifyPropertyChanged(TOTALTIMESPENTWITHTRANSFER); }
        }

        private const string USAGETIME = "UsageTime";
        public TimeSpan UsageTime
        {
            get { return DateTime.Now.Subtract(UsageStart); }
        }

        private const string TOTALUSAGETIME = "TotalUsageTime";
        public TimeSpan TotalUsageTime
        {
            get { return _statistics.TotalUsageTime + UsageTime; }
            set { _statistics.TotalUsageTime = value; NotifyPropertyChanged(TOTALUSAGETIME); }
        }

        private const string APPLICATIONSTARTED = "ApplicationStarted";
        public int ApplicationStarted
        {
            get { return _statistics.ApplicationStarted; }
            set { _statistics.ApplicationStarted = value; NotifyPropertyChanged(APPLICATIONSTARTED); }
        }

        private const string APPLICATIONCRASHED = "ApplicationCrashed";
        public int ApplicationCrashed
        {
            get { return _statistics.ApplicationCrashed; }
            set { _statistics.ApplicationCrashed = value; NotifyPropertyChanged(APPLICATIONCRASHED); }
        }

        private const string SUCCESSFULUPDATE = "SuccessfulUpdate";
        public int SuccessfulUpdate
        {
            get { return _statistics.SuccessfulUpdate; }
            set { _statistics.SuccessfulUpdate = value; NotifyPropertyChanged(SUCCESSFULUPDATE); }
        }

        private const string UNSUCCESSFULUPDATE = "UnsuccessfulUpdate";
        public int UnsuccessfulUpdate
        {
            get { return _statistics.UnsuccessfulUpdate; }
            set { _statistics.UnsuccessfulUpdate = value; NotifyPropertyChanged(UNSUCCESSFULUPDATE); }
        }

        public StatisticsViewModel(ICacheManager cacheManager, IDbContext dbContext)
        {
            UsageStart = DateTime.Now;
            CommandUsage = new Dictionary<string, int>();
            ServerUsage = new Dictionary<FtpServerType, int>();

            _cacheManager = cacheManager;
            _dbContext = dbContext;
            using(var db = _dbContext.Open())
            {
                _statistics = db.Get<Statistics>().First();
            }
            DelegateCommand.BeforeAction = CountCommandUsage;
            EventAggregator.GetEvent<OpenNestedPaneEvent>().Subscribe(OnPaneOpen);
        }

        private void OnPaneOpen(OpenNestedPaneEventArgs e)
        {
            var ftp = e.Openee as FtpContentViewModel;
            if (ftp == null) return;
            if (ServerUsage.ContainsKey(ftp.ServerType))
            {
                ServerUsage[ftp.ServerType]++;
            }
            else
            {
                ServerUsage.Add(ftp.ServerType, 1);
            }
        }

        public void PersistData()
        {
            using (var db = _dbContext.Open())
            {
                _statistics.TotalBytesTransferred = TotalBytesTransferred;
                _statistics.TotalFilesTransferred = TotalFilesTransferred;
                _statistics.TotalTimeSpentWithTransfer = TotalTimeSpentWithTransfer;
                _statistics.TotalUsageTime = TotalUsageTime;
                db.Persist(_statistics);
            }
        }

        private void CountCommandUsage(MethodInfo commandAction)
        {
            var declaringType = commandAction.DeclaringType;
            var className = string.Empty;
            if (declaringType != null)
            {
                className = declaringType.Name;
                var genericArgs = declaringType.GetGenericArguments();
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    var genericType = genericArgs[i];
                    var prefix = i == 0 ? "<" : ",";
                    var suffix = i == genericArgs.Length - 1 ? ">" : string.Empty;
                    className = className.Replace(string.Format("`{0}", i + 1), string.Format("{0}{1}{2}", prefix, genericType.Name, suffix));
                }
                className += ".";
            }
            var command = className + commandAction.Name;
            if (CommandUsage.ContainsKey(command))
                CommandUsage[command]++;
            else
                CommandUsage.Add(command, 1);
        }
    }
}