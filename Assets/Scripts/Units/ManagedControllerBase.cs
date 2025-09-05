using Arcatech;
using System;
using UnityEngine;
[Serializable]
public abstract class ManagedControllerBase : IManagedController
{
    [SerializeField] public bool DebugMessage = false;
    public ManagedControllerBase(ActiveGameUnitComponent dummyUnit)
    {
        Owner = dummyUnit;
    }

    public ActiveGameUnitComponent Owner { get; }
    public abstract void StartController();
    public abstract void ControllerUpdate(float delta);
    public abstract void FixedControllerUpdate(float fixedDelta);
    
    public abstract void StopController();



}
