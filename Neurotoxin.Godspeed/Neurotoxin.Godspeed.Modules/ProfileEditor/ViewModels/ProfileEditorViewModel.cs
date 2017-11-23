using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Presentation.ViewModels;
using Neurotoxin.Godspeed.Presentation.Extensions;
using System.Linq;

namespace Neurotoxin.Godspeed.Modules.ProfileEditor.ViewModels
{
    /// <summary>
    /// ViewModel for ProfileEditorView.
    /// </summary>
    public class ProfileEditorViewModel : ModuleViewModelBase
    {
        private string _path;
        private string _otherPath;
        private StfsPackage _profile;

        #region Properties

        private const string TITLEID = "TitleId";
        private string _titleId;
        public string TitleId
        {
            get { return _titleId; }
            set { _titleId = value; NotifyPropertyChanged(TITLEID); }
        }

        private const string CONSOLEID = "ConsoleId";
        private string _consoleId;
        public string ConsoleId
        {
            get { return _consoleId; }
            set { _consoleId = value; NotifyPropertyChanged(CONSOLEID); }
        }

        private const string PROFILEID = "ProfileId";
        private string _profileId;
        public string ProfileId
        {
            get { return _profileId; }
            set { _profileId = value; NotifyPropertyChanged(PROFILEID); }
        }

        private const string DEVICEID = "DeviceId";
        private string _deviceId;
        public string DeviceId
        {
            get { return _deviceId; }
            set { _deviceId = value; NotifyPropertyChanged(DEVICEID); }
        }

        private const string DISPLAYNAME = "DisplayName";
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; NotifyPropertyChanged(DISPLAYNAME); }
        }

        private const string TITLENAME = "TitleName";
        private string _titleName;
        public string TitleName
        {
            get { return _titleName; }
            set { _titleName = value; NotifyPropertyChanged(TITLENAME); }
        }

        private const string THUMBNAIL = "Thumbnail";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; NotifyPropertyChanged(THUMBNAIL); }
        }

        private const string TITLETHUMBNAIL = "TitleThumbnail";
        private ImageSource _titleThumbnail;
        public ImageSource TitleThumbnail
        {
            get { return _titleThumbnail; }
            set { _titleThumbnail = value; NotifyPropertyChanged(TITLETHUMBNAIL); }
        }

        private const string TREE = "Tree";
        private ObservableCollection<TreeItemViewModel> _tree;
        public ObservableCollection<TreeItemViewModel> Tree
        {
            get { return _tree; }
            set { _tree = value; NotifyPropertyChanged(TREE); }
        }

        private const string SELECTEDFILE = "SelectedFile";
        private TreeItemViewModel _selectedFile;
        public TreeItemViewModel SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; NotifyPropertyChanged(SELECTEDFILE); }
        }

        private const string GAMERSCORE = "Gamerscore";
        private int _gamerscore;
        public int Gamerscore
        {
            get { return _gamerscore; }
            set { _gamerscore = value; NotifyPropertyChanged(GAMERSCORE); }
        }

        private const string CALCULATEDGAMERSCORE = "CalculatedGamerscore";
        private int _calculatedGamerscore;
        public int CalculatedGamerscore
        {
            get { return _calculatedGamerscore; }
            set { _calculatedGamerscore = value; NotifyPropertyChanged(CALCULATEDGAMERSCORE); }
        }

        private const string CALCULATEDGAMERSCORE2 = "CalculatedGamerscore2";
        private int _calculatedGamerscore2;
        public int CalculatedGamerscore2
        {
            get { return _calculatedGamerscore2; }
            set { _calculatedGamerscore2 = value; NotifyPropertyChanged(CALCULATEDGAMERSCORE2); }
        }

        private const string ACHIEVEMENTS = "Achievements";
        private ObservableCollection<Achievement> _achievements;
        public ObservableCollection<Achievement> Achievements
        {
            get { return _achievements; }
            set { _achievements = value; NotifyPropertyChanged(ACHIEVEMENTS); }
        }

        private const string SELECTEDACHIEVEMENT = "SelectedAchievement";
        private Achievement _selectedAchievement;
        public Achievement SelectedAchievement
        {
            get { return _selectedAchievement; }
            set { _selectedAchievement = value; NotifyPropertyChanged(SELECTEDACHIEVEMENT); }
        }

        private ObservableCollection<Game> _gameList;
        public ObservableCollection<Game> GameList
        {
            get { return _gameList; }
            set 
            { 
                _gameList = value;
                Games = value;
            }
        }

        private const string GAMES = "Games";
        private ObservableCollection<Game> _games;
        public ObservableCollection<Game> Games
        {
            get { return _games; }
            set { _games = value; NotifyPropertyChanged(GAMES); }
        }

        private const string GAMESFILTER = "GamesFilter";
        private string _gamesFilter;
        public string GamesFilter
        {
            get { return _gamesFilter; }
            set 
            { 
                _gamesFilter = value;
                Games = String.IsNullOrEmpty(value)
                            ? GameList
                            : GameList.Where(g => g.Title.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
                                      .ToObservableCollection();
                if (!Games.Contains(SelectedGame)) SelectedGame = null;
                NotifyPropertyChanged(GAMESFILTER); 
            }
        }

        private const string SELECTEDGAME = "SelectedGame";
        private Game _selectedGame;
        public Game SelectedGame
        {
            get { return _selectedGame; }
            set
            {
                _selectedGame = value;
                NotifyPropertyChanged(SELECTEDGAME);
                if (value == null) return;
                var game = _profile.GetGameFile(value.TitleId + ".gpd");
                game.Parse();
                Achievements = game.Achievements.OrderBySyncList().Select(a =>
                {
                    var thumbnail = game.Images.FirstOrDefault(i => i.Entry.Id == a.ImageId);
                    return new Achievement
                    {
                        AchievementId = a.AchievementId,
                        Gamerscore = a.Gamerscore,
                        Name = a.Name,
                        Description = a.IsUnlocked ? a.UnlockedDescription : a.LockedDescription,
                        Thumbnail = thumbnail != null ? GetThumbnail(thumbnail.ImageData) : null,
                        State = a.IsUnlocked ? "Unlocked" : a.IsSecret ? "Secret" : "Locked",
                        UnlockTime = a.UnlockTime
                    };
                }).ToObservableCollection();
            }
        }

        #endregion

        public override bool HasDirty()
        {
            throw new NotImplementedException();
        }

        protected override void ResetDirtyFlags()
        {
            throw new NotImplementedException();
        }

        public override bool IsDirty(string propertyName)
        {
            throw new NotImplementedException();
        }

        #region Commands

        public DelegateCommand<object> OpenInHexViewerCommand { get; private set; }

        private void ExecuteOpenInHexViewerCommand(object cmdParam)
        {
            GpdFile gpd = SelectedFile.Name == _profile.TitleId + ".gpd"
                              ? _profile.ProfileInfo as GpdFile
                              : _profile.GetGameFile(SelectedFile.Name);

            if (gpd != null)
            {
                var mli = new ModuleLoadInfo
                              {
                                  ModuleName = "HexViewer",
                                  Title = SelectedFile.Name,
                                  LoadCommand = LoadCommand.Load,
                                  LoadParameter = new Tuple<byte[], BinMap>(gpd.Binary.ReadAll(), gpd.BinMap)
                              };
                EventAggregator.GetEvent<ModuleOpenEvent>().Publish(new ModuleOpenEventArgs(mli));
            }
        }

        private bool CanExecuteOpenInHexViewerCommand(object cmdParam)
        {
            return SelectedFile != null && !SelectedFile.IsDirectory;
        }

        public DelegateCommand<object> MergeCommand { get; private set; }

        private void ExecuteMergeCommand(object cmdParam)
        {
            _otherPath = @"..\..\..\..\Resources\mergeable\E0000027FA233BE2";
            //_otherPath = @"..\..\..\..\Resources\newmergeapproach\E00001D5D85ED487.0114";
            LoadSubscribe();
            WorkHandler.Run(Merge, MergeCallback);
        }

        public DelegateCommand<object> SaveCommand { get; private set; }

        private void ExecuteSaveCommand(object cmdParam)
        {
            _profile.Save(@"c:\merc\Contour\Resources\woodpaul\E0000546AE28F8B7.out");
            //_profile.Save(@"..\..\..\..\Resources\newmergeapproach\0112_0114.out");
        }

        public DelegateCommand<object> ExtractCommand { get; private set; }

        private void ExecuteExtractCommand(object cmdParam)
        {
            _profile.ExtractAll(@"..\..\..\..\Resources\mergeable\ext_2");
        }

        public DelegateCommand<object> UnlockAchievementCommand { get; private set; }

        private void ExecuteUnlockAchievementCommand(object cmdParam)
        {
            _profile.UnlockAchievement(SelectedGame.TitleId, SelectedAchievement.AchievementId, File.ReadAllBytes(@"..\..\..\..\Resources\mergeable\aktualis\30.png"));
            SelectedGame = SelectedGame;
            CalculatedGamerscore2 = _profile.Games.Values.Sum(g => g.Gamerscore);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            OpenInHexViewerCommand.RaiseCanExecuteChanged();
        }

        #endregion

        public ProfileEditorViewModel()
        {
            OpenInHexViewerCommand = new DelegateCommand<object>(ExecuteOpenInHexViewerCommand, CanExecuteOpenInHexViewerCommand);
            MergeCommand = new DelegateCommand<object>(ExecuteMergeCommand);
            SaveCommand = new DelegateCommand<object>(ExecuteSaveCommand);
            ExtractCommand = new DelegateCommand<object>(ExecuteExtractCommand);
            UnlockAchievementCommand = new DelegateCommand<object>(ExecuteUnlockAchievementCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    _path = (string) cmdParam;
                    LoadSubscribe();
                    WorkHandler.Run(LoadFile, LoadFileCallback);
                    break;
                case LoadCommand.MergeWith:
                    //_profile.MergeWith((StfsPackage)cmdParam);
                    break;
            }
        }

        private void LoadSubscribe()
        {
            IsInProgress = true;
            LoadingQueueLength = 1;
            LoadingProgress = 0;
            //LogHelper.StatusBarChange += LogHelperStatusBarChange;
            //LogHelper.StatusBarMax += LogHelperStatusBarMax;
            //LogHelper.StatusBarText += LogHelperStatusBarText;
        }

        //private void LogHelperStatusBarChange(object sender, ValueChangedEventArgs e)
        //{
        //    UIThread.BeginRun(() => LoadingProgress = e.NewValue);
        //}

        //private void LogHelperStatusBarMax(object sender, ValueChangedEventArgs e)
        //{
        //    UIThread.BeginRun(() =>
        //                          {
        //                              LoadingQueueLength = e.NewValue;
        //                              LoadingProgress = 0;
        //                          });
        //}

        //private void LogHelperStatusBarText(object sender, TextChangedEventArgs e)
        //{
        //    UIThread.BeginRun(() => LoadingInfo = e.Text);
        //}

        private StfsPackage LoadFile()
        {
            var file = File.ReadAllBytes(_path);
            StfsPackage profile = null;
            profile = ModelFactory.GetModel<StfsPackage>(file);
            profile.ExtractContent();
            return profile;
        }

        private void LoadFileCallback(StfsPackage profile)
        {
            _profile = profile;
            Initialize();
            IsInProgress = false;
            //LogHelper.StatusBarChange -= LogHelperStatusBarChange;
            //LogHelper.StatusBarMax -= LogHelperStatusBarMax;
            //LogHelper.StatusBarText -= LogHelperStatusBarText;
            LoadingInfo = "Done.";
        }

        //HACK!
        private StfsPackage Merge()
        {
            var file = File.ReadAllBytes(_otherPath);
            var other = ModelFactory.GetModel<StfsPackage>(file);
            other.ExtractContent();
            _profile.MergeWith(other);
            return other;

            //---

            //var titleEntry = _profile.ProfileInfo.TitlesPlayed.First(t => t.TitleName == "LIMBO");
            //var fileEntry = _profile.GetFileEntry(titleEntry.TitleCode + ".gpd");
            //var game = _profile.GetGameFile(titleEntry.TitleCode + ".gpd", true);
            //var ach1 = game.Achievements.First(a => !a.IsUnlocked);
            //game.UnlockAchievement(ach1, AchievementLockFlags.Unlocked, DateTime.Now, File.ReadAllBytes(@"d:\NTX-CNT\Contour\Resources\xbox_profile\ac2-extact\1.png"));
            //game.Recalculate();
            //game.Rebuild();
            //_profile.ReplaceFile(fileEntry, game);
            //titleEntry.AchievementsUnlocked = game.UnlockedAchievementCount;
            //titleEntry.GamerscoreUnlocked = game.Gamerscore;
            //_profile.ProfileInfo.Recalculate();

            //var x = _profile.TitleId.ToHex();

            ////File.WriteAllLines(@"..\..\..\..\Resources\mergeable\" + x + ".map1", _profile.ProfileInfo.BinMap.Output());

            //_profile.ProfileInfo.Rebuild();

            ////File.WriteAllLines(@"..\..\..\..\Resources\mergeable\" + x + ".map2", _profile.ProfileInfo.BinMap.Output());

            //var profileEntry = _profile.GetFileEntry(x + ".gpd");
            //_profile.ReplaceFile(profileEntry, _profile.ProfileInfo);
            //return _profile;
        }

        private void MergeCallback(StfsPackage profile)
        {
            LoadFileCallback(_profile);
            Gamerscore = _profile.ProfileInfo.GamercardCred;
            CalculatedGamerscore = _profile.ProfileInfo.TitlesPlayed.Sum(t => t.GamerscoreUnlocked);
        }

        private void Initialize()
        {
            TitleId = _profile.TitleId;
            ConsoleId = _profile.ConsoleId;
            ProfileId = _profile.ProfileId;
            DeviceId = _profile.DeviceId.ToHex();
            DisplayName = _profile.DisplayName;
            TitleName = _profile.TitleName;
            Tree = _profile.BuildTreeFromFileListing();
            Thumbnail = _profile.GetThumbnailImage();
            TitleThumbnail = _profile.GetTitleThumbnailImage();
            
            LoadInfo.Title = _profile.Account.GamerTag;
            Gamerscore = _profile.ProfileInfo.GamercardCred;
            CalculatedGamerscore = _profile.ProfileInfo.TitlesPlayed.Sum(t => t.GamerscoreUnlocked);

            const string pattern = "{0}/{1}";

            GameList = _profile.ProfileInfo.TitlesPlayed.Select(g => new Game
            {
                TitleId = g.TitleCode,
                Title = g.TitleName,
                Achievements = String.Format(pattern, g.AchievementsUnlocked, g.AchievementCount),
                Gamerscore = String.Format(pattern, g.GamerscoreUnlocked, g.TotalGamerScore),
                Thumbnail = GetThumbnail(_profile.Games.Values.First(gg => gg.TitleId == g.TitleCode).Thumbnail)
            }).ToObservableCollection();

            CalculatedGamerscore2 = _profile.Games.Values.Sum(g => g.Gamerscore);

            //HACK
            SelectedGame = Games.First(g => g.TitleId == "4D530805");
        }

        private ImageSource GetThumbnail(byte[] image)
        {
            if (image == null)
            {
                return null;
                //return new BitmapImage(new Uri("pack://application:,,,/Neurotoxin.Godspeed.Modules;component/Resources/hidden_achivement.png"));
            }
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(image);
            bitmap.EndInit();
            return bitmap;
        }
    }
}