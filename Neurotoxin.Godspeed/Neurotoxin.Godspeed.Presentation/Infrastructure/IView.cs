namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IView
    {
    }

    public interface IView<T> : IView
    {
        T ViewModel { get; }
    }

}