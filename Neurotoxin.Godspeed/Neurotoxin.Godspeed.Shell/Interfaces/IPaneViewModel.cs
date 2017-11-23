using System;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IPaneViewModel : IViewModel, IDisposable
    {
        bool IsLoaded { get; }
        FileListPaneSettings Settings { get; }
        string ProgressMessage { get; }
        bool IsActive { get; }
        void SetActive();
        void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null);
        object Close(object data);
        void Refresh(bool refreshCache);
    }

}