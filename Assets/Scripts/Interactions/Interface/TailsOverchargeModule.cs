using System;
using System.Collections;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.UI;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

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
        private bool _isActivationAnimating = false;
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

        public bool OverchargeEngaged { get; private set; } = false;
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
            
            UpdateUIAndState();
        }
        
        private void Update()
        {
            bool windowExpired = false;

            // 1. Проверяем истечение окна траты энергии в реальном времени
            if (_windowActive && Time.time - _windowStartTime > overchargeTimeWindow)
            {
                if (ShowingDebugs) 
                    Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge window expired by timeout (Update loop)");
                
                _windowActive = false;
                windowExpired = true;
            }

            // 2. Если окно истекло (чтобы сбросить статус) или оно активно 
            // (для плавного обновления timeRemaining в снапшоте), вызываем апдейт состояния и UI.
            if (windowExpired || _windowActive)
            {
                UpdateUIAndState();
            }
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
                _isReadyForOvercharge = statCurrent >= overchargeLevelThreshold;
                UpdateUIAndState();
            }

            if (source != entity || stat != ResourceStatType.Energy || changeType != EntityStatsComponent.ExpendType.UsableCost)
            {
               // if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Stats update ignored: stat={stat}, changeType={changeType}, source={source}");
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
                UpdateUIAndState();
            }

            if (!_windowActive)
            {
                // Окно не активно - решаем, начинать копить перегрузку или выдать короткий бафф.
                if (energyBeforeSpend < overchargeLevelThreshold)
                {
                    if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Below overcharge threshold ({overchargeLevelThreshold}) -> short buff pending");
                    _pendingShortBuff = true;
                    UpdateUIAndState();
                    return;
                }

                _windowActive = true;
                _windowStartTime = Time.time;
                _windowSpentAccumulator = 0f;
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge tracking window started");
                UpdateUIAndState();
                
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
                UpdateUIAndState();
            }
            else
            {
                UpdateUIAndState();
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
            UpdateUIAndState();
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
            OnUIUpdate?.Invoke(GetUISnapshot());
        }

        [ProButton]
        private void TriggerOvercharge()
        {
            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge triggered!");

            OverchargeEngaged = true;
            SetOverchargeTriggerPending(true);

            if (_overchargeDurationRoutine != null)
            {
                StopCoroutine(_overchargeDurationRoutine);
            }
            _overchargeDurationRoutine = StartCoroutine(EndOverchargeAfterDuration());
            UpdateUIAndState();
        }
        private IEnumerator EndOverchargeAfterDuration()
        {
            yield return new WaitForSeconds(overchargeBuffDuration);

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge buff duration ended -> OverChargeReady = false");

            OverchargeEngaged = false;
            _overchargeDurationRoutine = null;
            UpdateUIAndState();
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

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] OnStateEntered: state={state}");

            // Проверяем имя входящего состояния напрямую по аргументу, а не по контексту
            if (state.StateName == overChargeEnter.nextState.stateDisplayName)
            {
                _isActivationAnimating = true;
        
                // Потребляем триггер, если он был выставлен
                if (context.OverchargeTriggerPending)
                {
                    SetOverchargeTriggerPending(false);
                    if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge trigger consumed on enter");
                }
            }

            UpdateUIAndState();
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            // Сбрасываем флаг только при выходе из конкретного состояния активации
            if (state.StateName == overChargeEnter.nextState.stateDisplayName)
            {
                _isActivationAnimating = false;
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Activation animation state exited");
            }
    
            UpdateUIAndState();
        }
        
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
                    UpdateUIAndState();
                    break;
                case StateMachineNotifyType.Cancel:
                    if (_pendingOverchargeTrigger && ShowingDebugs)
                        Debug.Log($"[{nameof(TailsOverchargeModule)}] Action cancelled - discarding pending overcharge");
                    _pendingOverchargeTrigger = false;
                    UpdateUIAndState();
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
            _isReadyForOvercharge = false;
            if (OverchargeEngaged)
            {
                _cachedContext.OverchargeTriggerPending = false;
                OverchargeEngaged = false;
            }
            UpdateUIAndState();
        }
        
        
        #region UI
        private float _lastEnergyCurrent;
        private float _lastEnergyMax;
        private OverchargeModuleState _state;
        private bool _isReadyForOvercharge = false;
        private void UpdateUIAndState()
        {
            // Приоритет состояний (от высшего к низшему):
            // 1. Active - перегрузка активна
            // 2. Activation - анимация перегрузки
            // 3. Ready - готов к перегрузке
            // 4. InSpendWindow - идет накопление
            // 5. Idle - все остальное

            if (OverchargeEngaged)
            {
                _state = OverchargeModuleState.Active;
            }
            // ИСПРАВЛЕНИЕ: Используем надежный флаг вместо проверки контекста
            else if (_isActivationAnimating)
            {
                _state = OverchargeModuleState.Activation;
            }
            else if (_pendingOverchargeTrigger)
            {
                // Если триггер висит, но мы еще не вошли в стейт активации
                _state = OverchargeModuleState.Ready; 
            }
            else if (_isReadyForOvercharge && !_windowActive)
            {
                _state = OverchargeModuleState.Ready;
            }
            else if (_windowActive)
            {
                _state = OverchargeModuleState.InSpendWindow;
            }
            else
            {
                _state = OverchargeModuleState.Idle;
            }

            OnUIUpdate?.Invoke(GetUISnapshot());
        }
        public event UnityAction<OverchargeUISnapshot> OnUIUpdate = delegate { };

        OverchargeUISnapshot GetUISnapshot()
        {
            float timeRemaining = 0f;
            if (_windowActive)
            {
                timeRemaining = Mathf.Max(0f, overchargeTimeWindow - (Time.time - _windowStartTime));
            }

            return new OverchargeUISnapshot(
                currentEnergy: _lastEnergyCurrent,
                maxEnergy: _lastEnergyMax,
                threshold: overchargeLevelThreshold,
                windowSpentEnergy: _windowSpentAccumulator,
                requiredSpentEnergy: _lastEnergyMax * overchargeSpendFraction,
                windowTimeRemaining: timeRemaining,
                windowDuration: overchargeTimeWindow,
                currentState: _state);
        }
        #endregion
    }
}