using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class OperationResult
    {
        public TransferResult Result { get; private set; }
        public string TargetPath { get; private set; }

        public static bool operator == (OperationResult cp, TransferResult tp)
        {
            return cp != null && cp.Result == tp;
        }

        public static bool operator != (OperationResult cp, TransferResult tp)
        {
            return !(cp == tp);
        }

        public OperationResult(TransferResult result, string targetPath = null)
        {
            Result = result;
            TargetPath = targetPath;
        }

        protected bool Equals(OperationResult other)
        {
            return Result.Equals(other.Result) && string.Equals(TargetPath, other.TargetPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperationResult)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Result.GetHashCode() * 397) ^ (TargetPath != null ? TargetPath.GetHashCode() : 0);
            }
        }
    }
}