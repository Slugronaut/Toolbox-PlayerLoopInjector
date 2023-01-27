using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Toolbox;
using Toolbox.AutoCreate;
using Toolbox.UpdateSystem;


public class PlayerLoopTests
{
    public class MockEmptyClass { }

    [AutoCreate]
    public class MockUpdatable : IUpdatable<MockUpdateSystem>
    {
        public bool ShutdownCalled { get; private set; }
        public bool InitCalled { get; private set; }
        public int UpdateTickCount { get; private set; }

        public void OnShutdown(IUpdateSystem system)
        {
            ShutdownCalled = true;
        }

        public void OnSystemInit(IUpdateSystem system)
        {
            InitCalled = true;
        }

        public void Update()
        {
            UpdateTickCount++;
        }
    }

    public class MockUpdateSystem : IUpdateSystem
    {
        public string SystemName => throw new System.NotImplementedException();

        public void LateUpdate()
        {
        }

        public void OnAppEnd()
        {
        }

        public void PreUpdate()
        {
        }

        public void RegisterUpdatable(ILoopUpdatable updateable, PlayerLoopTiming timing, int priority = 0)
        {
        }

        public void UnregisterUpdateable(ILoopUpdatable updateable)
        {
        }

        public void Update()
        {
        }
    }

    public class MockUpdateSystem2 : IUpdateSystem
    {
        public string SystemName => throw new System.NotImplementedException();

        public void LateUpdate()
        {
        }

        public void OnAppEnd()
        {
        }

        public void PreUpdate()
        {
        }

        public void RegisterUpdatable(ILoopUpdatable updateable, PlayerLoopTiming timing, int priority = 0)
        {
        }

        public void UnregisterUpdateable(ILoopUpdatable updateable)
        {
        }

        public void Update()
        {
        }
    }

    public class MockUpdateSystem3 : MockUpdateSystem { }

    

    [Test]
    public void UpdateSystemInterfaceTypesFound()
    {
        var updateSystemTypes = PlayerLoopInjector.FindAllUpdateSystemTypes();
        Assert.IsNotNull(updateSystemTypes);
        Assert.GreaterOrEqual(updateSystemTypes.Length, 1);
        Assert.Contains(typeof(MockUpdateSystem), updateSystemTypes);
    }

    [Test]
    public void UpdatableTypesCaughtInvalidSystemArg()
    {
        var autoCreatables = AutoCreator.InstantiateAllAutoCreateables().Select(x => x.Value);
        try
        {
            PlayerLoopInjector.FilterUpdatableTypes(typeof(MockEmptyClass), autoCreatables);
            Assert.Fail("Filter method did not throw appropriate exception.");
        }
        catch (ArgumentException)
        {
            Assert.Pass();
        }
    }

    [Test]
    public void UpdatableTypesAllowValidSystemArg()
    {
        var autoCreatables = AutoCreator.InstantiateAllAutoCreateables().Select(x => x.Value);
        try
        {
            PlayerLoopInjector.FilterUpdatableTypes(typeof(MockUpdateSystem), autoCreatables);
        }
        catch (ArgumentException)
        {
            Assert.Fail("Filter method did not accept a valid argument.");
        }
    }

    [Test]
    public void UpdatableTypesFound()
    {
        var autoCreatables = AutoCreator.InstantiateAllAutoCreateables().Select(x => x.Value).ToList();
        List<ILoopUpdatable> updatables = null;
        try
        {
            updatables = PlayerLoopInjector.FilterUpdatableTypes(typeof(MockUpdateSystem), autoCreatables).ToList();
        }
        catch(ArgumentException)
        {
            Assert.Fail("Filter method did not acceopt a valid argument.");
        }

        Assert.NotNull(updatables);
        Assert.GreaterOrEqual(updatables.Count, 1);
        Assert.Contains(typeof(MockUpdatable), updatables.Select(x => x.GetType()).ToList());
    }

    [Test]
    public void IncompatibleUpdatableTypesSkipped()
    {
        var autoCreatables = AutoCreator.InstantiateAllAutoCreateables().Select(x => x.Value).ToList();
        List<ILoopUpdatable> updatables = null;
        try
        {
            updatables = PlayerLoopInjector.FilterUpdatableTypes(typeof(MockUpdateSystem2), autoCreatables).ToList();
        }
        catch (ArgumentException)
        {
            Assert.Fail("Filter method did not acceopt a valid argument.");
        }

        Assert.NotNull(updatables);
        Assert.AreEqual(0, updatables.Count);
        Assert.IsFalse(updatables.Select(x => x.GetType()).ToList().Contains(typeof(MockUpdatable)));
    }

}
