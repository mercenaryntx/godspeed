namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IGeneralController
    {
        /// <summary>
        /// Starts the controller.
        /// </summary>
        void Run();

        /// <summary>
        /// Resets the controller. Stops it if running.
        /// </summary>
        void Reset();
    }
}
