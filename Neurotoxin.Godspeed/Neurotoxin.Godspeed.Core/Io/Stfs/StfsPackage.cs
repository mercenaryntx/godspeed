using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Gpd.Entries;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Io.Stfs
{
    public abstract class StfsPackage : Package<StfsVolumeDescriptor>
    {
        public const int DefaultHeaderSizeVersion1 = 0x971A;

        public FileEntry FileStructure { get; private set; }
        public List<FileEntry> FlatFileList { get; private set; }
        public Dictionary<FileEntry, GameFile> Games { get; protected set; }
        public Account Account { get; private set; }
        public FileEntry ProfileEntry { get; private set; }
        public DashboardFile ProfileInfo { get; private set; }
        public HashTable TopTable { get; private set; }

        protected List<int> UnallocatedHashEntries { get; set; }
        public Sex Sex { get; private set; }
        protected int TopLevel { get; private set; }
        protected int FirstHashTableAddress { get; private set; }

        protected StfsPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset)
            : base(offsetTable, binary, startOffset)
        {
        }

        #region Read

        protected override void Parse()
        {
            FirstHashTableAddress = ((HeaderSize + 0x0FFF) & 0x7FFFF000);
            Sex = (VolumeDescriptor.BlockSeparation & 1) == 1 ? Sex.Female : Sex.Male;
            if (VolumeDescriptor.AllocatedBlockCount >= 0x70E4)
                throw new NotSupportedException("STFS package too big to handle!");
            TopLevel = VolumeDescriptor.AllocatedBlockCount >= 0xAA ? 1 : 0;

            NotifyActionDuration("Collect hash entries", () =>
                                                       {
                                                           UnallocatedHashEntries = new List<int>();
                                                           TopTable = GetLevelNHashTable(0, TopLevel);
                                                           if (TopLevel == 1)
                                                           {
                                                               TopTable.Tables = new List<HashTable>();
                                                               for (var i = 0; i < TopTable.EntryCount; i++)
                                                               {
                                                                   var table = GetLevelNHashTable(i, 0);
                                                                   TopTable.Entries[i].RealBlock = table.Block;
                                                                   TopTable.Tables.Add(table);
                                                               }
                                                           }
                                                       });
            NotifyActionDuration("Collect file entries", () => FileStructure = ReadFileListing());
        }

        public void SwitchToBackupTables()
        {
            //HACK
            VolumeDescriptor.FileTableBlockNum = 1;
            FileStructure = ReadFileListing();

            //if (Sex == Sex.Female) return;
            //VolumeDescriptor.BlockSeparation = VolumeDescriptor.BlockSeparation == 2 ? 0 : 2;
            //UnallocatedHashEntries = new List<int>();
            //TopTable = GetLevelNHashTable(0, TopLevel);
            //if (TopLevel == 1)
            //{
            //    TopTable.Tables = new List<HashTable>();
            //    for (var i = 0; i < TopTable.EntryCount; i++)
            //    {
            //        var tableEntry = TopTable.Entries[i];
            //        tableEntry.Status = tableEntry.Status == BlockStatus.NewlyAllocated || tableEntry.Status == BlockStatus.PreviouslyAllocated
            //                                ? BlockStatus.Allocated
            //                                : BlockStatus.NewlyAllocated;
            //        var table = GetLevelNHashTable(i, 0);
            //        tableEntry.RealBlock = table.Block;
            //        TopTable.Tables.Add(table);
            //    }
            //}
            //FileStructure = ReadFileListing();
        }

        public void SwitchTables()
        {
            var oldTopTable = TopTable;
            IsModified = true;
            UnallocatedHashEntries = new List<int>();

            var topTableOffset = TopTable.StartOffset;
            var topTableBuffer = Binary.ReadBytes(topTableOffset, 0x1000);
            if (VolumeDescriptor.BlockSeparation == 2)
            {
                topTableOffset -= 0x1000;
                VolumeDescriptor.BlockSeparation = 0;
            }
            else
            {
                topTableOffset += 0x1000;
                VolumeDescriptor.BlockSeparation = 2;
            }
            Binary.WriteBytes(topTableOffset, topTableBuffer, 0, 0x1000);
            TopTable = GetLevelNHashTable(0, TopLevel);

            if (TopLevel == 0) return;

            TopTable.Tables = new List<HashTable>();
            for (var i = 0; i < TopTable.EntryCount; i++)
            {
                var tableEntry = TopTable.Entries[i];
                var offset = oldTopTable.Tables[i].StartOffset;
                var buffer = Binary.ReadBytes(offset, 0x1000);
                if (tableEntry.Status == BlockStatus.NewlyAllocated ||
                    tableEntry.Status == BlockStatus.PreviouslyAllocated)
                {
                    offset -= 0x1000;
                    tableEntry.Status = BlockStatus.Allocated;
                }
                else
                {
                    offset += 0x1000;
                    tableEntry.Status = BlockStatus.NewlyAllocated;
                }
                Binary.WriteBytes(offset, buffer, 0, 0x1000);
                var table = GetLevelNHashTable(i, 0);
                tableEntry.RealBlock = table.Block;
                TopTable.Tables.Add(table);

                for (var j = 0; j < table.EntryCount; j++)
                {
                    var entry = table.Entries[j];
                    if (entry.Status == BlockStatus.NewlyAllocated) entry.Status = BlockStatus.Allocated;
                }
            }
        }

        public HashTable GetLevelNHashTable(int index, int level)
        {
            if (level < 0 || level > TopLevel)
                throw new ArgumentException("Invalid level: " + level);

            var x = TopLevel != level ? 0xAA : 1;
            var current = ComputeLevelNBackingHashBlockNumber(index*x, level);
            int entryCount;

            if (level == TopLevel)
            {
                if (VolumeDescriptor.BlockSeparation == 2) current++;

                entryCount = VolumeDescriptor.AllocatedBlockCount;
                if (entryCount >= 0xAA) entryCount = (entryCount + 0xA9)/0xAA;
            }
            else if (level + 1 == TopLevel)
            {
                var entry = TopTable.Entries[index];
                if (entry.Status == BlockStatus.NewlyAllocated || entry.Status == BlockStatus.PreviouslyAllocated)
                    current++;

                // calculate the number of entries in the requested table
                entryCount = index + 1 == TopTable.EntryCount
                                 ? VolumeDescriptor.AllocatedBlockCount%0xAA
                                 : index == TopTable.EntryCount ? 0 : 0xAA;
            }
            else
            {
                throw new NotSupportedException();
            }

            var currentHashAddress = (current << 0xC) + FirstHashTableAddress;
            Binary.EnsureBinarySize(currentHashAddress + 0x1000);
            var table = ModelFactory.GetModel<HashTable>(Binary, currentHashAddress);
            table.Block = current;
            table.EntryCount = entryCount;

            for (var j = 0; j < 0xAA; j++)
            {
                var entry = table.Entries[j];
                entry.Block = index*0xAA + j;
                entry.RealBlock = GetRealBlockNum(entry.Block.Value);
            }
            if (level == 0)
            {
                var unallocatedEntries =
                    table.Entries.Where(
                        e => e.Status == BlockStatus.Unallocated || e.Status == BlockStatus.PreviouslyAllocated);
                UnallocatedHashEntries.AddRange(unallocatedEntries.Select(e => e.Block.Value));
            }

            return table;
        }

        private int ComputeLevelNBackingHashBlockNumber(int blockNum, int level)
        {
            var blockStep = 0xAB;
            if (Sex == Sex.Male) blockStep++;

            switch (level)
            {
                case 0:
                    if (blockNum < 0xAA) return 0;
                    var num = (blockNum/0xAA)*blockStep + 1;
                    if (Sex == Sex.Male) num++;
                    return num;
                case 1:
                    return blockStep;
                default:
                    throw new NotSupportedException("Invalid level: " + level);
            }
        }

        private FileEntry ReadFileListing()
        {
            var root = ModelFactory.GetModel<FileEntry>();
            root.PathIndicator = 0xFFFF;
            root.Name = "Root";
            root.EntryIndex = 0xFFFF;

            var block = VolumeDescriptor.FileTableBlockNum;

            var fl = new List<FileEntry>();
            for (var x = 0; x < VolumeDescriptor.FileTableBlockCount; x++)
            {
                var currentAddr = GetRealAddressOfBlock(block);
                for (var i = 0; i < 64; i++)
                {
                    var addr = currentAddr + i*0x40;
                    var fe = ModelFactory.GetModel<FileEntry>(Binary, addr);
                    //TODO: remove this if
                    if (block == VolumeDescriptor.FileTableBlockNum && i == 0)
                        fe.FileEntryAddress = addr;
                    fe.EntryIndex = (x*0x40) + i;

                    if (fe.Name != String.Empty)
                        fl.Add(fe);
                }
                var he = GetHashEntry(block);
                block = he.NextBlock;
            }

            FlatFileList = fl;
            BuildFileHierarchy(fl, root);
            return root;
        }

        private int GetRealBlockNum(int blockNum)
        {
            // check for invalid block number
            if (blockNum >= 0x70E4)
                throw new InvalidOperationException("STFS: Block number must be less than 0xFFFFFF.\n");
            var byteGender = Convert.ToByte(Sex);
            var backingDataBlockNumber = (((blockNum + 0xAA)/0xAA) << byteGender) + blockNum;
            if (blockNum >= 0xAA) backingDataBlockNumber += ((blockNum + 0x70E4)/0x70E4) << byteGender;
            return backingDataBlockNumber;
        }

        public int GetRealAddressOfBlock(int blockNum)
        {
            return (GetRealBlockNum(blockNum) << 0x0C) + FirstHashTableAddress;
        }

        //public int GetHashTableAddress(int index, int level)
        //{
        //    var x = TopLevel != level ? 0xAA : 1;
        //    var current = ComputeLevelNBackingHashBlockNumber(index * x, level);
        //    return (current << 0xC) + FirstHashTableAddress;
        //}

        public HashEntry GetHashEntry(int blockNum)
        {
            switch (TopLevel)
            {
                case 0:
                    return TopTable.Entries[blockNum];
                case 1:
                    return TopTable.Tables[blockNum / 0xAA].Entries[blockNum % 0xAA];
                default:
                    throw new NotSupportedException("Not supported table level");
            }
        }

        private static void BuildFileHierarchy(List<FileEntry> entries, FileEntry parentFolder)
        {
            foreach (var entry in entries.Where(entry => entry.PathIndicator == parentFolder.EntryIndex))
            {
                // add it if it's a file
                if (!entry.IsDirectory) parentFolder.Files.Add(entry);
                // if it's a directory and not the current directory, then add it
                else if (entry.EntryIndex != parentFolder.EntryIndex) parentFolder.Folders.Add(entry);
            }

            // for every folder added, add the files to them
            foreach (var entry in parentFolder.Folders)
                BuildFileHierarchy(entries, entry);
        }

        public FileEntry GetFolderEntry(string path, bool allowNull = false)
        {
            var folder = FileStructure;
            var parts = path.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
            var i = parts.Length > 0 && parts[0] == "Root" ? 1 : 0;
            while (i < parts.Length)
            {
                folder = folder.Folders.FirstOrDefault(f => f.Name == parts[i]);
                if (folder == null) break;
                i++;
            }
            if (folder == null && !allowNull)
                throw new DirectoryNotFoundException("Folder does not exists in the package: " + parts[i]);
            return folder;
        }

        public FileEntry GetFileEntry(string path, bool allowNull = false)
        {
            var filename = Path.GetFileName(path);
            var folderPath = string.IsNullOrEmpty(filename) ? path : path.Replace(filename, string.Empty);
            var folder = GetFolderEntry(folderPath, allowNull);
            var file = folder.Files.FirstOrDefault(f => f.Name == filename);
            if (allowNull) return file;
            if (file == null)
                throw new FileNotFoundException("File does not exists in the package: " + filename);
            return file;
        }

        #endregion

        #region Extract

        private void ExtractProfile()
        {
            NotifyActionDuration("Extract Profile", () =>
            {
                ProfileEntry = GetFileEntry(TitleId + ".gpd");
                ProfileInfo = ModelFactory.GetModel<DashboardFile>(ExtractFile(ProfileEntry));
                ProfileInfo.Parse();
            });
        }

        public void ExtractAccount()
        {
            NotifyActionDuration("Extract Account", () =>
            {
                var account = ExtractFile("Account");
                Account = Account.Decrypt(new MemoryStream(account), ConsoleType.Retail);
            });
        }

        public virtual void ExtractGames()
        {
            NotifyActionDuration("Extract Game files", () =>
            {
                if (ProfileInfo == null) ExtractProfile();
                var games = FileStructure.Files.Where(f => Path.GetExtension(f.Name) == ".gpd"
                                                        && ProfileInfo.TitlesPlayed.Any(t => t.TitleCode == Path.GetFileNameWithoutExtension(f.Name)));
                var count = games.Count();
                NotifyContentCountDetermined(count);
                Games = new Dictionary<FileEntry, GameFile>();
                foreach (var game in games.Select(gpd => GetGameFile(gpd)))
                {
                    NotifyContentParsed(game);
                }
            });
        }

        public ProfileEmbeddedContent ExtractPec()
        {
            var pec = ExtractFile("PEC");
            var model = ModelFactory.GetModel<ProfileEmbeddedContent>(pec);
            model.ExtractGames();
            return model;
        }

        public void ExtractContent()
        {
            ExtractProfile();
            ExtractGames();
            ExtractAccount();
        }

        public GameFile GetGameFile(string fileName, bool parse = false)
        {
            if (fileName.StartsWith("F") || !fileName.EndsWith(".gpd"))
                throw new NotSupportedException("Invalid file: " + fileName);

            var entry = GetFileEntry(fileName, true);
            return entry == null ? null : GetGameFile(entry, parse);
        }

        protected GameFile GetGameFile(FileEntry entry, bool parse = false)
        {
            if (!Games.ContainsKey(entry))
            {
                var file = ExtractFile(entry);
                return CreateGameFileModel(entry, file, parse);
            }
            var game = Games[entry];
            if (parse && !game.IsParsed) game.Parse();
            return game;
        }

        private GameFile CreateGameFileModel(FileEntry entry, byte[] binary, bool parse)
        {
            var game = ModelFactory.GetModel<GameFile>(binary);
            if (parse) game.Parse();
            game.TitleId = Path.GetFileNameWithoutExtension(entry.Name);
            Games.Add(entry, game);
            return game;
        }

        public void ExtractAll(string outPath)
        {
            ExtractDirectory(FileStructure, outPath);
        }

        public void ExtractDirectory(FileEntry folder, string outPath)
        {
            var dir = Path.Combine(outPath, folder.Name);
            Directory.CreateDirectory(dir);
            foreach (var subfolder in folder.Folders)
            {
                ExtractDirectory(subfolder, dir);
            }
            foreach (var file in folder.Files)
            {
                ExtractFile(file, dir);
            }
        }

        public byte[] ExtractFile(string path, int? limit = null)
        {
            var entry = GetFileEntry(path);
            return ExtractFile(entry, limit);
        }

        public void ExtractFile(FileEntry entry, string dir)
        {
            File.WriteAllBytes(Path.Combine(dir, entry.Name), ExtractFile(entry));
        }

        private byte[] ExtractFile(FileEntry entry, int? limit = null)
        {
            var fileSize = entry.FileSize;
            if (limit != null && limit.Value < entry.FileSize) fileSize = limit.Value;
            var output = new byte[fileSize];
            var outpos = 0;
            var block = entry.StartingBlockNum;
            var remaining = fileSize;
            do
            {
                if (block >= VolumeDescriptor.AllocatedBlockCount)
                    throw new InvalidOperationException("STFS: Reference to illegal block number.\n");

                var readBlock = remaining > 0x1000 ? 0x1000 : remaining;
                remaining -= readBlock;
                var pos = GetRealAddressOfBlock(block);
                Binary.ReadBytes(pos, output, outpos, readBlock);
                outpos += readBlock;

                var he = GetHashEntry(block);
                block = he.NextBlock;
            } while (remaining != 0);

            return output;
        }

        public MemoryStream OpenEntryStream(string path, int? limit = null)
        {
            return new MemoryStream(ExtractFile(path, limit));
        }

        public IEnumerable<KeyValuePair<int?, BlockStatus>> GetFileEntryBlockList(FileEntry entry)
        {
            var block = entry.StartingBlockNum;
            var blockList = new KeyValuePair<int?, BlockStatus>[entry.BlocksForFile];
            var i = 0;
            while (i < entry.BlocksForFile && block < VolumeDescriptor.AllocatedBlockCount)
            {
                var he = GetHashEntry(block);
                blockList[i] = new KeyValuePair<int?, BlockStatus>(block, he.Status);
                block = he.NextBlock;
                i++;
            }
            return blockList;
        }

        public byte[] ExtractBlock(int block)
        {
            var output = new byte[0x1000];
            var pos = GetRealAddressOfBlock(block);
            Binary.ReadBytes(pos, output, 0, 0x1000);
            return output;
        }

        #endregion

        #region Write

        public byte[] Save()
        {
            BeforeSaving();
            return Binary.ReadBytes(0, Binary.Length);
        }

        public void Save(string path)
        {
            BeforeSaving();
            Binary.Save(path);
        }

        private void BeforeSaving()
        {
            //HACK Horizon
            VolumeDescriptor.AllocatedBlockCount += 4;
            TopTable.Tables.Last().EntryCount += 4;
            Binary.EnsureBinarySize(GetRealAddressOfBlock(VolumeDescriptor.AllocatedBlockCount));

            Rehash();
            Resign();
        }

        private int[] AllocateBlocks(int count)
        {
            if (count <= 0) return new int[0];

            var freeCount = UnallocatedHashEntries.Count > count ? count : UnallocatedHashEntries.Count;
            var res = new List<int>();
            if (freeCount > 0)
            {
                res.AddRange(UnallocatedHashEntries.Take(freeCount));
                UnallocatedHashEntries.RemoveRange(0, freeCount);

                var lastTable = TopLevel == 0 ? TopTable : TopTable.Tables[TopTable.EntryCount - 1];
                var lastEntry = lastTable.Entries.FirstOrDefault(e => e.Block == res.Last());
                if (lastEntry != null)
                {
                    var index = lastTable.Entries.ToList().IndexOf(lastEntry);
                    lastTable.EntryCount = index + 1;
                    if (res.Last() + 1 > VolumeDescriptor.AllocatedBlockCount)
                    {
                        VolumeDescriptor.AllocatedBlockCount = res.Last() + 1;
                        Binary.EnsureBinarySize(GetRealAddressOfBlock(VolumeDescriptor.AllocatedBlockCount));
                    }
                }
            }
            var toAlloc = count - freeCount;
            if (toAlloc > 0)
            {
                switch (TopLevel)
                {
                    case 0:
                        throw new NotImplementedException("Table upgrade from lvl 0 to 1 needed!");
                    case 1:
                        if (TopTable.EntryCount == 0xAA)
                            throw new NotImplementedException("Table upgrade from lvl 1 to 2 needed!");

                        //allocate a new table
                        var table = GetLevelNHashTable(TopTable.EntryCount, 0);
                        var tableEntry = TopTable.Entries[TopTable.EntryCount++];
                        tableEntry.RealBlock = table.Block;
                        tableEntry.Status = BlockStatus.Allocated;
                        TopTable.Tables.Add(table);

                        res.AddRange(AllocateBlocks(toAlloc));
                        break;
                    default:
                        throw new NotSupportedException("Unsupported level" + TopLevel);
                }
            }
            return res.ToArray();
        }

        public void UnlockAchievement(string titleId, int achievementId, byte[] image)
        {
            var fileEntry = GetFileEntry(titleId + ".gpd");
            var game = Games[fileEntry];
            game.UnlockAchievement(achievementId, image);
            RebuildGame(game, fileEntry);
        }

        private void RebuildGame(GameFile game, FileEntry fileEntry = null, TitleEntry titleEntry = null)
        {
            Debug.WriteLine("{0} {1}", game.TitleId, game.Title);
            var name = game.TitleId + ".gpd";
            fileEntry = fileEntry ?? GetFileEntry(name, true);
            titleEntry = titleEntry ?? ProfileInfo.TitlesPlayed.FirstOrDefault(t => t.TitleCode == game.TitleId);
            if (titleEntry == null)
                throw new ArgumentException("Invalid title: " + game.TitleId);
            game.Rebuild();

            ////HACK: Horizon emu
            //RemoveFile(fileEntry);

            ReplaceFile(fileEntry, game);
            titleEntry.AchievementsUnlocked = game.UnlockedAchievementCount;
            titleEntry.GamerscoreUnlocked = game.Gamerscore;
        }

        public void MergeWith(StfsPackage otherProfile)
        {
            if (!IsModified) SwitchTables();

            var count = otherProfile.ProfileInfo.TitlesPlayed.Count;
            NotifyContentCountDetermined(count);

            var otherPec = otherProfile.ExtractPec();
            var pecFileEntry = GetFileEntry("PEC");
            var pec = ExtractPec();

            //HACK horizon
            int? newBlock = AllocateBlocks(1)[0];

            foreach (var title in otherProfile.ProfileInfo.TitlesPlayed)
            {
                var watch = new Stopwatch();
                watch.Start();
                var name = title.TitleCode + ".gpd";
                var otherGame = otherProfile.GetGameFile(name, true);
                var otherAvatarAwards = otherPec.GetGameFile(name, true);

                var fileEntry = GetFileEntry(name, true);
                if (fileEntry != null)
                {
                    var titleEntry = ProfileInfo.TitlesPlayed.FirstOrDefault(t => t.TitleCode == title.TitleCode);
                    if (titleEntry != null)
                    {
                        //Title already exists in target, merge is necessary
                        var game = GetGameFile(fileEntry, true);
                        if (game.MergeWith(otherGame))
                        {
                            RebuildGame(game);
                        }

                        if (otherAvatarAwards != null)
                        {
                            var avatarAwards = pec.GetGameFile(name, true);
                            if (avatarAwards != null)
                            {
                                if (avatarAwards.MergeWith(otherAvatarAwards))
                                {
                                    avatarAwards.Rebuild();
                                    var pecGpdEntry = pec.GetFileEntry(name);
                                    pec.ReplaceFile(pecGpdEntry, avatarAwards);
                                }
                            }
                            else
                            {
                                pec.AddFile(name, otherAvatarAwards.Binary.ReadAll());
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Title doesn't exists, but the gpd file does");
                        //Title doesn't exists, but the gpd file does, we just replace that

                        //var otherBinary = otherGame.Binary.ReadAll();
                        //ReplaceFile(fileEntry, otherBinary);
                        //var tid = title.TitleId.ToArray();
                        //Array.Reverse(tid);
                        //var id = BitConverter.ToUInt32(tid, 0);
                        //ProfileInfo.AddNewEntry<TitleEntry>(EntryType.Title, title.AllBytes, id);
                        //info = "added";
                    }
                }
                else
                {
                    if (otherAvatarAwards != null) pec.AddFile(name, otherAvatarAwards.Binary.ReadAll());

                    if (otherGame != null)
                    {
                        //Add gpd and title
                        var otherBinary = otherGame.Binary.ReadAll();

                        //File.WriteAllBytes(@"d:\NTX-CNT\Contour\Resources\mergeable\aktualis\q\" + i + "_" + name, otherBinary);

                        fileEntry = AddFile(name, otherBinary);
                        CreateGameFileModel(fileEntry, otherBinary, true);

                        var tid = title.TitleId.ToArray();
                        Array.Reverse(tid);
                        var id = BitConverter.ToUInt32(tid, 0);
                        ProfileInfo.AddNewEntry<TitleEntry>(EntryType.Title, title.AllBytes, id);
                    }
                }
                watch.Stop();
                //LogHelper.NotifyStatusBarChange(++i);
                //LogHelper.NotifyStatusBarText(string.Format("{0}/{1} {2} merged", i, count, title.TitleName));
            }

            //LogHelper.NotifyStatusBarText("Rebuilding profile...");
            foreach (var title in ProfileInfo.TitlesPlayed)
            {
                var name = title.TitleCode + ".gpd";
                var game = GetGameFile(name, true);
                title.LastAchievementEarnedOn = game.AchievementCount > 0 ? game.Achievements.Max(a => a.UnlockTime) : DateTime.MinValue;
            }

            //HACK: Horizon
            var previous = 0;
            var block = VolumeDescriptor.FileTableBlockNum;
            for (var k = 0; k < VolumeDescriptor.FileTableBlockCount - 1; k++)
            {
                var hek = GetHashEntry(block);
                previous = block;
                block = hek.NextBlock;
            }
            Binary.WriteBytes(GetRealAddressOfBlock(newBlock.Value), Binary.ReadBytes(GetRealAddressOfBlock(block), 0x1000), 0, 0x1000);
            var he = GetHashEntry(block);
            he.Status = BlockStatus.PreviouslyAllocated;
            he = GetHashEntry(newBlock.Value);
            he.Status = BlockStatus.NewlyAllocated;
            UnallocatedHashEntries.Add(block);
            GetHashEntry(previous).NextBlock = newBlock.Value;

            ProfileInfo.Recalculate();
            ProfileInfo.Rebuild();
            ReplaceFile(ProfileEntry, ProfileInfo);

            pec.Rehash();
            pec.Resign();
            ReplaceFile(pecFileEntry, pec.Binary.ReadAll());
        }

        public void ReplaceFile(FileEntry fileEntry, GpdFile gpd)
        {
            ReplaceFile(fileEntry, gpd.Binary.ReadAll());
        }

        public void ReplaceFile(FileEntry fileEntry, byte[] content)
        {
            if (fileEntry.IsDirectory)
                throw new NotSupportedException("Directories doesn't have content!");

            var remaining = content.Length;

            var allocatedBlockCount = fileEntry.BlocksForFile;
            var newBlockCount = (content.Length + 0xFFF) / 0x1000;
            var blocks = AllocateBlocks(newBlockCount - allocatedBlockCount);
            var block = fileEntry.StartingBlockNum;
            if (allocatedBlockCount == 0)
            {
                block = blocks[0];
                fileEntry.StartingBlockNum = blocks[0];
            }
            var consecutive = true;

            for (var i = 0; i < newBlockCount; i++)
            {
                var pos = GetRealAddressOfBlock(block);
                var size = remaining > 0x1000 ? 0x1000 : remaining;
                remaining -= size;
                var buffer = new byte[0x1000];
                Buffer.BlockCopy(content, i * 0x1000, buffer, 0, size);
                Binary.WriteBytes(pos, buffer, 0, 0x1000);

                var he = GetHashEntry(block);
                if (i < allocatedBlockCount - 1)
                {
                    he.Status = BlockStatus.Allocated;
                    if (he.NextBlock != block + 1) consecutive = false;
                    block = he.NextBlock;
                }
                else if (i < newBlockCount - 1)
                {
                    he.Status = BlockStatus.NewlyAllocated;
                    block = blocks[i - allocatedBlockCount + 1];
                    he.NextBlock = block;
                    if (he.NextBlock != he.Block + 1) consecutive = false;
                }
                else
                {
                    he.Status = BlockStatus.NewlyAllocated;
                    he.NextBlock = 0xFFFFFF;
                }
            }

            for (var i = newBlockCount; i < allocatedBlockCount; i++)
            {
                var he = GetHashEntry(block);
                if (UnallocatedHashEntries.Contains(block))
                    throw new Exception("qwe");
                UnallocatedHashEntries.Add(block);
                he.Status = BlockStatus.PreviouslyAllocated;
                block = he.NextBlock;
                he.NextBlock = 0xFFFFFF;
            }

            fileEntry.FileSize = content.Length;
            fileEntry.BlocksForFile = newBlockCount;
            fileEntry.BlocksForFileCopy = newBlockCount;
            fileEntry.BlocksAreConsecutive = consecutive;
        }

        public FileEntry AddFile(string path, byte[] content)
        {
            var name = Path.GetFileName(path);
            var parent = GetFolderEntry(path.Replace(name, string.Empty));
            return AddFile(parent, name, content);
        }

        public FileEntry AddFile(FileEntry parent, string name, byte[] content)
        {
            var newEntry = AllocateNewFileEntry();
            newEntry.CacheEnabled = false;

            newEntry.Name = name;
            newEntry.Flags = (FileEntryFlags)name.Length;
            newEntry.PathIndicator = (ushort)parent.EntryIndex;
            newEntry.BlocksForFile = 0;
            ReplaceFile(newEntry, content);
            parent.Files.Add(newEntry);

            return newEntry;
        }

        private FileEntry AllocateNewFileEntry()
        {
            HashEntry he = null;
            var block = VolumeDescriptor.FileTableBlockNum;
            for (var x = 0; x < VolumeDescriptor.FileTableBlockCount; x++)
            {
                var currentAddr = GetRealAddressOfBlock(block);
                for (var i = 0; i < 64; i++)
                {
                    var addr = currentAddr + i * 0x40;
                    var fe = ModelFactory.GetModel<FileEntry>(Binary, addr);

                    if (fe.Name != String.Empty) continue;

                    var date = DateTime.Now.ToFatFileTime();
                    fe.FileEntryAddress = addr;
                    fe.EntryIndex = (x * 0x40) + i;
                    fe.CreatedTimeStamp = date;
                    fe.AccessTimeStamp = date;
                    return fe;
                }
                he = GetHashEntry(block);
                block = he.NextBlock;
            }

            var blocks = AllocateBlocks(1);
            block = blocks[0];
            he.NextBlock = block;

            var blockAddr = GetRealAddressOfBlock(block);
            var fileEntry = CreateNewFileEntry(blockAddr, 0, VolumeDescriptor.FileTableBlockCount);
            VolumeDescriptor.FileTableBlockCount++;
            return fileEntry;
        }

        private FileEntry CreateNewFileEntry(int currentAddr, int index, int tableNum)
        {
            var addr = currentAddr + index * 0x40;
            var fe = ModelFactory.GetModel<FileEntry>(Binary, addr);

            if (fe.Name != String.Empty) return null;

            fe.FileEntryAddress = addr;
            fe.EntryIndex = (tableNum * 0x40) + index;
            fe.CreatedTimeStamp = DateTime.Now.ToFatFileTime();
            fe.AccessTimeStamp = DateTime.Now.ToFatFileTime();
            return fe;
        }

        public void RemoveFile(string path)
        {
            var name = Path.GetFileName(path);
            var parent = GetFolderEntry(path.Replace(name, string.Empty));
            var fileEntry = parent.Files.First(f => f.Name == name);
            RemoveFile(parent, fileEntry);
        }

        private void RemoveFile(FileEntry parent, FileEntry fileEntry)
        {
            var block = fileEntry.StartingBlockNum;
            for (var i = 0; i < fileEntry.BlocksForFile; i++)
            {
                var he = GetHashEntry(block);
                he.Status = BlockStatus.PreviouslyAllocated;
                if (UnallocatedHashEntries.Contains(block))
                    throw new Exception("qwe");
                UnallocatedHashEntries.Add(block);
                block = he.NextBlock;
            }
            fileEntry.BlocksForFile = 0;
            fileEntry.Name = string.Empty;
            parent.Files.Remove(fileEntry);
        }

        public FileEntry AddFolder(string path)
        {
            var name = Path.GetFileName(path);
            var parent = GetFolderEntry(path.Replace(name, string.Empty));
            return AddFolder(parent, name);
        }

        public FileEntry AddFolder(FileEntry parent, string name)
        {
            var newEntry = AllocateNewFileEntry();
            newEntry.CacheEnabled = false;

            newEntry.Name = name;
            newEntry.Flags = FileEntryFlags.IsDirectory;
            newEntry.PathIndicator = (ushort)parent.EntryIndex;
            newEntry.BlocksForFile = 0;
            parent.Folders.Add(newEntry);
            return newEntry;
        }

        public void RemoveFolder(string path)
        {
            var name = Path.GetFileName(path.TrimEnd('/','\\'));
            var parent = GetFolderEntry(path.Replace(name, string.Empty));
            var folderEntry = parent.Files.First(f => f.Name == name);
            RemoveFolder(parent, folderEntry);
        }

        public void RemoveFolder(FileEntry parent, FileEntry folder)
        {
            folder.Folders.ForEach(f => RemoveFolder(folder, f));
            folder.Files.ForEach(f => RemoveFile(folder, f));
            folder.Name = string.Empty;
            parent.Folders.Remove(folder);
        }

        public FileEntry Rename(string path, string newName)
        {
            var entry = GetFolderEntry(path, true) ?? GetFileEntry(path);
            entry.Name = newName;
            return entry;
        }

        #endregion

        #region Security

        public override void Rehash()
        {
            int unallocCount = 0;
            switch (TopLevel)
            {
                case 0:
                    for (var i = 0; i < TopTable.EntryCount; i++)
                    {
                        var pos = GetRealAddressOfBlock(i);
                        var entry = TopTable.Entries[i];
                        if (entry.Status == BlockStatus.Unallocated || entry.Status == BlockStatus.PreviouslyAllocated)
                            unallocCount++;
                        else
                            entry.BlockHash = HashBlock(pos);
                    }
                    break;
                case 1:
                    for (var i = 0; i < TopTable.EntryCount; i++)
                    {
                        for (var j = 0; j < TopTable.Tables[i].EntryCount; j++)
                        {
                            var lowEntry = TopTable.Tables[i].Entries[j];
                            var pos = GetRealAddressOfBlock(lowEntry.Block.Value);
                            if (lowEntry.Status == BlockStatus.Unallocated || lowEntry.Status == BlockStatus.PreviouslyAllocated)
                                unallocCount++;
                            else
                                lowEntry.BlockHash = HashBlock(pos);
                        }
                        var tableEntries = TopTable.Tables[i].Entries.Take(TopTable.Tables[i].EntryCount).ToArray();
                        var u = tableEntries.Count(e => e.Status == BlockStatus.Unallocated);
                        var p = tableEntries.Count(e => e.Status == BlockStatus.PreviouslyAllocated);
                        var highEntry = TopTable.Entries[i];
                        highEntry.BlockHash = HashBlock(TopTable.Tables[i].StartOffset);
                        highEntry.NextBlock = (p << 15) + u;
                    }
                    break;
                default:
                    throw new NotSupportedException("Not supported level: " + TopLevel);
            }

            TopTable.AllocatedBlockCount = VolumeDescriptor.AllocatedBlockCount;
            VolumeDescriptor.UnallocatedBlockCount = unallocCount;
            VolumeDescriptor.TopHashTableHash = HashBlock(TopTable.StartOffset);

            const int headerStart = 0x344;

            // calculate header size / first hash table address
            var calculated = ((HeaderSize + 0xFFF) & 0xF000);
            var headerSize = calculated - headerStart;

            HeaderHash = HashBlock(headerStart, headerSize);
        }

        #endregion

    }
}