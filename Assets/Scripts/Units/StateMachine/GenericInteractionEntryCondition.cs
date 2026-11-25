using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Generic interaction entry")]
    public sealed class GenericInteractionEntryCondition : SerializedStateTransitionCondition
    {
        [SerializeField] private bool startsInteraction;
        [SerializeField, Tooltip("Условия входа из обычного Idle или позы рядом с предметом.")]
        private SerializedStateTransitionCondition[] entryConditions = System.Array.Empty<SerializedStateTransitionCondition>();

        public override bool CanTransition(StateMachineContext ctx)
        {
            var interactor = ctx.Interactor;
            if (interactor == null || !interactor.HasValidTarget || ctx.CurrentState == null ||
                ctx.DeadState || ctx.StunnedState) return false;

            bool fromDefault = ctx.CurrentState == ctx.DefaultState;
            var animation = interactor.AnimationState;
            bool entering = fromDefault || animation == InteractionState.Idle;
            if (!entering && !animation.HasValue) return false;

            if (ctx.HasPendingCommand)
            {
                if (!startsInteraction || ctx.PendingActionType != UnitActionType.Use ||
                    interactor.State != InteractionState.Idle || !entering) return false;
            }
            else if (startsInteraction)
            {
                // Автотриггер выставляет Starting без команды Use.
                if (interactor.State != InteractionState.Starting) return false;
            }
            else if (interactor.State == InteractionState.Idle && !fromDefault)
            {
                // Сброс логического статуса в Idle может произойти до конца финального клипа.
                return false;
            }

            if (entering)
                foreach (var condition in entryConditions)
                    if (condition == null || !condition.CanTransition(ctx)) return false;
            return true;
        }
    }
}
