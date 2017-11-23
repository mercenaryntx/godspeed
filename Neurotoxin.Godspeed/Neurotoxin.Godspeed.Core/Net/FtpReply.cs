using System.Globalization;
using System.Text.RegularExpressions;

namespace Neurotoxin.Godspeed.Core.Net 
{

    public class FtpReply : IFtpReply 
    {
        public FtpResponseType Type { get; private set; }
        public string Raw { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public bool IsMultipart { get; private set; }
        public string InfoMessages { get; set; }
        public bool Success { get; private set; }

        public FtpReply(string line)
        {
            Raw = line;

            var m = Regex.Match(line, "^(?<code>[0-9]{3})(?<delimiter>[ -])(?<message>.*)$", RegexOptions.Multiline);
            if (!m.Success) return;

            Code = m.Groups["code"].Value;
            Message = m.Groups["message"].Value;
            IsMultipart = m.Groups["delimiter"].Value == "-";

            if (string.IsNullOrEmpty(Code)) return;
            int code;
            int.TryParse(Code[0].ToString(CultureInfo.InvariantCulture), out code);
            Type = (FtpResponseType) code;
            Success = code >= 1 && code <= 3;
        }
    }
}
