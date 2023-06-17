namespace Toolbox.UpdateSystem
{
    /// <summary>
    /// Base class for creating simple objects that automatically
    /// register and update every frame while the app is running.
    /// </summary>
    public abstract class Updatable : IUpdatable<SharedUpdateSystem>
    {
        IUpdateSystem System;
        bool _Enabled = true;

        public bool Enabled
        {
            get => _Enabled;
            set
            {
                if (_Enabled == value) return;

                _Enabled = value;
                if (value)
                    System.RegisterUpdatable(this, PlayerLoopTiming.Update);
                else System.UnregisterUpdateable(this);
            }
        }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void Awake() { }

        public virtual void Update() { }

        public void OnShutdown(IUpdateSystem system)
        {
            _Enabled = false;
            OnDisable();
        }

        public void OnSystemInit(IUpdateSystem system)
        {
            System = system;
            _Enabled = true;
            Awake();
            OnEnable();
        }

    }
}