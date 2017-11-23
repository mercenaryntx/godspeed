using System.Runtime.Serialization;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class F3PluginGetProfileInfo
    {
        [DataMember(Name = "index")]
        public int Index { get; set; }

        [DataMember(Name = "gamertag")]
        public string Gamertag { get; set; }

        [DataMember(Name = "gamerscore")]
        public int Gamerscore { get; set; }

        [DataMember(Name = "signedin")]
        public int SignedIn { get; set; }

        [DataMember(Name = "xuid")]
        public string Xuid { get; set; }
    }
}