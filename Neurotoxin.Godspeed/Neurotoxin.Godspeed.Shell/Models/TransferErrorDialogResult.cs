using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class TransferErrorDialogResult
    {
        public ErrorResolutionBehavior Behavior { get; set; }
        public CopyAction? Action { get; set; }
        public CopyActionScope Scope { get; set; }

        public TransferErrorDialogResult(ErrorResolutionBehavior behavior, CopyActionScope scope = Constants.CopyActionScope.Current, CopyAction? action = null)
        {
            Behavior = behavior;
            Scope = scope;
            Action = action;
        }
    }
}