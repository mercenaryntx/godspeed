using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using FakeItEasy;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Dummies;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Tests
{
    [TestClass]
    public class FakingTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TreeDepthTest()
        {
            var m = new DummyContent {FakingRules = new FakingRules {TreeDepth = 3, ItemCount = 1, ItemTypes = new [] { ItemType.Directory }}};
            var l1 = m.GetDrives();
            Assert.IsTrue(l1.Any(), "There should be one drive");
            var l2 = GetList(m, l1.First().Path);
            Assert.IsTrue(l2.Any(), "There should be one item on the second level");
            var l3 = GetList(m, l2.First().Path);
            Assert.IsTrue(l3.Any(), "There should be one item on the third level");
        }

        [TestMethod]
        public void TreeDepthRangeTest()
        {
            var m = new DummyContent { FakingRules = new FakingRules { TreeDepth = new Range(2, 4), ItemCount = 1, ItemTypes = new[] { ItemType.Directory }}};
            var l1 = m.GetDrives();
            Assert.IsTrue(l1.Any(), "There should be one drive");
            var l2 = GetList(m, l1.First().Path);
            Assert.IsTrue(l2.Any(), "There should be one item on level 2");
            var l3 = GetList(m, l2.First().Path);
            if (!l3.Any()) return;
            var l4 = GetList(m, l3.First().Path);
            if (!l4.Any()) return;
            var l5 = GetList(m, l4.First().Path);
            Assert.IsFalse(l5.Any(), "There should be any items below level 4");
        }

        [TestMethod]
        public void DriveTypeTest()
        {
            var m = new DummyContent { FakingRules = new FakingRules { TreeDepth = 1, ItemCount = 10 } };
            var l1 = m.GetDrives();
            var sb = new StringBuilder();
            foreach (var item in l1.Where(item => item.Type != ItemType.Drive))
            {
                sb.AppendFormat("Item {0} should be Drive instead of {1}{2}", item.Name, item.Type, Environment.NewLine);
            }
            Assert.IsTrue(sb.Length == 0, sb.ToString());
        }

        [TestMethod]
        public void FileAndDirectoryTypeTest()
        {
            var m = new DummyContent { FakingRules = new FakingRules { TreeDepth = 3, ItemCount = 10 } };
            var l1 = m.GetDrives();
            var l2 = GetList(m, l1.First().Path);
            Assert.IsTrue(l2.All(d => d.Type == ItemType.Directory || d.Type == ItemType.File), "All item type on level 2 should be File or Directory");
            var l3 = GetList(m, l2.First().Path);
            Assert.IsTrue(l2.All(d => d.Type == ItemType.Directory || d.Type == ItemType.File), "All item type on level 3 should be File or Directory");
        }

        private IEnumerable<FileSystemItem> GetList(DummyContent m, string path)
        {
            Console.WriteLine("[GetList] " + path);
            return m.GetList(path);
        }
    }
}