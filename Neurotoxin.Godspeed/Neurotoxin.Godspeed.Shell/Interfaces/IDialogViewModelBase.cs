using System;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IDialogViewModelBase
    {
        void Close();
        event EventHandler Closing;
    }
}