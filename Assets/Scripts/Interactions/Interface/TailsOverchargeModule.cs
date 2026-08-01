using System;
using System.Collections;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.UI;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{
    [RequireComponent(typeof(EntityStatsComponent), typeof(UsablesCasterComponent))]
    public class TailsOverchargeModule : ValidatedMonoBehaviour, IUnitCommandPerformer, IStatUpdatesViewer, IStateAugmentor,IStateMachineNotificationReceiver
    {
        [SerializeField,Self] EntityStatsComponent statsComponent;
        [Header("Overcharge!")]
        [Tooltip("Длительность баффа перегрузки (сек.), который накладывает сам стейт. " +
                 "Выставляется вручную и должна совпадать с реальной длительностью баффа.")]
        [SerializeField] private float overchargeBuffDuration = 5f;
        [SerializeField] private SerializedStateTransition overChargeEnter;
        // Overcharge effects are applied in the State
        private StateTransition _transition;

        [SerializeField] private SerializedActionResult[] energySpendEffects;
        // visual
        private ActionResult[] _onSpend;

        [Header("Overcharge thresholds")]
        [Tooltip("Уровень энергии, ниже которого трата энергии дает лишь короткий бафф")]
        [SerializeField] private float overchargeLevelThreshold = 50f;

        [Tooltip("Какая доля максимального запаса энергии должна быть потрачена в пределах окна, чтобы сработала перегрузка")]
        [SerializeField, Range(0f, 1f)] private float overchargeSpendFraction = 0.7f;

        [Tooltip("Длительность окна отслеживания резкого расхода энергии, сек.")]
        [SerializeField] private float overchargeTimeWindow = 3f;

        [SerializeField, Self] private BaseGameEntityComponent entity;

        public bool OverChargeReady { get; private set; } = false;
        private StateMachineContext _cachedContext;

        // выставляется в HandleStatsUpdate, применяется в DoUnitCommand — но только если команда успешна
        private bool _pendingShortBuff;
        private bool _pendingOverchargeTrigger;

        // отслеживание "резкого расхода" энергии выше границы перегрузки
        private bool _windowActive;
        private float _windowStartTime;
        private float _windowSpentAccumulator;
        private Coroutine _overchargeDurationRoutine;
        
        [SerializeField] private bool ShowingDebugs = false;

        private void Start()
        {
            _onSpend = new ActionResult[energySpendEffects.Length];
            for (int i = 0; i < energySpendEffects.Length; i++)
            {
                _onSpend[i] = energySpendEffects[i].Deserialize();
            }
            statsComponent.RegisterStatsViewer((this));
        }

        public void PrepareCommand(UnitActionType type)
        {
            if (!enabled) return;

            _pendingShortBuff = false;
            _pendingOverchargeTrigger = false;

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] PrepareCommand: type={type}");
        }

        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta,
            EntityStatsComponent.ExpendType changeType, BaseGameEntityComponent source)
        {
            if (!enabled) return;

            // Кэшируем текущее/максимальное значение энергии для UI независимо от того,
            // чья это трата (в отличие от логики перегрузки, тут важен любой Energy-апдейт,
            // включая пассивный реген).
            if (stat == ResourceStatType.Energy)
            {
                _lastEnergyCurrent = statCurrent;
                _lastEnergyMax = statMax;
            }

            if (source != entity || stat != ResourceStatType.Energy || changeType != EntityStatsComponent.ExpendType.UsableCost)
            {
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Stats update ignored: stat={stat}, changeType={changeType}, source={source}");
                return;
            }

            HandleAbilityEnergySpent(statCurrent, statMax, statDelta);
        }

        private void HandleAbilityEnergySpent(float statCurrent, float statMax, float statDelta)
        {
            float energyBeforeSpend = statCurrent - statDelta;
            float spentThisTick = -statDelta; // statDelta отрицательный

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Ability energy spend: before={energyBeforeSpend}, after={statCurrent}, max={statMax}");

            // если окно было активно, но истекло по времени - гасим его перед новой проверкой
            if (_windowActive && Time.time - _windowStartTime > overchargeTimeWindow)
            {
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge window expired by timeout");
                _windowActive = false;
            }

            if (!_windowActive)
            {
                // Окно не активно - решаем, начинать копить перегрузку или выдать короткий бафф.
                if (energyBeforeSpend < overchargeLevelThreshold)
                {
                    if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Below overcharge threshold ({overchargeLevelThreshold}) -> short buff pending");
                    _pendingShortBuff = true;
                    return;
                }

                _windowActive = true;
                _windowStartTime = Time.time;
                _windowSpentAccumulator = 0f;
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge tracking window started");
            }

            // Окно активно (только что начато или продолжается) - копим трату,
            // даже если текущий каст стартовал уже ниже порога (это ожидаемо для "рывка").
            _windowSpentAccumulator += spentThisTick;

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Spent in window: {_windowSpentAccumulator} / needed {statMax * overchargeSpendFraction}");

            if (_windowSpentAccumulator >= statMax * overchargeSpendFraction)
            {
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge threshold reached -> overcharge pending");
                _pendingOverchargeTrigger = true;
                _windowActive = false;
            }
        }

        public void DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (!enabled) return;
            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] DoUnitCommand: type={type}, success={wasSuccessful}, pendingBuff={_pendingShortBuff}, pendingOvercharge={_pendingOverchargeTrigger}");

            // wasSuccessful == true означает только "действие успешно запущено".
            // Короткий бафф мгновенный - применяем сразу.
            // Перегрузку применять здесь нельзя - она ждёт StateMachineNotification/EndUse.
            if (wasSuccessful && _pendingShortBuff)
            {
                ApplyShortBuff();
            }

            _pendingShortBuff = false;
            // _pendingOverchargeTrigger НЕ трогаем - его обрабатывает StateMachineNotification
        }

        [ProButton]
        public void ApplyShortBuff()
        {
            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Applying short buff, effects count={_onSpend.Length}");
            for (int i = 0; i < _onSpend.Length; i++)
            {
                _onSpend[i].ProduceResult(entity, entity, entity.EffectSpawn.position, entity.EffectSpawn.rotation);
            }
        }

        [ProButton]
        private void TriggerOvercharge()
        {
            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge triggered!");

            OverChargeReady = true;
            SetOverchargeTriggerPending(true);

            if (_overchargeDurationRoutine != null)
            {
                StopCoroutine(_overchargeDurationRoutine);
            }
            _overchargeDurationRoutine = StartCoroutine(EndOverchargeAfterDuration());
        }
        private IEnumerator EndOverchargeAfterDuration()
        {
            yield return new WaitForSeconds(overchargeBuffDuration);

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge buff duration ended -> OverChargeReady = false");

            OverChargeReady = false;
            _overchargeDurationRoutine = null;
        }

        public void SetShieldValue(ResourceStatType stat, float currentValue)
        {
            // NOOP
        }

        public void Attach(IStateAugmentorReceiver machine)
        {
            _transition ??= overChargeEnter.Build();
            machine.AddTransition(_transition);
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            machine.RemoveTransition(_transition);
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
            _cachedContext ??= context;

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] OnStateEntered: state={state}, triggerPending={context.OverchargeTriggerPending}");

            // Переход в стейт перегрузки состоялся - гасим одноразовый триггер,
            // чтобы условие не продолжало матчиться каждый кадр.
            if (state.StateName == overChargeEnter.nextState.stateDisplayName && context.OverchargeTriggerPending)
            {
                SetOverchargeTriggerPending(false);
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge trigger consumed");
            }
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        { }
        private void SetOverchargeTriggerPending(bool value)
        {
            _cachedContext.OverchargeTriggerPending = value;
            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] OverchargeTriggerPending -> {value}");
        }


        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
            if (!enabled) return;

            switch (notifyType)
            {
                case StateMachineNotifyType.NoNotify:
                    break;
                case StateMachineNotifyType.Starting:
                    break;
                case StateMachineNotifyType.Use:
                    break;
                case StateMachineNotifyType.EndUse:
                    if (_pendingOverchargeTrigger)
                    {
                        if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] EndUse -> applying overcharge");
                        TriggerOvercharge();
                    }
                    _pendingOverchargeTrigger = false;
                    break;
                case StateMachineNotifyType.Cancel:
                    if (_pendingOverchargeTrigger && ShowingDebugs)
                        Debug.Log($"[{nameof(TailsOverchargeModule)}] Action cancelled - discarding pending overcharge");
                    _pendingOverchargeTrigger = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(notifyType), notifyType, null);
            }
        }
        private void OnDisable()
        {
            // На случай отключения компонента посреди активного баффа/окна - убираем "зависший" таймер
            // и не оставляем персонажа навечно в состоянии перегрузки.
            if (_overchargeDurationRoutine != null)
            {
                StopCoroutine(_overchargeDurationRoutine);
                _overchargeDurationRoutine = null;
            }

            _pendingShortBuff = false;
            _pendingOverchargeTrigger = false;
            _windowActive = false;

            if (OverChargeReady)
            {
                _cachedContext.OverchargeTriggerPending = false;
                OverChargeReady = false;
            }
        }
        
        
        #region UI
        private float _lastEnergyCurrent;
        private float _lastEnergyMax;
        
        public OverchargeUISnapshot GetUISnapshot()
        {
            float windowTimeRemaining = 0f;
            if (_windowActive)
            {
                windowTimeRemaining = Mathf.Max(0f, overchargeTimeWindow - (Time.time - _windowStartTime));
            }

            // "Ready" - бафф перегрузки сейчас реально действует (гейм-эффект, длится overchargeBuffDuration).
            // "Active" - стейт-машина прямо сейчас находится в визуальном стейте перегрузки
            //            (может закончиться раньше, чем сам бафф - если анимация короче баффа).
            bool isOverchargeActive = _cachedContext != null && _cachedContext.CurrentState.StateName == _transition.NextState.StateName;

            return new OverchargeUISnapshot(
                currentEnergy: _lastEnergyCurrent,
                maxEnergy: _lastEnergyMax,
                threshold: overchargeLevelThreshold,
                isWindowActive: _windowActive,
                windowSpentEnergy: _windowSpentAccumulator,
                requiredSpentEnergy: _lastEnergyMax * overchargeSpendFraction,
                windowTimeRemaining: windowTimeRemaining,
                windowDuration: overchargeTimeWindow,
                isOverchargeReady: OverChargeReady,
                isOverchargeActive: isOverchargeActive);
        }
        #endregion
    }
}