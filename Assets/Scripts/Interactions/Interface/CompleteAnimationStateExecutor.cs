using System;
using Arcatech;
using Arcatech.Interactions;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractionStatesInjectorComponent))]
public class CompleteAnimationStateExecutor : InteractionExecutor
{
    private InteractionStatesInjectorComponent comp;
    private UnityAction<InteractionState> action;
    [SerializeField, ReadOnlyText] private string waitForCompletionStateName;

    private void OnValidate()
    {
        GetComponent<InteractionStatesInjectorComponent>()
            .TryGetTransitionByInteractionState(InteractionState.InProgress, out var transition);
        waitForCompletionStateName = transition.nextState.animatorStateName;
    }

    private void Awake()
    {
        comp = GetComponent<InteractionStatesInjectorComponent>();
    }

    
    private void OnEnable() => comp.StateAnimationExited.AddListener(HandleEvent);

    private void OnDisable()
    {
        comp.StateAnimationExited.RemoveListener(HandleEvent);
        action = null; // не оставляем висящий колбэк
    }

    private void HandleEvent(InteractionState exitedState)
    {
        if (action == null) return;                             // ещё не запущен / уже завершён
        if (exitedState != InteractionState.InProgress) return; // ждём именно InProgress

        var cb = action;
        action = null;                       // one-shot: сначала очистка, потом вызов
        cb.Invoke(InteractionState.Success); // терминальный статус для пайплайна
    }

    public override void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete)
    {
        action = onComplete;
    }

    public override void Cancel(InteractionContext ctx) // если в базе есть virtual Cancel
    {
        action = null;
    }
}