using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using System;
using UnityEngine;
[Serializable]
public abstract class ManagedControllerBase : IManagedController
{
    [SerializeField] public bool DebugMessage = false;
    public ManagedControllerBase(DummyUnit dummyUnit)
    {
        Owner = dummyUnit;
    }

    public DummyUnit Owner { get; }
    public abstract void StartController();
    public abstract void ControllerUpdate(float delta);
    public abstract void FixedControllerUpdate(float fixedDelta);
    
    public abstract void StopController();



}
