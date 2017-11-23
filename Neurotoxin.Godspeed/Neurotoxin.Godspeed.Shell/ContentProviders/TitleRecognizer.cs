using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Database.Models;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using ServiceStack;
using WPFLocalizeExtension.Engine;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class TitleRecognizer : ITitleRecognizer
    {
        private readonly IFileManager _fileManager;
        private readonly ICacheManager _cacheManager;
        private readonly IUserSettingsProvider _userSettingsProvider;
        private readonly IEventAggregator _eventAggregator;

        private static readonly List<RecognitionInformation> RecognitionKeywords = new List<RecognitionInformation>
            {
                new RecognitionInformation("^0000000000000000$", Resx.Games, TitleType.SystemDir, ItemType.Directory),
                new RecognitionInformation("^584E07D2$", Resx.XNAIndiePlayer, TitleType.SystemDir),
                new RecognitionInformation("^FFFE07C3$", Resx.GamerPicturePlural, TitleType.SystemDir),
                new RecognitionInformation("^FFFE07D1$", Resx.ProfileSingular, TitleType.SystemDir),
                new RecognitionInformation("^FFFE07DF$", Resx.AvatarEditor, TitleType.SystemDir),
                new RecognitionInformation("^F[0-9A-F]{7}$", Resx.SystemData, TitleType.SystemDir),
                new RecognitionInformation("^F[0-9A-F]{7}.gpd$", Resx.SystemData, TitleType.SystemFile, ItemType.File),
                new RecognitionInformation("^[1-9A-E][0-9A-F]{7}$", Resx.UnknownGame, TitleType.Game),
                new RecognitionInformation("^[1-9A-E][0-9A-F]{7}.gpd$", Resx.UnknownGame, TitleType.Game, ItemType.File),
                new RecognitionInformation("^[0-9A-F]{8}$", Resx.UnknownContent, TitleType.Content),
                new RecognitionInformation("^E[0-9A-F]{15}$", Resx.UnknownProfile, TitleType.Profile, ItemType.Directory | ItemType.File),
                new RecognitionInformation("^TU[\\w\\.]+$|^[0-9A-F]{4,}$", Resx.UnknownDataFile, TitleType.DataFile, ItemType.File),
            };

        public Func<FileSystemItem, bool> RecognizeInaccessibleProfile { get; set; }

        public TitleRecognizer(IFileManager fileManager, ICacheManager cacheManager, IUserSettingsProvider userSettingsProvider, IEventAggregator eventAggregator)
        {
            _fileManager = fileManager;
            _cacheManager = cacheManager;
            _userSettingsProvider = userSettingsProvider;
            _eventAggregator = eventAggregator;
        }

        public bool RecognizeType(FileSystemItem item)
        {
            var recognition = RecognizeByName(item.Name, item.Type);
            if (recognition == null) return false;

            item.TitleType = recognition.TitleType;
            if (recognition.TitleType == TitleType.Content)
            {
                var content = BitConverter.ToInt32(item.Name.FromHex(), 0);
                if (Enum.IsDefined(typeof (ContentType), content))
                {
                    item.ContentType = (ContentType) content;
                    item.Title = GetContentTypeTitle(item.ContentType);
                }
            }
            else
            {
                item.Title = recognition.Title;
            }
            return true;
        }

        public static string GetTitle(string name)
        {
            var recognition = RecognizeByName(name);
            if (recognition == null) return null;
            if (recognition.TitleType == TitleType.Content)
            {
                var content = BitConverter.ToInt32(name.FromHex(), 0);
                if (Enum.IsDefined(typeof (ContentType), content))
                    return GetContentTypeTitle((ContentType) content);
            }
            return recognition.Title;
        }

        private static string GetContentTypeTitle(ContentType contentType)
        {
            return Resx.ResourceManager.GetString(contentType + "Plural", LocalizeDictionary.Instance.Culture);
        }

        private static RecognitionInformation RecognizeByName(string name, ItemType? flag = null)
        {
            var recognition = RecognitionKeywords.FirstOrDefault(r => new Regex(r.Pattern, RegexOptions.IgnoreCase).IsMatch(name));
            return recognition != null && flag.HasValue && !recognition.ItemTypeFlags.HasFlag(flag.Value) ? null : recognition;
        }

        public bool MergeWithCachedEntry(FileSystemItem item)
        {
            if (item.TitleType == TitleType.Unknown ||
                item.TitleType == TitleType.SystemDir || 
                item.TitleType == TitleType.SystemFile)
            {
                return true;
            }

            var cacheKey = GetCacheKey(item);
            var storedItem = _cacheManager.Get(cacheKey.Key);
            if (storedItem != null) MergeItems(item, storedItem);

            if (cacheKey.Item == null)
            {
                item.IsCached = true;
                item.IsLocked = true;
                item.LockMessage = cacheKey.ErrorMessage ?? Resx.InaccessibleFileErrorMessage;
                if (item.TitleType == TitleType.Profile)
                {
                    var common = _cacheManager.Get(item.Name);
                    if (common != null) MergeItems(item, common);
                    else if (RecognizeInaccessibleProfile != null && RecognizeInaccessibleProfile.Invoke(item))
                            SaveProfileCache(cacheKey, item, null);
                }
            } 
            else
            {
                var validCacheItem = _cacheManager.Get(cacheKey);
                item.IsCached = validCacheItem != null;
            }

            return item.IsCached;
        }

        private static void MergeItems(FileSystemItem a, CacheItem b)
        {
            if (b == null) return;

            a.Title = b.Title;
            a.TitleType = (TitleType)b.TitleType;
            a.ContentType = (ContentType)b.ContentType;
            a.Thumbnail = b.Thumbnail;
            a.RecognitionState = (RecognitionState)b.RecognitionState;
        }

        public CacheItem RecognizeTitle(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            CacheItem cacheItem = null;

            switch (item.TitleType)
            {
                case TitleType.Profile:
                    var content = GetProfileData(item, cacheKey.Item);
                    if (content == null && RecognizeInaccessibleProfile != null) RecognizeInaccessibleProfile.Invoke(item);
                    cacheItem = SaveProfileCache(cacheKey, item, content);
                    break;
                case TitleType.Game:
                    if (item.Type == ItemType.File)
                    {
                        GetGameDataFromGpd(item);
                    } 
                    else if (!GetGameData(item))
                    {
                        if (_userSettingsProvider.UseUnity) GetGameDataFromUnity(item);
                    }
                    cacheItem = SaveGameCache(cacheKey, item);
                    break;
                case TitleType.Content:
                    cacheItem = _cacheManager.Set(cacheKey.Key, item);
                    item.IsCached = true;
                    break;
                case TitleType.DataFile:
                    if (item.Type == ItemType.File)
                    {
                        try
                        {
                            var header = _fileManager.ReadFileContent(item.Path, StfsPackage.DefaultHeaderSizeVersion1 + 12);
                            var svod = ModelFactory.GetModel<SvodPackage>(header);
                            if (svod.IsValid)
                            {
                                item.Title = svod.DisplayName;
                                if (svod.InstallerType == InstallerType.TitleUpdate)
                                {
                                    var ver = svod.ReadValue<Version>(StfsPackage.DefaultHeaderSizeVersion1 + 8, 4, new BinaryDataAttribute(EndianType.BigEndian));
                                    item.Title = string.Format("{0} (TU{1})", svod.DisplayName, ver.Build);
                                }
                                if (!svod.ThumbnailImage.All(b => b == 0)) item.Thumbnail = svod.ThumbnailImage;
                                item.ContentType = svod.ContentType;
                                item.RecognitionState = RecognitionState.Recognized;
                            }
                            cacheItem = SaveDataFileCache(cacheKey, item);
                        } 
                        catch
                        {
                            item.IsLocked = true;
                            item.LockMessage = Resx.InaccessibleFileErrorMessage;
                        }
                        item.IsCached = true;
                    }
                    break;
            }
            return cacheItem;
        }

        private CacheItem SaveProfileCache(CacheComplexKey cacheKey, FileSystemItem item, byte[] content)
        {
            var profileExpiration = GetExpirationFrom(_userSettingsProvider.ProfileExpiration);
            item.IsCached = true;
            var cacheItem = _userSettingsProvider.ProfileInvalidation
                            ? _cacheManager.Set(cacheKey.Key, item, profileExpiration, cacheKey.Date, cacheKey.Size, content)
                            : _cacheManager.Set(cacheKey.Key, item, profileExpiration);
            _cacheManager.Set(item.Name, item);
            return cacheItem;
        }

        private CacheItem SaveGameCache(CacheComplexKey cacheKey, FileSystemItem item)
        {
            var gameExpiration = GetGameExpiration(item.RecognitionState);
            item.IsCached = true;
            return _cacheManager.Set(cacheKey.Key, item, GetExpirationFrom(gameExpiration));
        }

        private CacheItem SaveDataFileCache(CacheComplexKey cacheKey, FileSystemItem item)
        {
            if (item.RecognitionState == RecognitionState.Recognized)
            {
                var svodExpiration = GetExpirationFrom(_userSettingsProvider.XboxLiveContentExpiration);
                return _userSettingsProvider.XboxLiveContentInvalidation
                           ? _cacheManager.Set(cacheKey.Key, item, svodExpiration, item.Date, item.Size)
                           : _cacheManager.Set(cacheKey.Key, item, svodExpiration);
            }
            return _cacheManager.Set(cacheKey.Key, item, GetExpirationFrom(_userSettingsProvider.UnknownContentExpiration));
        }

        private static DateTime? GetExpirationFrom(int expiration)
        {
            if (expiration == 0) return null;
            return DateTime.Now.AddDays(expiration);
        }

        private int GetGameExpiration(RecognitionState recognitionState)
        {
            switch (recognitionState)
            {
                case RecognitionState.Recognized:
                    return _userSettingsProvider.RecognizedGameExpiration;
                case RecognitionState.PartiallyRecognized:
                    return _userSettingsProvider.PartiallyRecognizedGameExpiration;
                case RecognitionState.NotRecognized:
                    return _userSettingsProvider.UnrecognizedGameExpiration;
                default:
                    throw new NotSupportedException("Invalid recognition state value: " + recognitionState);
            }
        }

        private ProfileItemWrapper GetProfileItem(FileSystemItem item)
        {
            const string pattern = "{1}FFFE07D1{2}00010000{2}{0}";
            var profileFullPath = string.Format(pattern, item.Name, item.FullPath, _fileManager.Slash);
            string message = null;
            var profilePath = string.Format(pattern, item.Name, item.Path, _fileManager.Slash);
            var profileItem = _fileManager.GetItemInfo(profilePath);
            if (profileItem != null)
            {
                if (profileItem.Type == ItemType.File)
                {
                    RecognizeType(profileItem);
                }
                else
                {
                    profileItem = null;
                    message = Resx.ProfileIsInUseErrorMessage;
                }
            } 
            else
            {
                message = Resx.ProfileDoesntExistErrorMessage;
            }
            return new ProfileItemWrapper(profileFullPath, profileItem, message); ;
        }

        private CacheComplexKey GetCacheKey(FileSystemItem item)
        {
            var key = new CacheComplexKey();
            switch (item.TitleType)
            {
                case TitleType.SystemDir:
                case TitleType.SystemFile:
                case TitleType.Content:
                    key.Item = item;
                    key.Key = item.Name;
                    break;
                case TitleType.Game:
                    key.Item = item;
                    key.Key = item.Type == ItemType.File ? item.Name.Replace(".gpd", string.Empty) : item.Name;
                    break;
                case TitleType.Profile:
                    if (item.Type == ItemType.File)
                    {
                        key.Item = item;
                        key.Key = item.FullPath;
                    } 
                    else
                    {
                        var recognition = GetProfileItem(item);
                        key.Item = recognition.Item;
                        key.Key = recognition.Path;
                        key.ErrorMessage = recognition.ErrorMessage;
                    }
                    break;
                case TitleType.DataFile:
                    key.Item = item;
                    key.Key = item.FullPath;
                    break;
            }
            if (key.Item != null)
            {
                key.Size = key.Item.Size;
                key.Date = key.Item.Date;
            }
            return key;
        }

        private byte[] GetProfileData(FileSystemItem item, FileSystemItem cacheItem)
        {
            if (cacheItem == null) return null;

            try
            {
                var bytes = _fileManager.ReadFileContent(cacheItem.Path);

                var stfs = ModelFactory.GetModel<StfsPackage>(bytes);
                stfs.ExtractAccount();
                item.Title = stfs.Account.GamerTag;
                item.Thumbnail = stfs.ThumbnailImage;
                item.ContentType = stfs.ContentType;
                item.RecognitionState = RecognitionState.Recognized;
                return bytes;
            }
            catch (Exception ex)
            {
                item.IsLocked = true;
                //TODO: exception message in non-English environment
                item.LockMessage = ex.Message == "Permission denied" || ex.Message == "Could not open file"
                    ? Resx.ProfileIsInUseErrorMessage
                    : ex.Message;
                return null;
            }
        }

        private bool GetGameData(FileSystemItem item)
        {
            var infoFileFound = false;

            string gamePath = null;
            var systemdirs = new[] { item.Name.StartsWith("5841") ? "000D0000" : "00007000", "00080000" };
            var exists = false;
            foreach (var systemdir in systemdirs)
            {
                gamePath = item.Path + systemdir;
                exists = _fileManager.FolderExists(gamePath);
                if (exists) break;

                var r = new Regex(@"(?<content>Content[\\/])E0000[0-9A-F]{11}", RegexOptions.IgnoreCase);
                if (!r.IsMatch(gamePath)) continue;

                gamePath = r.Replace(gamePath, "${content}0000000000000000");
                exists = _fileManager.FolderExists(gamePath);
                if (exists) break;
            }

            if (!exists) return false;

            try
            {
                var file = _fileManager.GetList(gamePath).FirstOrDefault(i => i.Type == ItemType.File);
                if (file != null)
                {
                    var fileContent = _fileManager.ReadFileContent(file.Path, StfsPackage.DefaultHeaderSizeVersion1);
                    var svod = ModelFactory.GetModel<SvodPackage>(fileContent);
                    if (svod.IsValid)
                    {
                        item.Title = svod.TitleName;
                        item.Thumbnail = svod.ThumbnailImage;
                        item.ContentType = svod.ContentType;
                        item.RecognitionState = RecognitionState.Recognized;
                        infoFileFound = true;
                    }
                }
            }
            catch { }

            return infoFileFound;
        }

        private bool GetGameDataFromUnity(FileSystemItem item)
        {
            string title = null;
            byte[] thumbnail = null;
            var result = false;
            try
            {
                var webClient = new WebClient();
                var json = webClient.DownloadString(string.Format("http://xboxunity.net/Resources/Lib/TitleList.php?search={0}", item.Name));
                if (!string.IsNullOrEmpty(json))
                {
                    var unityResponse = json.FromJson<UnityResponse>();
                    var firstResult = unityResponse.Items.FirstOrDefault();
                    title = firstResult != null ? firstResult.Name.Trim() : null;
                    result = !string.IsNullOrEmpty(title);
                    if (result)
                    {
                        var bytes = webClient.DownloadData(string.Format("http://xboxunity.net/Resources/Lib/Icon.php?tid={0}", item.Name));
                        if (bytes != null && webClient.ResponseHeaders[HttpResponseHeader.ContentType] == "image/png") 
                            thumbnail = bytes;
                    }
                }
            }
            catch {}

            if (result)
            {
                item.Title = title;
                if (thumbnail != null)
                {
                    item.Thumbnail = thumbnail;
                    item.RecognitionState = RecognitionState.Recognized;
                } 
                else
                {
                    UIThread.Run(() => _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("PartialRecognitionMessage", MessageIcon.Warning)));
                    item.RecognitionState = RecognitionState.PartiallyRecognized;
                }
            }
            return result;
        }

        private bool GetGameDataFromGpd(FileSystemItem item)
        {
            try
            {
                var fileContent = _fileManager.ReadFileContent(item.Path);
                var gpd = ModelFactory.GetModel<GameFile>(fileContent);
                gpd.Parse();
                if (gpd.Strings.Count > 0) item.Title = gpd.Strings.First().Text;
                item.Thumbnail = gpd.Thumbnail;
                item.RecognitionState = RecognitionState.Recognized;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateTitle(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            var cacheItem = _cacheManager.Get(cacheKey);
            if (cacheItem == null) return false;
            if (_cacheManager.ContainsKey(cacheKey.Key))
            {
                _cacheManager.UpdateTitle(cacheKey.Key, item);    
            }
            else
            {
                switch (item.TitleType)
                {
                    case TitleType.Game:
                        SaveGameCache(cacheKey, item);
                        break;
                    case TitleType.Profile:
                        SaveProfileCache(cacheKey, item, null);
                        break;
                    case TitleType.Content:
                        _cacheManager.Set(cacheKey.Key, item);
                        break;
                    case TitleType.DataFile:
                        SaveDataFileCache(cacheKey, item);
                        break;
                }
            }
            return true;
        }

        public void ThrowCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            if (cacheKey.Key != null) _cacheManager.Remove(cacheKey.Key);
        }

        public byte[] GetBinaryContent(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            var cacheEntry = _cacheManager.Get(cacheKey);
            if (cacheEntry == null)
            {
                cacheEntry = RecognizeTitle(item);
                if (cacheEntry == null)
                {
                    throw new ApplicationException(string.Format("Item cannot be recognized anymore: {0}", cacheKey.Item.Path));
                }
            }
            return _cacheManager.GetBinaryContent(cacheKey.Key);
        }

        public CacheItem GetCacheEntry(FileSystemItem item)
        {
            CacheComplexKey cacheKey;
            return GetCacheEntry(item, out cacheKey);
        }

        public CacheItem GetCacheEntry(FileSystemItem item, out CacheComplexKey cacheKey)
        {
            cacheKey = GetCacheKey(item);
            return _cacheManager.Get(cacheKey);
        }

        public CacheItem GetCacheEntry(string key)
        {
            return _cacheManager.Get(key);
        }
    }
}