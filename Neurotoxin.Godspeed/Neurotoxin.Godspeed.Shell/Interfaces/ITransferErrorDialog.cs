using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ITransferErrorDialog
    {
        bool? ShowDialog();
        TransferErrorDialogResult Result { get; }
    }
}