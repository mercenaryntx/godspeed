using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Helpers;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Gpd.Entries;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Core.Tests
{
    [TestFixture]
    public class StfsTests
    {
        //private byte[] _file;
        //private StfsPackage _package;

        private const string OrigPath = @"c:\merc\Contour\Resources\hashcheck\E00001D5D85ED487.orig";
        private const string VelocityPath = @"..\..\..\..\Resources\hashcheck\E00001D5D85ED487.velocity";
        private const string HorizonPath = @"..\..\..\..\Resources\hashcheck\E00001D5D85ED487.horizon";

        //[SetUp]
        //public void SetUp()
        //{
        //    _file = File.ReadAllBytes(OrigPath);
        //    _package = ModelFactory.GetModel<StfsPackage>(_file);
        //}

        [Test]
        public void ReplaceFileWithBiggerGpd()
        {
            var file = File.ReadAllBytes(OrigPath);
            var package = ModelFactory.GetModel<StfsPackage>(file);

            var name = "55530879.gpd";
            var key = package.GetFileEntry("55530879.gpd"); //139333
            var newContent = File.ReadAllBytes(@"..\..\..\..\Resources\xbox_profile\extract\Root\5553080B.gpd");
                //154792

            package.ReplaceFile(key, newContent);

            var extracted = package.ExtractFile(name);
            Assert.AreEqual(newContent, extracted, "Content doesn't match");
        }

        [Test]
        public void AddNewGpdFile()
        {
            var file = File.ReadAllBytes(OrigPath);
            var package = ModelFactory.GetModel<StfsPackage>(file);

            var name = "_newfile.gpd";
            var newContent = File.ReadAllBytes(@"..\..\..\..\Resources\xbox_profile\extract\Root\5553080B.gpd");
                //154792

            package.AddFile(name, newContent);

            var extracted = package.ExtractFile(name);
            Assert.AreEqual(newContent, extracted, "Content doesn't match");
        }

        [Test]
        public void HashEntryTest()
        {
            var model = ModelFactory.GetModel<HashEntry>();
            model.BlockHash = new byte[] {1, 2, 3, 4, 5};
            model.NextBlock = 5000;
            model.Status = BlockStatus.Allocated;

            Assert.AreEqual(0x14, model.BlockHash.Length, "BlockHash length mismatch");
            Assert.AreEqual(new byte[] {1, 2, 3, 4, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, model.BlockHash,
                            "BlockHash value mismatch");
            Assert.AreEqual(5000, model.NextBlock, "NextBlock value mismatch");
            Assert.AreEqual(BlockStatus.Allocated, model.Status, "Status value mismatch");
        }

        [Test]
        public void AchievementEntryTest()
        {
            var model = ModelFactory.GetModel<AchievementEntry>();
            model.Size = 128;
            model.AchievementId = 1;
            model.ImageId = 2;
            model.Gamerscore = 65535;
            model.Flags = AchievementLockFlags.Unlocked;
            model.UnlockTime = new DateTime(2012, 12, 12);
            model.Name = "Lorem ipsum";
            model.UnlockedDescription = "dolor sit amet";
            model.LockedDescription = "secret";

            Assert.AreEqual(128, model.Size, "Size mismatch");
            Assert.AreEqual(1, model.AchievementId, "Achievement ID mismatch");
            Assert.AreEqual(2, model.ImageId, "Image ID mismatch");
            Assert.AreEqual(65535, model.Gamerscore, "Gamerscore mismatch");
            Assert.AreEqual(AchievementLockFlags.Unlocked, model.Flags, "Flag mismatch");
            Assert.AreEqual("2012-12-12", model.UnlockTime.ToString("yyyy-MM-dd"), "Unlock time mismatch");
            Assert.AreEqual("Lorem ipsum", model.Name, "Name mismatch");
            Assert.AreEqual("dolor sit amet", model.UnlockedDescription, "Unlocked description mismatch");
            Assert.AreEqual("secret", model.LockedDescription, "Locked description mismatch");
        }

        [Test]
        public void IntactnessTest()
        {
            var file = File.ReadAllBytes(OrigPath);
            var package = ModelFactory.GetModel<StfsPackage>(file);

            package.Save(OrigPath + ".tmp");
            var file1 = File.ReadAllBytes(OrigPath);
            var file2 = File.ReadAllBytes(OrigPath + ".tmp");

            Assert.AreEqual(file1, file2, "File content has been compromised");
        }

        [Test]
        public void ProfileEmbeddedContentTest1()
        {
            var pec = ModelFactory.GetModel<ProfileEmbeddedContent>();
            pec.VolumeDescriptor.Size = 0;
        }

        [Test]
        public void ProfileEmbeddedContentTest2()
        {
            var content = File.ReadAllBytes(@"..\..\..\..\Resources\mergeable\ext_orig\Root\PEC");
            var pec = ModelFactory.GetModel<ProfileEmbeddedContent>(content);
            Assert.AreEqual(0x1000, pec.HeaderSize);
            File.WriteAllLines(@"..\..\..\..\Resources\mergeable\PEC.map", pec.BinMap.Output());
        }

        [Test]
        public void ProfileEmbeddedContentRehashTest()
        {
            var content = File.ReadAllBytes(@"..\..\..\..\Resources\mergeable\ext_orig\Root\PEC");
            var pec1 = ModelFactory.GetModel<ProfileEmbeddedContent>(content);
            pec1.CacheEnabled = false;
            var pec2 = ModelFactory.GetModel<ProfileEmbeddedContent>(content);
            pec2.CacheEnabled = false;
            pec2.Rehash();
            pec2.Resign();
            var modifiedContent = pec2.Binary.ReadAll();
            var pec3 = ModelFactory.GetModel<ProfileEmbeddedContent>(modifiedContent);
            pec3.CacheEnabled = false;
            Assert.AreEqual(content, modifiedContent);
            Assert.AreEqual(pec1.VolumeDescriptor.TopHashTableHash, pec3.VolumeDescriptor.TopHashTableHash);
            Assert.AreEqual(pec1.Certificate.Binary.ReadAll(), pec3.Certificate.Binary.ReadAll());
        }

        [Test]
        public void StfsRehashTest()
        {
            var file = File.ReadAllBytes(OrigPath);
            var package = ModelFactory.GetModel<StfsPackage>(file);

            var save = @"..\..\..\..\Resources\mergeable\merge.resigned";
            package.Save(save);
            var modified = File.ReadAllBytes(save);
            var p3 = ModelFactory.GetModel<StfsPackage>(modified);
            //Assert.AreEqual(_package.VolumeDescriptor.TopHashTableHash, p3.VolumeDescriptor.TopHashTableHash);
            //Assert.AreEqual(_package.Certificate.OwnerConsoleId, p3.Certificate.OwnerConsoleId);
        }

        [Test]
        public void PerformanceTestInt32()
        {
            var file = File.ReadAllBytes(OrigPath);
            var package = ModelFactory.GetModel<StfsPackage>(file);

            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < 1000; i++)
            {
                //var x = _package.VolumeDescriptor.AllocatedBlockCount;
                var x = package.Magic;
            }
            watch.Stop();
            Debug.WriteLine(watch.Elapsed);
            package.CacheEnabled = true;
            watch.Reset();
            watch.Start();
            for (var i = 0; i < 1000; i++)
            {
                //var x = _package.VolumeDescriptor.AllocatedBlockCount;
                var x = package.Magic;
            }
            watch.Stop();
            Debug.WriteLine(watch.Elapsed);

        }

        //[Test]
        //public void GpdRebuildTest()
        //{
        //    var file = File.ReadAllBytes(OrigPath);
        //    var package = ModelFactory.GetModel<StfsPackage>(file);


        //    var headerHashBefore = package.HeaderHash.ToHex();
        //    var blockHashesBefore = new string[package.HashEntries.Count];
        //    for (var i = 0; i < package.HashEntries.Count; i++)
        //        blockHashesBefore[i] = package.HashEntries[i].BlockHash.ToHex();

        //    Debug.WriteLine(headerHashBefore, "Original");

        //    package.Rehash();
        //    package.Resign();

        //    var headerHashAfter = package.HeaderHash.ToHex();
        //    var blockHashesAfter = new string[package.HashEntries.Count];
        //    for (var i = 0; i < package.HashEntries.Count; i++)
        //        blockHashesAfter[i] = package.HashEntries[i].BlockHash.ToHex();

        //    Debug.WriteLine(headerHashAfter, "Resigned");

        //    var velocity = ModelFactory.GetModel<StfsPackage>(File.ReadAllBytes(VelocityPath));

        //    var headerHashVelocity = velocity.HeaderHash.ToHex();
        //    var blockHashesVelocity = new string[velocity.HashEntries.Count];
        //    for (var i = 0; i < velocity.HashEntries.Count; i++)
        //        blockHashesVelocity[i] = velocity.HashEntries[i].BlockHash.ToHex();

        //    Debug.WriteLine(headerHashVelocity, "Velocity");

        //    var horizon = ModelFactory.GetModel<StfsPackage>(File.ReadAllBytes(HorizonPath));

        //    var headerHashHorizon = horizon.HeaderHash.ToHex();
        //    var blockHashesHorizon = new string[horizon.HashEntries.Count];
        //    for (var i = 0; i < horizon.HashEntries.Count; i++)
        //        blockHashesHorizon[i] = horizon.HashEntries[i].BlockHash.ToHex();

        //    Debug.WriteLine(headerHashHorizon, "Horizon");

        //    //Assert.AreEqual(headerHashBefore, headerHashAfter);
        //    //Assert.AreEqual(blockHashesBefore, headerHashAfter);

        //    //Assert.AreEqual(headerHashVelocity, headerHashAfter);
        //    //Assert.AreEqual(blockHashesVelocity, headerHashAfter);

        //    //Assert.AreEqual(headerHashVelocity, headerHashBefore);
        //    //Assert.AreEqual(blockHashesVelocity, headerHashBefore);
        //}

        [Test]
        public void RehashTest()
        {
            //var file = File.ReadAllBytes(OrigPath);
            //var package = ModelFactory.GetModel<StfsPackage>(file);

            //for (var i = 0; i < package.HashEntries.Count; i++)
            //{
            //    var pos = package.BlockToAddress(i);
            //    var buffer = package.Binary.ReadBytes(pos, 0x1000);
            //    var orig = package.HashEntries[i].BlockHash;
            //    var hash = StfsPackage.HashBlock(buffer);
            //    if (package.HashEntries[i].Status != BlockStatus.Unallocated)
            //    {
            //        var h1 = orig.ToHex();
            //        var h2 = hash.ToHex();
            //        Debug.WriteLine("[{0,4}] {1} {2}", i, h1, h2);
            //        Assert.AreEqual(h1, h2);
            //    }
            //}
        }

        [Test]
        public void ResignTest()
        {
            var file = File.ReadAllBytes(OrigPath);
            var package = ModelFactory.GetModel<StfsPackage>(file);

            var resigned = ModelFactory.GetModel<StfsPackage>(File.ReadAllBytes(OrigPath));
            resigned.Resign();

            //_package.Save(OutPath);

            //var velocity = ModelFactory.GetModel<StfsPackage>(File.ReadAllBytes(VelocityPath));
            //BinaryAssert.Assert(velocity.Certificate, _package.Certificate);

            //var fluffie = ModelFactory.GetModel<StfsPackage>(File.ReadAllBytes(@"..\..\..\..\Resources\hashcheck\out"));
            //var mysigned = ModelFactory.GetModel<StfsPackage>(File.ReadAllBytes(@"..\..\..\..\Resources\hashcheck\out1"));

            BinaryAssert.Assert(package.Certificate, resigned.Certificate);
        }

        //[Test]
        //public void ResignTest2()
        //{
        //    var desired = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\newmergeapproach\E00001D5D85ED487.0114");
        //    var merged = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\newmergeapproach\0112_0114_merge.orig");
        //    //merged.Rehash();

        //    //merged.GetUnallocatedBlocks(5);

        //    //merged.VolumeDescriptor.AllocatedBlockCount = 6167;
        //    //merged.VolumeDescriptor.FileTableBlockNum = 6110;
        //    //merged.VolumeDescriptor.UnallocatedBlockCount = 15;
        //    merged.Save(@"..\..\..\..\Resources\newmergeapproach\0112_0114_merge.out");

        //    for (var i = 0; i < desired.TopTable.EntryCount; i++)
        //    {
        //        var d = desired.HashEntries[i].BlockHash.ToHex();
        //        var m = merged.HashEntries[i].BlockHash.ToHex();
        //        if (d != m)
        //        {
        //            Debug.WriteLine("[{0}][{1}][{2}]", i, d, m);
        //        }
        //    }

        //    //Assert.AreEqual(desired.VolumeDescriptor.AllocatedBlockCount, merged.VolumeDescriptor.AllocatedBlockCount);

        //    //BinaryAssert.Assert(desired.Certificate, merged.Certificate);
        //}

        [Test]
        public void ResignAHorizonResignedProfile()
        {
            var merged = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\mergeable\aktualis\merge.2x");
            merged.Resign();
            File.WriteAllBytes(@"..\..\..\..\Resources\mergeable\aktualis\merge.3x", merged.Binary.ReadAll());
        }

        [Test]
        public void ExtractedGpdTest()
        {
            //var gpd = File.ReadAllBytes(@"..\..\..\..\Resources\mergeable\ext\Root\4D5308ED.gpd");
            var gpd = File.ReadAllBytes(@"..\..\..\..\Resources\mergeable\ext\Root\4D5308ED.orig");
            var game = ModelFactory.GetModel<GameFile>(gpd);
            game.Parse();
        }

        [Test]
        public void HashEntriesTest()
        {
            var p = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\mergeable\aktualis\E00001D5D85ED487.orig");
            for (var i = 0; i < p.VolumeDescriptor.AllocatedBlockCount; i++)
            {
                Debug.WriteLine("{0,4} -> {1,4}", i, p.TopTable.Tables[i/0xAA].Entries[i%0xAA].NextBlock);
            }
        }

        [Test]
        public void AllocatingNewTableTest()
        {
            var p = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\mergeable\aktualis\E00001D5D85ED487.orig");
            var table = p.GetLevelNHashTable(p.TopTable.EntryCount, 0);
            var tableEntry = p.TopTable.Entries[p.TopTable.EntryCount++];
            tableEntry.RealBlock = table.Block;
            tableEntry.Status = BlockStatus.Allocated;
            p.TopTable.Tables.Add(table);
            p.Save(@"..\..\..\..\Resources\mergeable\aktualis\merge.1x");
        }

        [Test]
        public void WrongAllocationTest()
        {
            var no = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\compare\deathspank.no");
            var wr = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\compare\deathspank.wr");
            var ok = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\compare\deathspank.ok");

            //var w = wr.FileStructure.Files.First(f => f.Name == "584109D1.gpd");
            //var o = ok.FileStructure.Files.First(f => f.Name == "584109D1.gpd");
            var w = wr.FileStructure.Files.First(f => f.Name == "58410A6F.gpd");
            var o = ok.FileStructure.Files.First(f => f.Name == "58410A6F.gpd");

            Debug.WriteLine("WR: ");
            var block = w.StartingBlockNum;
            for (var i = 0; i < w.BlocksForFile; i++)
            {
                var he = wr.GetHashEntry(block);
                Debug.WriteLine("{0,4}\t{1}", block, he.BlockHash.ToHex());
                block = he.NextBlock;
            }

            Debug.WriteLine("OK: ");
            block = o.StartingBlockNum;
            for (var i = 0; i < o.BlocksForFile; i++)
            {
                var he = ok.GetHashEntry(block);
                Debug.WriteLine("{0,4}\t{1}", block, he.BlockHash.ToHex());
                block = he.NextBlock;
            }

            Debug.WriteLine("Allo: {0} {1} {2}",
                            no.VolumeDescriptor.AllocatedBlockCount,
                            wr.VolumeDescriptor.AllocatedBlockCount,
                            ok.VolumeDescriptor.AllocatedBlockCount);
            Debug.WriteLine("Unal: {0} {1} {2}",
                            no.VolumeDescriptor.UnallocatedBlockCount,
                            wr.VolumeDescriptor.UnallocatedBlockCount,
                            ok.VolumeDescriptor.UnallocatedBlockCount);

            for (var i = 0; i < no.TopTable.EntryCount; i++)
            {
                for (var j = 0; j < no.TopTable.Tables[i].EntryCount; j++)
                {
                    var sno = no.TopTable.Tables[i].Entries[j].Status;
                    var swr = wr.TopTable.Tables[i].Entries[j].Status;
                    var sok = ok.TopTable.Tables[i].Entries[j].Status;
                    if (sno != swr || swr != sok || sno != sok)
                        Debug.WriteLine("{0}: {1} {2} {3}", no.TopTable.Tables[i].Entries[j].Block, sno, swr, sok);
                }
            }

            for (var i = 0; i < no.TopTable.EntryCount; i++)
            {
                for (var j = 0; j < no.TopTable.Tables[i].EntryCount; j++)
                {
                    var sno = no.TopTable.Tables[i].Entries[j].BlockHash.ToHex();
                    var swr = wr.TopTable.Tables[i].Entries[j].BlockHash.ToHex();
                    var sok = ok.TopTable.Tables[i].Entries[j].BlockHash.ToHex();
                    if (sno != swr || swr != sok || sno != sok)
                    {
                        var b = no.TopTable.Tables[i].Entries[j].Block.Value;
                        Debug.WriteLine("{0}: {1} {2} {3}", b, FindFile(no, b), FindFile(wr, b), FindFile(ok, b));
                    }
                }
            }

            Debug.WriteLine("BSep: {0} {1} {2}",
                            no.VolumeDescriptor.BlockSeparation,
                            wr.VolumeDescriptor.BlockSeparation,
                            ok.VolumeDescriptor.BlockSeparation);

            for (var i = 0; i < no.TopTable.EntryCount; i++)
            {
                var sno = no.TopTable.Entries[i].Status;
                var swr = wr.TopTable.Entries[i].Status;
                var sok = ok.TopTable.Entries[i].Status;
                if (sno != swr || swr != sok || sno != sok)
                    Debug.WriteLine("{0}: {1} {2} {3}", no.TopTable.Entries[i].Block, sno, swr, sok);
            }

        }

        private string FindFile(StfsPackage p, int b)
        {
            var hes = p.TopTable.Tables.SelectMany(t => t.Entries).ToArray();
            var bb = b;
            HashEntry he;
            do
            {
                he =
                    hes.FirstOrDefault(
                        h =>
                        h.NextBlock == bb &&
                        (h.Status == BlockStatus.Allocated || h.Status == BlockStatus.NewlyAllocated));
                if (he != null) bb = he.Block.Value;
            } while (he != null);
            var fno = p.FileStructure.Files.FirstOrDefault(f => f.StartingBlockNum == bb);
            if (fno != null) return fno.Name;

            if (p.VolumeDescriptor.FileTableBlockNum == bb) return "<FileTable>";

            return "-";
        }

        private void MapPackage(StfsPackage p)
        {
            foreach (var f in p.FileStructure.Files)
            {
                var block = f.StartingBlockNum;
                for (var i = 0; i < f.BlocksForFile; i++)
                {
                    var adr = p.GetRealAddressOfBlock(block);
                    //if (f.Name == "58410A6F.gpd")
                    //{
                    //    Debug.WriteLine("{0,2} {1,4}", i, block);
                    //}
                    p.BinMap.Add(adr, 0x1000, f.Name, i.ToString(CultureInfo.InvariantCulture), block);
                    var he = p.GetHashEntry(block);
                    block = he.NextBlock;
                }
            }
            foreach (var f in p.FileStructure.Folders[0].Files)
            {
                var block = f.StartingBlockNum;
                for (var i = 0; i < f.BlocksForFile; i++)
                {
                    var adr = p.GetRealAddressOfBlock(block);
                    p.BinMap.Add(adr, 0x1000, f.Name, i.ToString(CultureInfo.InvariantCulture), block);
                    var he = p.GetHashEntry(block);
                    block = he.NextBlock;
                }
            }

            {
                var block = p.VolumeDescriptor.FileTableBlockNum;
                for (var i = 0; i < p.VolumeDescriptor.FileTableBlockCount; i++)
                {
                    var adr = p.GetRealAddressOfBlock(block);
                    p.BinMap.Add(adr, 0x1000, "FileTable", i.ToString(CultureInfo.InvariantCulture), block);
                    var he = p.GetHashEntry(block);
                    block = he.NextBlock;
                }
            }

            p.BinMap.Add(p.TopTable.StartOffset, 0x1000, "TopTable", String.Empty);
            for (var i = 0; i < p.TopTable.EntryCount; i++)
            {
                p.BinMap.Add(p.TopTable.Tables[i].StartOffset, 0x1000, "Table", i.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Test]
        public void TopTableTest()
        {
            var wr = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\compare\full.wr");
            for (var i = 0; i < wr.TopTable.EntryCount; i++)
            {
                Debug.WriteLine("{0} {1}", i, wr.TopTable.Entries[i].NextBlock);
            }
        }

        [Test]
        public void BinaryCompare()
        {
            var wr = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\mergeable\aktualis\merge.1x");
            MapPackage(wr);
            var ok = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\variations2\8");
            MapPackage(ok);

            Debug.WriteLine("{0} {1}", wr.VolumeDescriptor.AllocatedBlockCount, ok.VolumeDescriptor.AllocatedBlockCount);

            //for (var i = 0; i < wr.FileStructure.Files.Count; i++)
            //{
            //    if (wr.FileStructure.Files[i].Flags != ok.FileStructure.Files[i].Flags)
            //    {
            //        Debug.WriteLine("{0} {1}", wr.FileStructure.Files[i].BlocksAreConsecutive, ok.FileStructure.Files[i].BlocksAreConsecutive);
            //    }
            //}

            //for (var i = 0; i < wr.FileStructure.Files.Count; i++)
            //{
            //    wr.FileStructure.Files[i].CreatedTimeStamp = ok.FileStructure.Files[i].CreatedTimeStamp;
            //    wr.FileStructure.Files[i].AccessTimeStamp = ok.FileStructure.Files[i].AccessTimeStamp;
            //}

            //wr.GetHashEntry(6224).BlockHash = wr.HashBlock(wr.GetRealAddressOfBlock(6224));
            //wr.Binary.WriteBytes(wr.TopTable.StartOffset + 4083, ok.Binary.ReadBytes(ok.TopTable.StartOffset + 4083, 0x1), 0, 0x1);

            //wr.TopTable.Entries[0].BlockHash = wr.HashBlock(wr.TopTable.Tables[0].StartOffset);
            //wr.VolumeDescriptor.TopHashTableHash = wr.HashBlock(wr.TopTable.StartOffset);

            //wr.Binary.WriteBytes(4, ok.Binary.ReadBytes(4, ok.Certificate.OffsetTableSize), 0, ok.Certificate.OffsetTableSize);
            //wr.Binary.WriteBytes(0x14F7240, ok.Binary.ReadBytes(0x14F7240, 64), 0, 64);
            //wr.HeaderHash = ok.HeaderHash;
            //wr.ThumbnailImage = ok.ThumbnailImage;

            File.WriteAllBytes(@"..\..\..\..\Resources\mergeable\aktualis\merge.1xx", wr.Binary.ReadAll());
            //return;

            //wr.Save(@"..\..\..\..\Resources\compare\deathspank.wr2");

            //var w = wr.FileStructure.Files.First(f => f.Name == "58410A6F.gpd");
            //var o = ok.FileStructure.Files.First(f => f.Name == "58410A6F.gpd");

            //Debug.WriteLine("WR: ");
            //var block = w.StartingBlockNum;
            //for (var i = 0; i < w.BlocksForFile; i++)
            //{
            //    var he = wr.GetHashEntry(block);
            //    Debug.WriteLine("{0,4}\t{1}", block, he.BlockHash.ToHex());
            //    block = he.NextBlock;
            //}

            //Debug.WriteLine("OK: ");
            //block = o.StartingBlockNum;
            //for (var i = 0; i < o.BlocksForFile; i++)
            //{
            //    var he = ok.GetHashEntry(block);
            //    Debug.WriteLine("{0,4}\t{1}", block, he.BlockHash.ToHex());
            //    block = he.NextBlock;
            //}

            BinMapHelper.ModelCompare(wr, ok);

            //Debug.WriteLine("{0} {1}", wr.TopTable.Entries[13].BlockHash.ToHex(), ok.TopTable.Entries[13].BlockHash.ToHex());
            //var wrTable = wr.TopTable.Tables[13];
            ////var wrData = wr.Binary.ReadBytes(wrTable.StartOffset, 0x1000);

            ////Debug.WriteLine(wrData.ToHex());

            //var okTable = ok.TopTable.Tables[13];
            ////var okData = ok.Binary.ReadBytes(okTable.StartOffset, 0x1000);

            //for (var j = 0; j < wrTable.EntryCount; j++)
            //{
            //    var entry = wrTable.Entries[j];
            //    if (entry.Status == BlockStatus.NewlyAllocated) entry.Status = BlockStatus.Allocated;
            //}

            ////Assert.AreEqual(wrData, okData);
            //Debug.WriteLine("{0} {1}", wr.HashBlock(wrTable.StartOffset).ToHex(), ok.HashBlock(okTable.StartOffset).ToHex());

            //Debug.WriteLine("WR: {0}%", (double)wrCoverage / wr.Binary.Length * 100);
        }

        [Test]
        public void SwitchTableTest()
        {
            var ok = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\compare\deathspank.ok");
            var currentTopTable = ok.GetLevelNHashTable(0, 1);
            ok.SwitchTables();

            for (var i = 0; i < currentTopTable.EntryCount; i++)
            {
                ok.TopTable.Entries[i].CacheEnabled = false;
                BinaryAssert.Assert(currentTopTable.Entries[i], ok.TopTable.Entries[i]);
            }
        }

        [Test]
        public void TopTableEntries()
        {
            var l = new Dictionary<string, List<string>>();
            var g = new Dictionary<string, string>();
            foreach (var file in Directory.GetFiles(@"..\..\..\..\Resources\variations"))
            {
                if (Path.HasExtension(file)) continue;
                var m = ModelFactory.GetModel<StfsPackage>(file);
                var log = new List<string>();
                for(var i = 0; i < m.TopTable.EntryCount; i++)
                {
                    var un = m.TopTable.Tables[i].Entries.Take(m.TopTable.Tables[i].EntryCount).Count(e => e.Status == BlockStatus.Unallocated);
                    var pr = m.TopTable.Tables[i].Entries.Take(m.TopTable.Tables[i].EntryCount).Count(e => e.Status == BlockStatus.PreviouslyAllocated);
                    var xx = m.TopTable.Entries[i].NextBlock & 0x7FFF;
                    var yy = m.TopTable.Entries[i].NextBlock >> 15;
                    log.Add(string.Format("{0} ({1}), {2} ({3})", xx, un, yy, pr));
                }
                l.Add(Path.GetFileName(file), log);
                //g.Add(Path.GetFileName(file), m.VolumeDescriptor.BlockSeparation.ToString(CultureInfo.InvariantCulture));
                g.Add(Path.GetFileName(file), m.TopTable.Tables.Last().EntryCount.ToString());
            }
            var output = new List<string>
                             {
                                 "ID\t" + string.Join("\t", l.Keys.ToArray()),
                                 "\t" + string.Join("\t", g.Values.Select(s => s))
                             };
            var max = l.Values.Max(v => v.Count);
            for (var i = 0; i < max; i++)
            {
                var x = i.ToString(CultureInfo.InvariantCulture);
                x = l.Keys.Aggregate(x, (current, k) => current + ("\t" + (i < l[k].Count ? l[k][i].ToString(CultureInfo.InvariantCulture) : "-")));
                output.Add(x);
            }
            File.WriteAllLines(@"..\..\..\..\Resources\variations\test.log", output);
        }

        [Test]
        public void ProfileInfoTest()
        {
            var p = ModelFactory.GetModel<StfsPackage>(@"..\..\..\..\Resources\mergeable\aktualis\E00001D5D85ED487.orig");
            var pfile = p.ExtractFile(p.TitleId + ".gpd");
            File.WriteAllBytes(@"..\..\..\..\Resources\mergeable\aktualis\untouched.profile", pfile);

            var ok = ModelFactory.GetModel<DashboardFile>(pfile);
            ok.Parse();
            File.WriteAllLines(@"..\..\..\..\Resources\mergeable\aktualis\untouched.map", ok.BinMap.Output());

            var pi = ModelFactory.GetModel<DashboardFile>(pfile);
            pi.Parse();
            pi.Recalculate();
            pi.Rebuild();
            var pfile2 = pi.Binary.ReadAll();
            File.WriteAllBytes(@"..\..\..\..\Resources\mergeable\aktualis\rebuilt.profile", pfile2);
            File.WriteAllLines(@"..\..\..\..\Resources\mergeable\aktualis\rebuilt.map", pi.BinMap.Output());

            p.ReplaceFile(p.GetFileEntry(p.TitleId + ".gpd"), pi);
            p.Save(@"..\..\..\..\Resources\mergeable\aktualis\E00001D5D85ED487.pi");

            //BinMapCompare(pi, ok);

            //Debug.WriteLine("{0} {1}", ok.EntryCount, pi.EntryCount);
            //Debug.WriteLine("{0} {1}", ok.EntryTableLength, pi.EntryTableLength);
            //Debug.WriteLine("{0} {1}", ok.FreeSpaceTableEntryCount, pi.FreeSpaceTableEntryCount);
            //Debug.WriteLine("{0} {1}", ok.FreeSpaceTableLength, pi.FreeSpaceTableLength);

            //CompareEntryList(ok.Achievements, pi.Achievements);
            //CompareEntryList(ok.Settings, pi.Settings);
            //CompareEntryList(ok.TitlesPlayed, pi.TitlesPlayed);
            //CompareEntryList(ok.Strings, pi.Strings);
            //CompareEntryList(ok.Images, pi.Images);
        }

        private void CompareEntryList<T>(EntryList<T> a, EntryList<T> b) where T : EntryBase
        {
            for (var i = 0; i < a.Count; i++)
            {
                BinMapHelper.ModelCompare(a.ToList()[i], b.ToList()[i]);
            }
        }

        [Test]
        public void FreeSpaceTableTest()
        {
            var files = Directory.GetFiles(@"c:\merc\Contour\Resources\xbox_profile\extract\Root\", "*.gpd");
            foreach (var file in files)
            {
                var gpdBytes = File.ReadAllBytes(file);
                var gpd = ModelFactory.GetModel<GpdFile>(gpdBytes);
                Debug.WriteLine("[{4}] {0,3}/{1} {2,3}/{3} {5,6} {6} {7} {8}", gpd.EntryCount, gpd.EntryTableLength, gpd.FreeSpaceTableEntryCount, gpd.FreeSpaceTableLength, Path.GetFileName(file), gpdBytes.Length, gpd.FreeSpace[0].Length, gpd.FreeSpace[0].AddressSpecifier, 0x18 + gpd.EntryTableLength * 0x12 + gpd.FreeSpaceTableLength * 0x8);
            }
        }

        [Test]
        public void GenderTest()
        {
            var profiles = new[] { "E000000F48B24EB1", "E00001575AC16703", "E00001D5D85ED487", "E000050E524FCEFD" };

            foreach (var profile in profiles)
            {
                var p = ModelFactory.GetModel<StfsPackage>(string.Format(@"c:\inetpub\ftproot\Hdd1\Content\{0}\FFFE07D1\00010000\{0}", profile));
                Debug.WriteLine(p.VolumeDescriptor.BlockSeparation);
            }
        }

        [Test]
        public void ResignBorderlands2Save()
        {
            var save = ModelFactory.GetModel<SvodPackage>(@"d:\Work\NTX-GODS\Ivory\E00001D5D85ED487\5454087C\00000001\Save0001.sav");
            var profile = ModelFactory.GetModel<StfsPackage>(@"d:\Work\NTX-GODS\liveprofile\Content\EA17543C80735A00\FFFE07D1\00010000\EA17543C80735A00");
            save.ConsoleId = profile.ConsoleId;
            save.DeviceId = profile.DeviceId;
            save.ProfileId = profile.ProfileId;
            save.Save(@"d:\Work\NTX-GODS\Ivory\x\5454087C\00000001\Save0002.sav");
        }

        [Test]
        public void ResignBorderlandsPSSave()
        {
            var profbin = ModelFactory.GetModel<SvodPackage>(@"e:\Work\NTX-GODS\Ivory\E00001D5D85ED487\545408B4\00000001\profile.bin");
            var save = ModelFactory.GetModel<SvodPackage>(@"e:\Work\NTX-GODS\Ivory\E00001D5D85ED487\545408B4\00000001\Save0001.sav");
            var savebak = ModelFactory.GetModel<SvodPackage>(@"e:\Work\NTX-GODS\Ivory\E00001D5D85ED487\545408B4\00000001\Save0001.sav.bak");

            var profile = ModelFactory.GetModel<StfsPackage>(@"e:\Work\NTX-GODS\liveprofile\Content\EA17543C80735A00\FFFE07D1\00010000\EA17543C80735A00");

            profbin.ConsoleId = profile.ConsoleId;
            profbin.DeviceId = profile.DeviceId;
            profbin.ProfileId = profile.ProfileId;
            profbin.Save(@"e:\Work\NTX-GODS\Ivory\x\545408B4\00000001\profile.bin");

            save.ConsoleId = profile.ConsoleId;
            save.DeviceId = profile.DeviceId;
            save.ProfileId = profile.ProfileId;
            save.Save(@"e:\Work\NTX-GODS\Ivory\x\545408B4\00000001\Save0001.sav");

            savebak.ConsoleId = profile.ConsoleId;
            savebak.DeviceId = profile.DeviceId;
            savebak.ProfileId = profile.ProfileId;
            savebak.Save(@"e:\Work\NTX-GODS\Ivory\x\545408B4\00000001\Save0001.sav.bak");
        }

    }
}