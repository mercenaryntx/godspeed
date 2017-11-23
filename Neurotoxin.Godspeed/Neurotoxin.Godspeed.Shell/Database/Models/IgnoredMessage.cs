using Neurotoxin.Godspeed.Core.Extensions;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database.Models
{
    public class IgnoredMessage : ModelBase
    {
        [Index]
        [PrimaryKey]
        [StringLength(40)]
        public string MessageHash { get; set; }

        public IgnoredMessage()
        {
        }

        public IgnoredMessage(string message)
        {
            MessageHash = message.Hash();
        }
    }
}