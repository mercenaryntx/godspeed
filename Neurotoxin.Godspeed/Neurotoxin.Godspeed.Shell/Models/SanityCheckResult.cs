using System.Collections.Generic;
using Neurotoxin.Godspeed.Shell.Events;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class SanityCheckResult
    {
        public bool DatabaseCreated { get; set; }
        public List<NotifyUserMessageEventArgs> UserMessages { get; set; }
    }
}