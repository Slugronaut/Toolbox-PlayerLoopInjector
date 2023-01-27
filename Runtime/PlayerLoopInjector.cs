using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.AutoCreate;
using Toolbox.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Toolbox.UpdateSystem
{
    /// <summary>
    /// Creates, inializes, and injects PlayerLoopSystems into the core engine update cycle.
    /// It also automatically creates and registers object implementing an updatable interface with said systems.
    /// This is all done via reflection at application startup.
    /// </summary>
    public static class PlayerLoopInjector
    {
        static readonly List<IUpdateSystem> Systems = new();
        static PlayerLoopSystem DefaultLoopSystem;


        /// <summary>
        /// Returns all types that implement the IUpdateSystem interface.
        /// </summary>
        public static Type[] FindAllUpdateSystemTypes()
        {
            return TypeHelper.GetDerivedTypes(typeof(IUpdateSystem), true, false, false);
        }

        /// <summary>
        /// Filters an enumeration of objects to ensure they are updatable interfaces that are compatible with the given system type.
        /// </summary>
        /// <param name="systemTypes"></param>
        /// <param name="autoCreatedObjects"></param>
        /// <returns></returns>
        public static IEnumerable<ILoopUpdatable> FilterUpdatableTypes(Type systemType, IEnumerable<object> autoCreatedObjects)
        {
            if (!systemType.GetInterfaces().Any(x => x == typeof(IUpdateSystem)))
                throw new ArgumentException("The supplied systemType must implement the IUpdateSystem interface.", "systemType");

            //filter the list of auto-creatables so that we only consider
            //those that take this iterations given system type as their generic parameter.
            return autoCreatedObjects
                .Where(x => x is ILoopUpdatable)
                .Select(x => x as ILoopUpdatable)
                .Where(x =>
                {
                    var type = x.GetType();
                    return type.GetInterfaces().Any(x => x.IsGenericType && x.GenericTypeArguments[0] == systemType);
                });
        }

        /// <summary>
        /// 
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Initialize()
        {
            DefaultLoopSystem = PlayerLoop.GetDefaultPlayerLoop();
            Application.quitting += HandleAppExit;
            Systems.Clear();

            var systemTypes = FindAllUpdateSystemTypes();
            foreach (var systemType in systemTypes)
                InstantiateAutoCreatable(systemType, FilterUpdatableTypes(systemType, AutoCreator.AutoCreatedObjects));

        }

        /// <summary>
        /// Helper method for creating a given type that implements IUpdateSystem and auto-creating
        /// and registering and
        /// </summary>
        /// <param name="systemType"></param>
        static void InstantiateAutoCreatable(Type systemType, IEnumerable<ILoopUpdatable> autoCreatables)
        {
            if (Activator.CreateInstance(systemType) is IUpdateSystem system)
            {
                Systems.Add(system);

                //we are breaking some rules here by simultaneously handle ALL types with the [AutoCreate] attribute
                //as well as specifically handle updateable types for this sytem. Ah well, there goes SOLID :P
                foreach (var inst in autoCreatables)
                {
                    var interfaces = inst.GetType().GetInterfaces();
                    //so here we can establish what, if any, system this new type instance is associated with.
                    if (interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IPreUpdatable<>)))
                    {
                        system.RegisterUpdatable(inst, PlayerLoopTiming.PreUpdate);
                        inst.OnSystemInit(system);
                    }
                    if (interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IUpdatable<>)))
                    {
                        system.RegisterUpdatable(inst, PlayerLoopTiming.Update);
                        inst.OnSystemInit(system);
                    }
                    if (interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ILateUpdatable<>)))
                    {
                        system.RegisterUpdatable(inst, PlayerLoopTiming.LateUpdate);
                        inst.OnSystemInit(system);
                    }
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void HandleAppExit()
        {
            Application.quitting -= HandleAppExit;
            PlayerLoop.SetPlayerLoop(DefaultLoopSystem);
            foreach (var system in Systems)
            {
                system.OnAppEnd();
            }
            Systems.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="position"></param>
        /// <param name="order"></param>
        /// <param name="systemToInject"></param>
        public static bool InjectSubSystem(ref PlayerLoopSystem root, Type systemTargetType, ref PlayerLoopSystem systemToInject)
        {
            if (root.subSystemList == null)
                return false;

            for (int i = 0; i < root.subSystemList.Length; i++)
            {
                if (root.subSystemList[i].type == systemTargetType)
                {
                    ArrayUtil.InsertElementByRef(ref root.subSystemList, ref systemToInject, i + 1);
                    return true;
                }

                if (InjectSubSystem(ref root.subSystemList[i], systemTargetType, ref systemToInject))
                    return true;
            }

            return false;
        }

    }
}
