#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Linq;
using UnityEngine;
using System.Threading;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.LowLevel;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
#else
using UnityEngine.Experimental.LowLevel;
using PlayerLoopType = UnityEngine.Experimental.PlayerLoop;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif
using Peg.Linq;

namespace Peg.UpdateSystem
{
    /// <summary>
    /// A global system that is started at the beginning of the app. Objects implementing
    /// the IUpdateable interface can register with it in order to receive update info every frame.
    /// </summary>
    public sealed class SharedUpdateSystem : IUpdateSystem
    {
        readonly List<ILoopUpdatable> PreUpdateQueue = new();
        readonly List<ILoopUpdatable> UpdateQueue = new();
        readonly List<ILoopUpdatable> LateUpdateQueue = new();
        public string SystemName => "Shared Update System";

        PlayerLoopSystem PreUpdateSys;
        PlayerLoopSystem UpdateSys;
        PlayerLoopSystem LateUpdateSys;

        

        /// <summary>
        /// 
        /// </summary>
        public SharedUpdateSystem()
        {
            PreUpdateSys = new PlayerLoopSystem()
            {
                updateDelegate = PreUpdate,
                type = typeof(SharedUpdateSystem),
            };
            UpdateSys = new PlayerLoopSystem()
            {
                updateDelegate = Update,
                type = typeof(SharedUpdateSystem),
            };
            LateUpdateSys = new PlayerLoopSystem()
            {
                updateDelegate = LateUpdate,
                type = typeof(SharedUpdateSystem),
            };

            var defaultSystems = PlayerLoop.GetDefaultPlayerLoop();
            PlayerLoopInjector.InjectSubSystem(ref defaultSystems, typeof(PreUpdate), ref PreUpdateSys);
            PlayerLoopInjector.InjectSubSystem(ref defaultSystems, typeof(Update), ref UpdateSys);
            PlayerLoopInjector.InjectSubSystem(ref defaultSystems, typeof(PostLateUpdate), ref LateUpdateSys);
            PlayerLoop.SetPlayerLoop(defaultSystems);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAppEnd()
        {
            foreach (var preUpdate in PreUpdateQueue)
                preUpdate.OnShutdown(this);

            foreach (var preUpdate in UpdateQueue)
                preUpdate.OnShutdown(this);

            foreach (var preUpdate in PreUpdateQueue)
                preUpdate.OnShutdown(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateable"></param>
        /// <param name="timing"></param>
        /// <param name="priority">Currently ignored</param>
        public void RegisterUpdatable(ILoopUpdatable updateable, PlayerLoopTiming timing, int priority = 0)
        {
            switch (timing)
            {
                case PlayerLoopTiming.PreUpdate:
                    {
                        PreUpdateQueue.Add(updateable);
                        break;
                    }
                case PlayerLoopTiming.Update:
                    {
                        UpdateQueue.Add(updateable);
                        break;
                    }
                case PlayerLoopTiming.LateUpdate:
                    {
                        LateUpdateQueue.Add(updateable);
                        break;
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateable"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void UnregisterUpdateable(ILoopUpdatable updateable)
        {
            if (!PreUpdateQueue.Remove(updateable))
                if (!UpdateQueue.Remove(updateable))
                    LateUpdateQueue.Remove(updateable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void PreUpdate()
        {
            for (int i = 0; i < PreUpdateQueue.Count; i++)
                PreUpdateQueue[i].Update();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Update()
        {
            for (int i = 0; i < UpdateQueue.Count; i++)
                UpdateQueue[i].Update();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void LateUpdate()
        {
            for (int i = 0; i < LateUpdateQueue.Count; i++)
                LateUpdateQueue[i].Update();

        }
    }

}