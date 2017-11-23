using System;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database.Models
{
    public class Statistics : ModelBase
    {
        internal static int LatestVersion = 2;

        [Index]
        [PrimaryKey]
        public int Id { get; set; }

        private int _version = 1;
        public int Version
        {
            get { return _version; }
            set { _version = value; SetDirtyFlag("Version"); }
        }

        private int _totalFilesTransferred;
        public int TotalFilesTransferred
        {
            get { return _totalFilesTransferred; }
            set { _totalFilesTransferred = value; SetDirtyFlag("TotalFilesTransferred"); }
        }

        private long _totalBytesTransferred;
        public long TotalBytesTransferred
        {
            get { return _totalBytesTransferred; }
            set { _totalBytesTransferred = value; SetDirtyFlag("TotalBytesTransferred"); }
        }

        private TimeSpan _totalTimeSpentWithTransfer;
        public TimeSpan TotalTimeSpentWithTransfer
        {
            get { return _totalTimeSpentWithTransfer; }
            set { _totalTimeSpentWithTransfer = value; SetDirtyFlag("TotalTimeSpentWithTransfer"); }
        }

        private TimeSpan _totalUsageTime;
        public TimeSpan TotalUsageTime
        {
            get { return _totalUsageTime; }
            set { _totalUsageTime = value; SetDirtyFlag("TotalUsageTime"); }
        }

        private int _applicationStarted;
        public int ApplicationStarted
        {
            get { return _applicationStarted; }
            set { _applicationStarted = value; SetDirtyFlag("ApplicationStarted"); }
        }

        private int _applicationCrashed;
        public int ApplicationCrashed
        {
            get { return _applicationCrashed; }
            set { _applicationCrashed = value; SetDirtyFlag("ApplicationCrashed"); }
        }

        private int _successfulUpdate;
        public int SuccessfulUpdate
        {
            get { return _successfulUpdate; }
            set { _successfulUpdate = value; SetDirtyFlag("SuccessfulUpdate"); }
        }

        private int _unsuccessfulUpdate;
        public int UnsuccessfulUpdate
        {
            get { return _unsuccessfulUpdate; }
            set { _unsuccessfulUpdate = value; SetDirtyFlag("UnsuccessfulUpdate"); }
        }
    }
}