using Arcatech.Stats;
using Arcatech.Units.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Arcatech.Units
{
    public class StateMachineContext
    {
        public UnitActionType PendingCommand;
        public Transform Spawn;
        public UnitState CurrentState;
        public BaseGameEntityComponent Owner;
        [CanBeNull] public EntityStatsComponent Stats;
        
        public IMove[] Movers;
        public IAim[] Aimers;
        public IInvulnerability[] Invulnerables;
        public void ClearCommand ()=> PendingCommand = UnitActionType.None;
    }
}