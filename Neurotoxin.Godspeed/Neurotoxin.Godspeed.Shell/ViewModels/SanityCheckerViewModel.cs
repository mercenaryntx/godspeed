using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Properties;
using ServiceStack.OrmLite;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.Extensions;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class SanityCheckerViewModel : ViewModelBase, IProgressViewModel
    {
        //TODO: move this somewhere else
        public static bool? DataGridSupportsRenaming { get; set; }

        private const string NEWERDOTNETVERSIONREQUIREDMESSAGEKEY = "NewerDotNetVersionRequiredMessage";

        private readonly IDbContext _dbContext;
        private readonly IResourceManager _resourceManager;
        private readonly IWindowManager _windowManager;
        private string _esentDir;

        #region Properies

        public string ProgressDialogTitle
        {
            get { return Resources.MigrationProgressDialogTitle; }
        }

        public string ProgressMessage
        {
            get { return Resources.MigrationProgressMessage; }
        }

        private const string ITEMSCOUNT = "ItemsCount";
        private int _itemsCount;
        public int ItemsCount
        {
            get { return _itemsCount; }
            set { _itemsCount = value; NotifyPropertyChanged(ITEMSCOUNT); }
        }

        private const string ITEMSMIGRATED = "ItemsMigrated";
        private int _itemsMigrated;
        public int ItemsMigrated
        {
            get { return _itemsMigrated; }
            set 
            { 
                _itemsMigrated = value;
                NotifyPropertyChanged(ITEMSMIGRATED);
                NotifyPropertyChanged(PROGRESSVALUE);
                NotifyPropertyChanged(PROGRESSVALUEDOUBLE);
            }
        }

        private const string PROGRESSVALUE = "ProgressValue";
        public int ProgressValue
        {
            get { return ItemsCount == 0 ? 0 : ItemsMigrated * 100 / ItemsCount; }
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

        #endregion

        public SanityCheckerViewModel(IDbContext dbContext, IResourceManager resourceManager, IWindowManager windowManager)
        {
            _dbContext = dbContext;
            _resourceManager = resourceManager;
            _windowManager = windowManager;
        }

        public void CheckAsync(Action<SanityCheckResult> callback)
        {
            WorkHandler.Run(CheckDatabase, b => CheckDatabaseCallback(b, callback));
        }

        private bool CheckDatabase()
        {
            CheckDataDirectory();
            return DatabaseExists();
        }

        private void CheckDatabaseCallback(bool databaseExisted, Action<SanityCheckResult> callback)
        {
            var result = new SanityCheckResult();
            var frameworkMessage = CheckFrameworkVersion();
            if (frameworkMessage != null) result.UserMessages = new List<NotifyUserMessageEventArgs> { frameworkMessage };
            WorkHandler.Run(RetryFailedUserReports);

            if (!databaseExisted)
            {
                result.DatabaseCreated = true;
                WorkHandler.Run(MigrateEsentToDatabase, cacheStore => MigrateEsentToDatabaseCallback(cacheStore, callback, result));
            }
            else
            {
                callback.Invoke(result);
            }
        }

        private static NotifyUserMessageEventArgs CheckFrameworkVersion()
        {
            var applicationAssembly = Assembly.GetAssembly(typeof(Application));
            var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
            var actualVersion = new Version(fvi.ProductVersion);
            var requiredVersion = new Version(4, 0, 30319, 18408);

            var versionOk = actualVersion >= requiredVersion;
            DataGridSupportsRenaming = versionOk;
            return versionOk ? null : new NotifyUserMessageEventArgs(NEWERDOTNETVERSIONREQUIREDMESSAGEKEY, MessageIcon.Warning, MessageCommand.OpenUrl, "http://www.microsoft.com/en-us/download/details.aspx?id=40779");
        }

        private static void RetryFailedUserReports()
        {
            var queue = Directory.GetDirectories(App.PostDirectory);
            foreach (var item in queue.OrderBy(f => f))
            {
                var file = Directory.GetFiles(item).First();
                if (!HttpForm.Repost(file)) continue;
                Directory.Delete(item, true);
            }
        }

        private void CheckDataDirectory()
        {
            var asm = Assembly.GetExecutingAssembly();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var company = asm.GetAttribute<AssemblyCompanyAttribute>().Company;
            var product = asm.GetAttribute<AssemblyProductAttribute>().Product;
            var version = asm.GetAttribute<AssemblyFileVersionAttribute>().Version;

            App.DataDirectory = string.Format(@"{0}\{1}\{2}\{3}", appData, company, product, version);
            if (!Directory.Exists(App.DataDirectory))
            {
                var v11 = string.Format(@"{0}\{1}\{2}\1.1", appData, company, product);
                if (Directory.Exists(v11))
                {
                    Directory.Move(v11, App.DataDirectory);
                }
                else
                {
                    Directory.CreateDirectory(App.DataDirectory);
                }
            }

            App.PostDirectory = Path.Combine(App.DataDirectory, "post");
            if (!Directory.Exists(App.PostDirectory)) Directory.CreateDirectory(App.PostDirectory);

            //for backward compatibility
            _esentDir = string.Format(@"{0}\{1}\{2}\1.0", appData, company, product);
        }

        private bool DatabaseExists()
        {
            using(var db = _dbContext.Open())
            {
                db.CreateTableIfNotExists<CacheItem>();
                db.CreateTableIfNotExists<FtpConnection>();
                db.CreateTableIfNotExists<FavoriteFolder>();
                db.CreateTableIfNotExists<IgnoredMessage>();

                db.CreateTableIfNotExists<Statistics>();
                var statistics = db.Get<Statistics>().FirstOrDefault();
                if (statistics != null && statistics.Version < Statistics.LatestVersion)
                {
                    statistics.Version = Statistics.LatestVersion;
                    db.DropAndCreateTable<Statistics>();
                    db.Insert(statistics);
                }

                db.CreateTableIfNotExists<UserSettings>();
                var userSettings = db.Get<UserSettings>().FirstOrDefault();
                if (userSettings == null) return false;

                if (userSettings.Version < UserSettings.LatestVersion)
                {
                    userSettings.Version = UserSettings.LatestVersion;
                    db.DropAndCreateTable<UserSettings>();
                    db.Insert(userSettings);
                }
                return true;
            }
        }

        private EsentPersistentDictionary MigrateEsentToDatabase()
        {
            EsentPersistentDictionary cacheStore = null;
            var statistics = new Statistics();
            var userSettings = new UserSettings();
            var sw = new Stopwatch();
            sw.Start();
            using (var db = _dbContext.Open(true))
            {
                if (EsentExists())
                {
                    this.NotifyProgressStarted();
                    cacheStore = new EsentPersistentDictionary(_esentDir);
                    UpgradeEsentToLatestVersion(cacheStore);
                    var keys = cacheStore.Keys;
                    SetItemsCount(keys.Length);
                    foreach (var key in keys)
                    {
                        var prefix = key.SubstringBefore("_");
                        switch (prefix)
                        {
                            case "CacheEntry":
                                MigrateCacheItem(cacheStore, db, key);
                                break;
                            case "FtpConnection":
                                MigrateFtpConnection(cacheStore, db, key);
                                break;
                            case "Stat":
                                MigrateStatistics(cacheStore, key, statistics);
                                break;
                            case "WarningMessage":
                                MigrateIgnoredMessages(db, key);
                                break;
                            default:
                                MigrateUserSettings(cacheStore, key, userSettings);
                                break;
                        }
                        IncrementItemsMigrated();
                    }
                }
                db.Insert(statistics);
                db.Insert(userSettings);
            }
            sw.Stop();
            Debug.WriteLine("[MIGRATION] Database created in {0}", sw.Elapsed);

            if (Directory.Exists(_esentDir))
            {
                foreach (var postData in Directory.GetFiles(Path.Combine(_esentDir, "post")))
                {
                    File.Move(postData, Path.Combine(App.PostDirectory, Path.GetFileName(postData)));
                }
            }
            if (cacheStore != null) this.NotifyProgressFinished();
            return cacheStore;
        }

        private void MigrateEsentToDatabaseCallback(EsentPersistentDictionary cacheStore, Action<SanityCheckResult> callback, SanityCheckResult result)
        {
            if (cacheStore != null && _windowManager.Confirm(Resx.Delete, Resx.DeleteEsentConfirmationMessage)) DeleteEsent(cacheStore);
            callback.Invoke(result);
        }

        private void MigrateCacheItem(EsentPersistentDictionary cacheStore, IDbConnection db, string key)
        {
            var v = cacheStore.Get<CacheEntry<FileSystemItem>>(key);
            var c = new CacheItem
            {
                Id = key.SubstringAfter("_"),
                Date = v.Date,
                Expiration = v.Expiration,
                Size = v.Size,
                Title = v.Content.Title,
                Type = (int)v.Content.Type,
                TitleType = (int)v.Content.TitleType,
                ContentType = (int)v.Content.ContentType,
                Thumbnail = v.Content.Thumbnail,
                Content = string.IsNullOrEmpty(v.TempFilePath) ? null : File.ReadAllBytes(v.TempFilePath),
                RecognitionState = (int)v.Content.RecognitionState
            };
            db.Insert(c);
        }

        private static void MigrateFtpConnection(EsentPersistentDictionary cacheStore, IDbConnection db, string key)
        {
            var v = cacheStore.Get<FtpConnection>(key);
            db.Insert(v);
        }

        private void MigrateStatistics(EsentPersistentDictionary cacheStore, string key, Statistics statistics)
        {
            var entryKey = key.SubstringAfter("_");
            switch (entryKey)
            {
                case "TotalFilesTransferred":
                    statistics.TotalFilesTransferred = cacheStore.Get<int>(key);
                    break;
                case "TotalBytesTransferred":
                    statistics.TotalBytesTransferred = cacheStore.Get<long>(key);
                    break;
                case "TotalTimeSpentWithTransfer":
                    statistics.TotalTimeSpentWithTransfer = cacheStore.Get<TimeSpan>(key);
                    break;
                case "TotalUsageTime":
                    statistics.TotalUsageTime = cacheStore.Get<TimeSpan>(key);
                    break;
                case "ApplicationStarted":
                    statistics.ApplicationStarted = cacheStore.Get<int>(key);
                    break;
                case "ApplicationCrashed":
                    statistics.ApplicationCrashed = cacheStore.Get<int>(key);
                    break;
            }
        }

        private static void MigrateIgnoredMessages(IDbConnection db, string key)
        {
            db.Insert(new IgnoredMessage(key.SubstringAfter("_")));
        }

        private static void MigrateUserSettings(EsentPersistentDictionary cacheStore, string key, UserSettings userSettings)
        {
            switch (key)
            {
                case "DisableCustomChrome":
                    userSettings.DisableCustomChrome = cacheStore.Get<bool>(key);
                    break;
                case "UseVersionChecker":
                    userSettings.UseVersionChecker = cacheStore.Get<bool>(key);
                    break;
                case "UseRemoteCopy":
                    userSettings.UseRemoteCopy = cacheStore.Get<bool>(key);
                    break;
                case "UseJqe360":
                    userSettings.UseUnity = cacheStore.Get<bool>(key);
                    break;
                case "ProfileInvalidation":
                    userSettings.ProfileInvalidation = cacheStore.Get<bool>(key);
                    break;
                case "ProfileExpiration":
                    userSettings.ProfileExpiration = cacheStore.Get<int>(key);
                    break;
                case "RecognizedGameExpiration":
                    userSettings.RecognizedGameExpiration = cacheStore.Get<int>(key);
                    break;
                case "PartiallyRecognizedGameExpiration":
                    userSettings.PartiallyRecognizedGameExpiration = cacheStore.Get<int>(key);
                    break;
                case "UnrecognizedGameExpiration":
                    userSettings.UnrecognizedGameExpiration = cacheStore.Get<int>(key);
                    break;
                case "XboxLiveContentExpiration":
                    userSettings.XboxLiveContentExpiration = cacheStore.Get<int>(key);
                    break;
                case "XboxLiveContentInvalidation":
                    userSettings.XboxLiveContentInvalidation = cacheStore.Get<bool>(key);
                    break;
                case "UnknownContentExpiration":
                    userSettings.UnknownContentExpiration = cacheStore.Get<int>(key);
                    break;
                case "LeftPaneType":
                    userSettings.LeftPaneType = cacheStore.Get<string>(key);
                    break;
                case "RightPaneType":
                    userSettings.RightPaneType = cacheStore.Get<string>(key);
                    break;
                case "LeftPaneFileListPaneSettings":
                    var l = cacheStore.Get<FileListPaneSettings>(key);
                    userSettings.LeftPaneDirectory = l.Directory;
                    userSettings.LeftPaneSortByField = l.SortByField;
                    userSettings.LeftPaneSortDirection = (int)l.SortDirection;
                    break;
                case "RightPaneFileListPaneSettings":
                    var r = cacheStore.Get<FileListPaneSettings>(key);
                    userSettings.RightPaneDirectory = r.Directory;
                    userSettings.RightPaneSortByField = r.SortByField;
                    userSettings.RightPaneSortDirection = (int)r.SortDirection;
                    break;
                case "DisableUserStatisticsParticipation":
                    userSettings.DisableUserStatisticsParticipation = cacheStore.Get<bool?>(key);
                    break;
            }

        }
        private bool EsentExists()
        {
            return File.Exists(Path.Combine(_esentDir, "PersistentDictionary.edb"));
        }

        private void DeleteEsent(EsentPersistentDictionary cacheStore)
        {
            cacheStore.Dispose();
            Directory.Delete(_esentDir, true);
        }

        private void UpgradeEsentToLatestVersion(EsentPersistentDictionary cacheStore)
        {
            const int requiredCacheVersion = 3;
            var actualCacheVersion = cacheStore.TryGet("CacheVersion", 1);
            if (actualCacheVersion == requiredCacheVersion) return;

            var cacheKeys = cacheStore.Keys.Where(k => k.StartsWith(Strings.UserMessageCacheItemPrefix)).ToList();
            if (cacheKeys.Any())
            {
                var resx = Resources.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false).Cast<DictionaryEntry>().ToArray();
                SetItemsCount(resx.Length);
                foreach (var entry in resx)
                {
                    var resxKey = (string)entry.Key;
                    if (!resxKey.EndsWith("Message")) continue;
                    var resxValue = (string)entry.Value;
                    var cacheKey = cacheKeys.FirstOrDefault(k => k == Strings.UserMessageCacheItemPrefix + resxValue.Hash());
                    if (cacheKey == null) continue;
                    cacheStore.Remove(cacheKey);
                    cacheStore.Set(Strings.UserMessageCacheItemPrefix + resxKey.Hash(), true);
                    IncrementItemsMigrated();
                }
            }

            if (actualCacheVersion != 1) return;

            var keys = cacheStore.Keys;
            var cacheEntries = keys.Where(k => k.StartsWith("CacheEntry_")).ToArray();
            var payload = _resourceManager.GetContentByteArray("/Resources/Items/32x32/xbox_logo.png");
            SetItemsCount(cacheEntries.Length);
            foreach (var hashKey in cacheEntries)
            {
                var entry = cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);
                if (entry.Content != null && !string.IsNullOrEmpty(entry.Content.Title))
                {
                    if (entry.Content.TitleType != TitleType.Game || entry.Content.RecognitionState != RecognitionState.NotRecognized) continue;

                    entry.Content.RecognitionState = entry.Content.Thumbnail.EqualsWith(payload)
                                                         ? RecognitionState.PartiallyRecognized
                                                         : RecognitionState.Recognized;
                    cacheStore.Update(hashKey, entry);
                }
                else
                {
                    cacheStore.Remove(hashKey);
                }
                IncrementItemsMigrated();
            }
        }

        private void SetItemsCount(int value)
        {
            UIThread.Run(() =>
                             {
                                 ItemsCount = value;
                                 ItemsMigrated = 0;
                             });
        }

        private void IncrementItemsMigrated()
        {
            UIThread.Run(() => ItemsMigrated++);
        }
    }
}