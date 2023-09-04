
namespace Peg.UpdateSystem
{

    /// <summary>
    /// 
    /// </summary>
    public interface ILoopUpdatable
    {
        void OnSystemInit(IUpdateSystem system);
        void Update();
        void OnShutdown(IUpdateSystem system);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IPreUpdatable<T> : ILoopUpdatable where T : IUpdateSystem{ }

    /// <summary>
    /// 
    /// </summary>
    public interface IUpdatable<T> : ILoopUpdatable where T : IUpdateSystem { }

    /// <summary>
    /// 
    /// </summary>
    public interface ILateUpdatable<T> : ILoopUpdatable where T : IUpdateSystem { }
}
