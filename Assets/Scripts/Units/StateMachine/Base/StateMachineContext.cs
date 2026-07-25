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
        public bool InInteraction { get; set; } = false;
        public bool OverchargeState { get; set; }
        public void ClearCommand()
        {
            PendingCommand = UnitActionType.None;
            InInteraction = false;
        }
    }

    /// <summary>
    /// ecchi code part
    /// </summary>
    public partial class StateMachineContext
    {
        private LewdnessContext _lewdnessContext;
        public void InitEcchiContext(LewdnessSettings cfg) =>
            _lewdnessContext = new LewdnessContext(cfg,Animator);

        [CanBeNull]
        public LewdnessContext EcchiContext => _lewdnessContext;
        
    }
}