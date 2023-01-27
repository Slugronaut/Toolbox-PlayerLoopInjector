
namespace Toolbox.UpdateSystem
{
    /// <summary>
    /// List of possible points within the engine update that an injected delegate will be invoked.
    /// </summary>
    public enum PlayerLoopTiming
    {
        PreUpdate,
        Update,
        LateUpdate,
    }
}