namespace Neurotoxin.Godspeed.Core.Net
{

    public interface IFtpReply
    {
        FtpResponseType Type { get; }
        string Raw { get; }
        string Code { get; }
        string Message { get; }
        bool IsMultipart { get; }
        string InfoMessages { get; set; }
        bool Success { get; }
    }
}