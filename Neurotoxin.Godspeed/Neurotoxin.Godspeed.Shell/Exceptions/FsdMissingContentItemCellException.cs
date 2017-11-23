using System;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Exceptions
{
    public class FsdMissingContentItemCellException : Exception
    {
        public FsdMissingContentItemCellException(FsdContentItemProperty key, string innerHtml)
            : base(string.Format("Cell named {0} is missing. (InnerHTML: {1})", key, innerHtml))
        {
        }
    }
}
