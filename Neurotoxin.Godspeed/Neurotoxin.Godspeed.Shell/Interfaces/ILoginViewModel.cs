namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ILoginViewModel
    {
        string Title { get; set; }
        string Message { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        bool IsUseDefaultEnabled { get; set; }
        bool IsRememberPasswordEnabled { get; set; }
        bool RememberPassword { get; set; }
        bool IsValid { get; }
    }
}