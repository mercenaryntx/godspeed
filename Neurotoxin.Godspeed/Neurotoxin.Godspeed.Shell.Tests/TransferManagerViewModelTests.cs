using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using FakeItEasy;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Dummies;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Godspeed.Shell.Tests
{
    [TestClass]
    public class TransferManagerViewModelTests
    {
        private static IUnityContainer Container { get; set; }
        private static IEventAggregator EventAggregator { get; set; }
        private static ConsoleWriter ConsoleWriter { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Container = new UnityContainer();
            Container.RegisterType<IWorkHandler, SyncWorkHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowManager, ConsoleWriter>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(A.Fake<IStatisticsViewModel>());
            Container.RegisterInstance(A.Fake<IUserSettingsProvider>());
            Container.RegisterInstance(A.Fake<ITitleRecognizer>());
            Container.RegisterInstance(A.Fake<IResourceManager>());
            Container.RegisterType<TransferManagerViewModel>();

            UnityInstance.Container = Container;
            EventAggregator = Container.Resolve<IEventAggregator>();
            ConsoleWriter = Container.Resolve<IWindowManager>() as ConsoleWriter;
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            
        }

        [TestMethod]
        public void InstantiationTest()
        {
            var vm = GetInstance();
            Assert.IsNotNull(vm, "TransferManagerViewModel should not be null");
        }

        [TestMethod]
        public void CopyTest()
        {
            var vm = GetInstance();
            var a = GetDummyContentViewModel("Source");
            var b = GetDummyContentViewModel("Target");
            a.SelectAllCommand.Execute(null);
            var selection = a.SelectedItems.Select(i => i.Model).ToList();
            vm.Copy(a, b, null);

            var sb = new StringBuilder();
            foreach (var aitem in selection)
            {
                if (!b.Items.Any(bitem => IsCopy(aitem, bitem.Model)))
                    sb.AppendLine(aitem.Name + " is missing from target");
            }
            var errorMessage = sb.ToString();
            Assert.IsTrue(string.IsNullOrEmpty(errorMessage), errorMessage);
        }

        [TestMethod]
        public void CopyNotificationTest01()
        {
            var vm = GetInstance();
            var a = GetDummyContentViewModel("Source");
            var b = GetDummyContentViewModel("Target");
            a.CurrentRow = a.Items.First(i => i.Name != "..");
            a.ToggleSelectionCommand.Execute(ToggleSelectionMode.Insert);

            var change = new List<TransferProgressChangedEventArgs>();
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(change.Add);

            var finish = new List<TransferFinishedEventArgs>();
            EventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(finish.Add);

            vm.Copy(a, b, null);

            Assert.IsTrue(change.Count > 0, "TransferProgressChangedEvent should be triggered at least one time.");
            Assert.AreEqual(1, finish.Count, "TransferFinishedEvent should be triggered exactly one time.");
        }

        [TestMethod]
        public void CopyNotificationTest02()
        {
            var vm = GetInstance();
            var a = GetDummyContentViewModel("Source");
            var b = GetDummyContentViewModel("Target");
            var selection = a.Items.First(i => i.Name != "..");
            a.CurrentRow = selection;
            a.ToggleSelectionCommand.Execute(ToggleSelectionMode.Insert);
            Assert.AreEqual(1, a.SelectedItems.Count(), "One item should be selected");
            vm.Copy(a, b, null);

            Assert.IsTrue(b.Items.Any(i => i.Name == selection.Name), string.Format("The file {0} should have been copied", selection.Name));

            var errorOccured = 0;
            ConsoleWriter.WriteErrorDialogResult = () => { errorOccured++; return new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.Current, CopyAction.Overwrite); };

            a.CurrentRow = selection;
            a.ToggleSelectionCommand.Execute(ToggleSelectionMode.Insert);
            Assert.AreEqual(1, a.SelectedItems.Count(), "One item should be selected");
            Assert.AreEqual(selection.Name, a.SelectedItems.First().Name, "The same item should have been selected for the second time too");

            var finish = new List<TransferFinishedEventArgs>();
            EventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(finish.Add);

            vm.Copy(a, b, null);

            Assert.AreEqual(1, errorOccured, "A file exists error should have been occurred exactly one time.");
            Assert.AreEqual(1, finish.Count, "TransferFinishedEvent should be triggered exactly one time.");
        }

        [TestMethod]
        public void CopyNotificationTest03()
        {
            var vm = GetInstance();
            var a = GetDummyContentViewModel("Source");
            var b = GetDummyContentViewModel("Target");
            var selection = a.Items.First(i => i.Name != "..");
            a.CurrentRow = selection;
            a.ToggleSelectionCommand.Execute(ToggleSelectionMode.Insert);

            Assert.AreEqual(1, a.SelectedItems.Count(), "One item should be selected");

            //first we copy the first file of the file
            var sourcePath = selection.Model.GetRelativePath(a.CurrentFolder.Path);
            var targetPath = b.GetTargetPath(sourcePath);
            var targetSize = selection.Size/2;

            using (var targetStream = b.GetStream(targetPath, FileMode.CreateNew, FileAccess.Write, 0))
            {
                a.CopyStream(selection.Model, targetStream, 0, targetSize);
            }
            b.Refresh(false);

            Assert.IsTrue(b.Items.Any(i => i.Name == selection.Name), string.Format("The file {0} should have been copied", selection.Name));
            Assert.IsTrue(b.Items.Any(i => i.Name == selection.Name && i.Size == targetSize), string.Format("The file {0} should be {1} bytes", selection.Name, targetSize));

            var errorOccured = 0;
            ConsoleWriter.WriteErrorDialogResult = () => { errorOccured++; return new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.Current, CopyAction.Resume); };

            var finish = new List<TransferFinishedEventArgs>();
            EventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(finish.Add);
            //then we try to do the proper way and see if it appends it
            vm.Copy(a, b, null);

            Assert.AreEqual(1, errorOccured, "A file exists error should have been occurred exactly one time.");
            Assert.AreEqual(1, finish.Count, "TransferFinishedEvent should be triggered exactly one time.");
            Assert.IsTrue(b.Items.Any(i => i.Name == selection.Name && i.Size == selection.Size), string.Format("The file {0} should be {1} bytes", selection.Name, selection.Size));
        }

        [TestMethod]
        public void CopyNotificationTest04()
        {
            const string tmpPath = @"C:\tmp\";
            const string aPath = @"C:\tmp\a\";
            const string bPath = @"C:\tmp\b\";
            const string afPath = @"C:\tmp\a\test.01";
            const string bfPath = @"C:\tmp\b\test.01";
            if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);
            if (!Directory.Exists(aPath)) Directory.CreateDirectory(aPath);
            if (!Directory.Exists(bPath)) Directory.CreateDirectory(bPath);
            if (File.Exists(afPath)) File.Delete(afPath);
            if (File.Exists(bfPath)) File.Delete(bfPath);

            var content = C.Random<byte[]>(0x8FFF, 0xFFFF);
            File.WriteAllBytes(afPath, content);

            var vm = GetInstance();
            var a = new LocalFileSystemContentViewModel();
            a.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(new FileListPaneSettings(aPath, "Name", ListSortDirection.Ascending, ColumnMode.Title, FileListPaneViewMode.List)));
            var b = new LocalFileSystemContentViewModel();
            b.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(new FileListPaneSettings(bPath, "Name", ListSortDirection.Ascending, ColumnMode.Title, FileListPaneViewMode.List)));

            var selection = a.Items.First(i => i.Name != "..");
            a.CurrentRow = selection;
            a.ToggleSelectionCommand.Execute(ToggleSelectionMode.Insert);
            Assert.AreEqual(1, a.SelectedItems.Count(), "One item should be selected");

            //first we copy the first file of the file
            var sourcePath = selection.Model.GetRelativePath(a.CurrentFolder.Path);
            var targetPath = b.GetTargetPath(sourcePath);
            var targetSize = selection.Size / 2;

            using (var targetStream = b.GetStream(targetPath, FileMode.CreateNew, FileAccess.Write, 0))
            {
                a.CopyStream(selection.Model, targetStream, 0, targetSize);
            }
            b.Refresh(false);

            Assert.IsTrue(b.Items.Any(i => i.Name == selection.Name), string.Format("The file {0} should have been copied", selection.Name));
            Assert.IsTrue(b.Items.Any(i => i.Name == selection.Name && i.Size == targetSize), string.Format("The file {0} should be {1} bytes", selection.Name, targetSize));

            var errorOccured = 0;
            ConsoleWriter.WriteErrorDialogResult = () => { errorOccured++; return new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.Current, CopyAction.Resume); };

            var finish = new List<TransferFinishedEventArgs>();
            EventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(finish.Add);
            //then we try to do the proper way and see if it appends it
            vm.Copy(a, b, null);

            Assert.AreEqual(1, errorOccured, "A file exists error should have been occurred exactly one time.");
            Assert.AreEqual(1, finish.Count, "TransferFinishedEvent should be triggered exactly one time.");
            Assert.IsTrue(b.Items.Any(i => i.Name == selection.Name && i.Size == selection.Size), string.Format("The file {0} should be {1} bytes", selection.Name, selection.Size));

            var bBytes = new byte[content.Length];
            using (var bStream = b.GetStream(targetPath, FileMode.Open, FileAccess.Read, 0))
            {
                bStream.Read(bBytes, 0, content.Length);
            }

            for (var i = 0; i < content.Length; i++)
            {
                Assert.AreEqual(content[i], bBytes[i], "The files content doesn't much at position: " + i);
            }

            Directory.Delete(tmpPath, true);
        }

        private bool IsCopy(FileSystemItem a, FileSystemItem b)
        {
            //TODO
            return a.Name == b.Name;
        }

        private TransferManagerViewModel GetInstance()
        {
            return Container.Resolve<TransferManagerViewModel>();
        }

        private DummyContentViewModel GetDummyContentViewModel(string name)
        {
            var dummy = new DummyContentViewModel(new FakingRules
                                                          {
                                                              TreeDepth = 3,
                                                              ItemCount = new Range(5, 10),
                                                              ItemCountOnLevel = new Dictionary<int, Range>
                                                                                     {
                                                                                         {0, new Range(1,5)}
                                                                                     }
                                                          });
            dummy.Drive = dummy.Drives.First();

            Console.WriteLine(name + ":");
            dummy.Items.ForEach(i => Console.WriteLine(i.Name));
            return dummy;
        }

    }
}