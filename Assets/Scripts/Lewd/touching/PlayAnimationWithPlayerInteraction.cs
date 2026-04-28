using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Interactions;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
[RequireComponent(typeof(Animator))]
public class StartPairedAnimationInteraction : ValidatedMonoBehaviour, IActiveInteractionHandler, IStateAugmentor
{
    
    [SerializeField] private SerializedStateTransition stateEntryTransition;  // В InteractStart игрока
    [SerializeField] private SerializedStateTransition stateLoopTransition;    // В InteractLoop игрока
    [SerializeField] private SerializedStateTransition stateSuccessTransition; // В InteractSuccess
    [SerializeField] private SerializedStateTransition stateFailTransition;    // В InteractFail

    // Триггеры для Anim на двери
    [SerializeField] private string startTrigger = "Start";
    [SerializeField] private string loopTrigger = "Loop";
    [SerializeField] private string successTrigger = "Success";
    [SerializeField] private string failTrigger = "Fail";

    private Animator _doorAnimator;
    private IInteractor _currentInteractor;
    private Coroutine _interactionCoroutine;
    private StateTransition _startTransition, _loopTransition, _successTransition, _failTransition;
    private List<StateTransition> _allTransitions = new();

    private void Awake()
    {
        _doorAnimator = GetComponent<Animator>();
        _startTransition = stateEntryTransition.Build();
        _loopTransition = stateLoopTransition?.Build();  // Если нужно отдельный transition в loop
        _successTransition = stateSuccessTransition.Build();
        _failTransition = stateFailTransition.Build();
        _allTransitions.AddRange(new[] { _startTransition, _loopTransition, _successTransition, _failTransition }.Where(t => t != null));
    }

    public void DoInteraction(bool success, IInteractor interactor)
    {
        if (!success) return;  // Неудача уже в condition

        _currentInteractor = interactor;

        // Запустим корутину для всего взаимодействия (стартуем с начала)
        if (_interactionCoroutine != null) StopCoroutine(_interactionCoroutine);
        _interactionCoroutine = StartCoroutine(RunPairedInteraction());
    }

    private IEnumerator RunPairedInteraction()
    {
        if (_currentInteractor == null) yield break;

        // Шаг 1: Запустить анимацию начала на двери и перейти в InteractStart на игроке
        _doorAnimator.SetTrigger(startTrigger);
        yield return new WaitForSeconds(0.1f);  // Синх-синх с crossfade, если нужно

        
        // Переход в InteractStart игрока поскольку выполнено условие ininteraction и interactPending
        

        // Шаг 2: После старта начать луп (одновременно)
        _doorAnimator.SetTrigger(loopTrigger);
        _doorAnimator.SetBool("IsLooping", true);  // Assume loop anim

        // Ожидание результата взаимодействия (мини-игра обновит HasInteractionResult)
        while (!_currentInteractor.InteractionContext.HasInteractionResult(out _))
        {
            yield return null;  // Проверяем каждый кадр
        }

        // Шаг 3: Получить результат QTE и перейти в success/fail
        if (_currentInteractor.InteractionContext.ConsumeInteractionResult(out bool result))
        {
            if (result)
            {
                _doorAnimator.SetTrigger(successTrigger);
                _doorAnimator.SetBool("IsLooping", false);  // Прерывание лупа
                // Переход в InteractSuccess
              // if (fsm.TryCommandTransition(UnitActionType.None, null))  // Или напрямую через conditions
                    TriggerSuccessState();
            }
            else
            {
                _doorAnimator.SetTrigger(failTrigger);
                _doorAnimator.SetBool("IsLooping", false);
                // Переход в InteractFail
                TriggerFailState();
            }
        }

        // Завершение вежливого
       // _currentInteractor.InteractionContext.EntityComponent.GetComponent<StateMachineContext>().InInteractionMode = false;
    }

    private void TriggerSuccessState()
    {
        // Добавьте логику для форсированного перехода в success, через FSM или напрямую
        if (_currentInteractor is InteractionComponent interactComp)
        {
            // Предполагаем, что FSM игрока имеет метод SetState или используйте augmentor
            var fsm = interactComp.GetComponent<EntityStateMachineComponent>();
            fsm.AddTransition(_successTransition);  // Проверьте, работает ли
        }
    }

    private void TriggerFailState()
    {
        // Аналогично для fail
        if (_currentInteractor is InteractionComponent interactComp)
        {
            var fsm = interactComp.GetComponent<EntityStateMachineComponent>();
            fsm.AddTransition(_failTransition);
        }
    }

    #region IStateAugmentor
    public void Attach(IStateAugmentorReceiver machine)
    {
        foreach (var t in _allTransitions)
        {
            machine.AddTransition(t);
        }
    }

    public void Detach(IStateAugmentorReceiver machine)
    {
        foreach (var t in _allTransitions)
        {
            machine.RemoveTransition(t);
        }
    }

    public void OnStateEntered(UnitState state, StateMachineContext context)
    {
        // Логика для StateEntered, если нужно (например, логировать взаимодействие)
        if (state == _startTransition?.NextState)
        {
            Debug.Log("InteractStart entered on player");
        }
    }

    public void OnStateExited(UnitState state, StateMachineContext context)
    {
        // Прерывание, если вышел из состояния (edge case)
        if (_interactionCoroutine != null)
        {
            StopCoroutine(_interactionCoroutine);
            _doorAnimator.SetTrigger(failTrigger);
            _doorAnimator.SetBool("IsLooping", false);
        }
    }
    #endregion
}
}