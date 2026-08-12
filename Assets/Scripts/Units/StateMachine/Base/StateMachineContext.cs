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
        public UnitCommand PendingCommand;

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
        public bool InInteraction { get; set; }

        // Разовый сигнал перехода в состояние перегрузки.
        public bool OverchargeTriggerPending { get; set; }

        public bool HasPendingCommand =>
            PendingCommand.Type != UnitActionType.None;

        public UnitActionType PendingActionType =>
            PendingCommand.Type;

        public Vector3 PendingDirection =>
            PendingCommand.Direction;

        public BaseGameEntityComponent PendingTarget =>
            PendingCommand.Target;

        public void SetCommand(UnitCommand command)
        {
            PendingCommand = command;
        }

        public void ClearCommand()
        {
            PendingCommand = UnitCommand.None;
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
            _lewdnessContext = new LewdnessContext(cfg, Animator);

        [CanBeNull] public LewdnessContext EcchiContext => _lewdnessContext;

    }
}