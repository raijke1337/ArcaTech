using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using System;
using UnityEngine;
[Serializable]
public abstract class ManagedControllerBase : IManagedController
{
    [SerializeField] public bool DebugMessage = false;
    public ManagedControllerBase(BaseEntity dummyUnit)
    {
        Owner = dummyUnit;
    }

    public BaseEntity Owner { get; }
    public abstract void StartController();
    public abstract void ControllerUpdate(float delta);
    public abstract void FixedControllerUpdate(float fixedDelta);
    
    public abstract void StopController();



}
