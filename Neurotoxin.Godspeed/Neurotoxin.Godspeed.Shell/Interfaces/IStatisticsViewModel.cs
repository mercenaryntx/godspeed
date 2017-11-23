using System;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IStatisticsViewModel : IViewModel
    {
        int GamesRecognizedFully { get; }
        int GamesRecognizedPartially { get; }
        int SvodPackagesRecognized { get; }
        int StfsPackagesRecognized { get; }
        int FilesTransferred { get; set; }
        int TotalFilesTransferred { get; set; }
        long BytesTransferred { get; set; }
        long TotalBytesTransferred { get; set; }
        TimeSpan TimeSpentWithTransfer { get; set; }
        TimeSpan TotalTimeSpentWithTransfer { get; set; }
        TimeSpan UsageTime { get; }
        TimeSpan TotalUsageTime { get; set; }
        int ApplicationStarted { get; set; }
        int ApplicationCrashed { get; set; }
        int SuccessfulUpdate { get; set; }
        int UnsuccessfulUpdate { get; set; }
    }
}