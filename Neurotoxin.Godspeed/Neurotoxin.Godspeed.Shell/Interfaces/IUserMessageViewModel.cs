namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IUserMessageViewModel
    {
        string Message { get; }
        bool IsRead { get; set; }
    }
}