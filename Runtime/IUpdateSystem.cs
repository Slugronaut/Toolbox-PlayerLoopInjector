

namespace Toolbox.UpdateSystem
{

    /// <summary>
    /// Any class implementing this interface will be injectable into the manua loop update cycle.
    /// Combine this with the [AutoCreate] attribute to allow it to be automatically created and injected
    /// at application startup.
    /// </summary>
    public interface IUpdateSystem
    {
        void RegisterUpdatable(ILoopUpdatable updateable, PlayerLoopTiming timing, int priority = 0);
        void UnregisterUpdateable(ILoopUpdatable updateable);
        void PreUpdate();
        void Update();
        void LateUpdate();
        void OnAppEnd();
        string SystemName { get; }
    }
}
