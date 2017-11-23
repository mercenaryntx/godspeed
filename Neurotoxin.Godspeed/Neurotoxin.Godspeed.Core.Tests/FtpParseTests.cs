using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Neurotoxin.Godspeed.Core.Net;

namespace Neurotoxin.Godspeed.Core.Tests
{
    [TestFixture]
    public class FtpParseTests
    {
        [Test]
        public void Issue549()
        {
            var raw = new List<string>
                          {
                              @"dr--r--r--   1 root  root    0                  Game", //unknown ftp server
                              @"-rwxrwxrwx   1 root root      13799424 Jul 11 2011 AvatarAssetPack" //aurora
                          };

            FtpListItem.Parser parser = null;

            var buf = raw[0];
            var item = FtpListItem.Parse("/", buf, FtpCapability.NONE, out parser);
            Assert.IsNotNull(parser);
            Assert.AreEqual(item.Type, FtpFileSystemObjectType.Directory);
            Assert.AreEqual(item.Name, "Game");
            Assert.AreEqual(item.Modified, DateTime.MinValue);
            Assert.AreEqual(item.Size, 0);

            buf = raw[1];
            item = FtpListItem.Parse("/", buf, FtpCapability.NONE, out parser);
            Assert.IsNotNull(parser);
            Assert.AreEqual(item.Type, FtpFileSystemObjectType.File);
            Assert.AreEqual(item.Name, "AvatarAssetPack");
            Assert.AreEqual(item.Modified, new DateTime(2011, 7, 11));
            Assert.AreEqual(item.Size, 13799424);

        }
    }
}