using System;
using Arcatech.Interactions;
using Arcatech.Lewding;
using Arcatech.Stats;
using Arcatech.Units.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Arcatech.Units
{
    [Serializable]
    public partial class StateMachineContext
    {
        public UnitActionType PendingCommand;
        public Transform Spawn;
        public UnitState CurrentState;
        public BaseGameEntityComponent Owner;
        [CanBeNull] public EntityStatsComponent Stats;
        [CanBeNull] public IMove[] Movers;
        [CanBeNull] public IAim[] Aimers;
        [CanBeNull] public IInvulnerability[] Invulnerables;
        [CanBeNull] public IInteractor Interactor;
        public Animator Animator;
        public bool KnockDownState { get; set; }
        public bool DeadState { get; set; }
        public bool InterruptQueued { get; set; }

        public void ClearCommand() => PendingCommand = UnitActionType.None;
    }

    public partial class StateMachineContext
    {
        [CanBeNull] public LewdnessContext EcchiContext;
    }
}