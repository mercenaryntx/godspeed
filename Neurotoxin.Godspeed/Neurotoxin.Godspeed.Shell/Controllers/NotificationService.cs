using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.Controllers
{
    public class NotificationService
    {
        private const string PARTICIPATIONMESSAGEKEY = "ParticipationMessage";
        //private const string FACEBOOKMESSAGEKEY = "FacebookMessage";
        //private const string CODEPLEXMESSAGEKEY = "CodeplexMessage";
        private const string NEWVERSIONAVAILABLEMESSAGEKEY = "NewVersionAvailableMessage";

        private Timer _participationTimer;
        //private Timer _facebookTimer;
        //private Timer _codeplexTimer;

        private readonly Queue<NotifyUserMessageEventArgs> _messageQueue = new Queue<NotifyUserMessageEventArgs>();

        private readonly IEventAggregator _eventAggregator;
        private readonly IStatisticsViewModel _statistics;
        private readonly IUserSettingsProvider _userSettings;
        private readonly IWorkHandler _workHandler;

        public NotificationService(IEventAggregator eventAggregator, IStatisticsViewModel statistics, IUserSettingsProvider userSettings, IWorkHandler workHandler)
        {
            _eventAggregator = eventAggregator;
            _statistics = statistics;
            _userSettings = userSettings;
            _workHandler = workHandler;

            eventAggregator.GetEvent<ShellInitializedEvent>().Subscribe(OnShellInitialized);
        }

        private void OnShellInitialized(ShellInitializedEventArgs obj)
        {
            foreach(var message in _messageQueue)
            {
                _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(message);
            }
            if (_userSettings.UseVersionChecker)
            {
                CheckForNewerVersion();
            }
            if (!_userSettings.DisableUserStatisticsParticipation.HasValue)
            {
                _participationTimer = new Timer(ParticipationMessage, null, 60000, -1);
            }
            //if (!_userSettings.IsMessageIgnored(FACEBOOKMESSAGEKEY))
            //{
            //    _facebookTimer = new Timer(FacebookMessage, null, 600000, -1);
            //}
            //if (!_userSettings.IsMessageIgnored(CODEPLEXMESSAGEKEY) && _statistics.ApplicationStarted > 9 && _statistics.TotalUsageTime > new TimeSpan(0, 2, 0, 0))
            //{
            //    _codeplexTimer = new Timer(CodeplexMessage, null, 60000, -1);
            //}
        }

        private void CheckForNewerVersion()
        {
            var feedReader = new NauXmlFeedReaderEx();
            var updManager = UpdateManager.Instance;
            updManager.UpdateSource = new SimpleWebSource(ConfigurationManager.AppSettings["UpdateSourceUrl"]);
            updManager.UpdateFeedReader = feedReader;
            updManager.ReinstateIfRestarted();
            updManager.Config.UpdateProcessName = "GODspeed";
            updManager.Config.UpdateExecutableName = "godspeed.exe";
            if (updManager.State == UpdateManager.UpdateProcessState.NotChecked)
            {
                _workHandler.Run(() =>
                {
                    updManager.CheckForUpdates();
                    return string.Compare(feedReader.LatestVersionOnServer, App.ApplicationVersion, StringComparison.InvariantCultureIgnoreCase) == 1 && updManager.UpdatesAvailable > 0;
                }, hasUpdate =>
                {
                    if (!hasUpdate) return;
                    var args = new NotifyUserMessageEventArgs(NEWVERSIONAVAILABLEMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenDialog, typeof(AutoUpdateDialog), MessageFlags.Ignorable, feedReader.LatestVersionOnServer);
                    _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
                });
            }
        }

        private void ParticipationMessage(object state)
        {
            _participationTimer.Dispose();
            var args = new NotifyUserMessageEventArgs(PARTICIPATIONMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenDialog, typeof(UserStatisticsParticipationDialog));
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }

        //private void FacebookMessage(object state)
        //{
        //    _facebookTimer.Dispose();
        //    var args = new NotifyUserMessageEventArgs(FACEBOOKMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenUrl, "http://www.facebook.com/godspeedftp", MessageFlags.Ignorable | MessageFlags.IgnoreAfterOpen);
        //    _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        //}

        //private void CodeplexMessage(object state)
        //{
        //    _codeplexTimer.Dispose();
        //    var args = new NotifyUserMessageEventArgs(CODEPLEXMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenUrl, "http://godspeed.codeplex.com", MessageFlags.Ignorable | MessageFlags.IgnoreAfterOpen);
        //    _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        //}

        public void QueueMessage(NotifyUserMessageEventArgs message)
        {
            _messageQueue.Enqueue(message);
        }

        public void QueueMessages(IEnumerable<NotifyUserMessageEventArgs> messages)
        {
            foreach (var message in messages)
            {
                QueueMessage(message);
            }
        }
    }
}