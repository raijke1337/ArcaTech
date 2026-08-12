using System.Collections;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.UI;
using Arcatech.Units;
using Arcatech.Units.Control;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Stats
{
    [RequireComponent(typeof(EntityStatsComponent), typeof(UsablesCasterComponent))]
    public class TailsOverchargeModule : ValidatedMonoBehaviour, IUnitCommandPerformer, IStatUpdatesViewer, IStateAugmentor, IStateMachineNotificationReceiver
    {
        #region Configuration

        [SerializeField, Self] private EntityStatsComponent statsComponent;
        [SerializeField, Self] private BaseGameEntityComponent entity;

        [Header("Overcharge Settings")]
        [Tooltip("Длительность баффа перегрузки (сек). Должна совпадать с реальной длительностью баффа в стейте.")]
        [SerializeField] private float overchargeBuffDuration = 5f;

        [SerializeField] private SerializedStateTransition overChargeEnter;
        [SerializeField] private SerializedActionResult[] energySpendEffects;

        [Header("Thresholds")]
        [Tooltip("Уровень энергии, ниже которого трата дает лишь короткий бафф")]
        [SerializeField] private float overchargeLevelThreshold = 50f;

        [Tooltip("Доля максимальной энергии, которую нужно потратить в окне для срабатывания перегрузки")]
        [SerializeField, Range(0f, 1f)] private float overchargeSpendFraction = 0.7f;

        [Tooltip("Длительность окна отслеживания резкого расхода энергии (сек)")]
        [SerializeField] private float overchargeTimeWindow = 3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        #endregion

        #region Runtime State

        private StateTransition _transition;
        private StateMachineContext _cachedContext;
        private ActionResult[] _onSpendEffects;

        // Pending actions
        private bool _pendingShortBuff;
        private bool _pendingOverchargeTrigger;

        // Energy tracking window
        private bool _windowActive;
        private float _windowStartTime;
        private float _windowSpentAccumulator;

        // Activation state
        private bool _isInActivationPhase;
        private bool _isOverchargeActive;
        private Coroutine _overchargeDurationRoutine;

        #endregion

        #region UI State

        private float _lastEnergyCurrent;
        private float _lastEnergyMax;
        private bool _isReadyForOvercharge;
        private OverchargeModuleState _currentState;

        public event UnityAction<OverchargeUISnapshot> OnUIUpdate = delegate { };

        #endregion

        #region Lifecycle

        private void Start()
        {
            InitializeEffects();
            statsComponent.RegisterStatsViewer(this);
            UpdateState();
        }

        private void Update()
        {
            if (!enabled) return;

            // Проверяем истечение окна траты в реальном времени
            if (_windowActive && IsWindowExpired())
            {
                LogDebug("Window expired by timeout");
                _windowActive = false;
                UpdateState();
            }
            else if (_windowActive)
            {
                // Обновляем UI для плавного изменения timeRemaining
                UpdateState();
            }
        }

        private void OnDisable()
        {
            ResetAllState();
        }

        #endregion

        #region Initialization

        private void InitializeEffects()
        {
            _onSpendEffects = new ActionResult[energySpendEffects.Length];
            for (int i = 0; i < energySpendEffects.Length; i++)
            {
                _onSpendEffects[i] = energySpendEffects[i].Deserialize();
            }
        }

        #endregion

        #region Energy Tracking

        public void PrepareCommand(UnitCommand command)
        {
            if (!enabled) return;

            _pendingShortBuff = false;
            _pendingOverchargeTrigger = false;

            LogDebug($"PrepareCommand: type={command}");
        }

        public void HandleStatsUpdate(ResourceStatType stat, float current, float max, float delta,
            EntityStatsComponent.ExpendType changeType, BaseGameEntityComponent source)
        {
            if (!enabled) return;

            // Обновляем кэш энергии для UI (любой апдейт Energy важен)
            if (stat == ResourceStatType.Energy)
            {
                CacheEnergyValues(current, max);
                _isReadyForOvercharge = current >= overchargeLevelThreshold;
                UpdateState();
            }

            // Обрабатываем только трату энергии на способности от нашего entity
            if (!IsRelevantEnergySpend(source, stat, changeType))
                return;

            HandleAbilityEnergySpent(current, max, delta);
        }

        private bool IsRelevantEnergySpend(BaseGameEntityComponent source, ResourceStatType stat,
            EntityStatsComponent.ExpendType changeType)
        {
            return source == entity &&
                   stat == ResourceStatType.Energy &&
                   changeType == EntityStatsComponent.ExpendType.UsableCost;
        }

        private void HandleAbilityEnergySpent(float current, float max, float delta)
        {
            float energyBeforeSpend = current - delta;
            float spentThisTick = -delta;

            LogDebug($"Energy spend: before={energyBeforeSpend}, after={current}, max={max}");

            // Проверяем и сбрасываем истекшее окно
            if (_windowActive && IsWindowExpired())
            {
                LogDebug("Window expired before new spend");
                _windowActive = false;
            }

            // Если окно не активно - решаем, начинать его или выдать короткий бафф
            if (!_windowActive)
            {
                if (energyBeforeSpend < overchargeLevelThreshold)
                {
                    LogDebug($"Below threshold ({overchargeLevelThreshold}) -> short buff pending");
                    _pendingShortBuff = true;
                    UpdateState();
                    return;
                }

                StartTrackingWindow();
            }

            // Копим трату в активном окне
            _windowSpentAccumulator += spentThisTick;
            LogDebug($"Spent in window: {_windowSpentAccumulator} / needed {max * overchargeSpendFraction}");

            // Проверяем достижение порога перегрузки
            if (_windowSpentAccumulator >= max * overchargeSpendFraction)
            {
                LogDebug("Overcharge threshold reached");
                _pendingOverchargeTrigger = true;
                _windowActive = false;
            }

            UpdateState();
        }

        private bool IsWindowExpired()
        {
            return Time.time - _windowStartTime > overchargeTimeWindow;
        }

        private void StartTrackingWindow()
        {
            _windowActive = true;
            _windowStartTime = Time.time;
            _windowSpentAccumulator = 0f;
            LogDebug("Tracking window started");
            UpdateState();
        }

        private void CacheEnergyValues(float current, float max)
        {
            _lastEnergyCurrent = current;
            _lastEnergyMax = max;
        }

        #endregion

        #region Command Execution

        public void DoUnitCommand(UnitCommand command, bool wasSuccessful)
        {
            if (!enabled) return;

            LogDebug($"DoUnitCommand: type={command}, success={wasSuccessful}, pendingBuff={_pendingShortBuff}, pendingOvercharge={_pendingOverchargeTrigger}");

            // Короткий бафф применяется сразу при успешном запуске
            if (wasSuccessful && _pendingShortBuff)
            {
                ApplyShortBuff();
            }

            _pendingShortBuff = false;
            UpdateState();
        }

        [ProButton]
        public void ApplyShortBuff()
        {
            LogDebug($"Applying short buff, effects count={_onSpendEffects.Length}");

            foreach (var effect in _onSpendEffects)
            {
                effect.ProduceResult(entity, entity, entity.EffectSpawn.position, entity.EffectSpawn.rotation);
            }

            NotifyUI();
        }

        #endregion

        #region Overcharge Management

        [ProButton]
        private void TriggerOvercharge()
        {
            LogDebug("Overcharge triggered!");

            // Фаза Activation: продлится до выхода из стейта Overcharge (конец анимации).
            // Корутину баффа здесь НЕ запускаем.
            _isInActivationPhase = true;
            SetOverchargeTriggerPending(true);
            UpdateState();
        }
        private IEnumerator EndOverchargeAfterDuration()
        {
            yield return new WaitForSeconds(overchargeBuffDuration);

            LogDebug("Overcharge duration ended");

            _isOverchargeActive = false;
            _overchargeDurationRoutine = null;
            UpdateState();
        }
        private void StartOverchargeBuff()
        {
            LogDebug("Activation finished -> buff started");

            _isInActivationPhase = false;
            _isOverchargeActive = true;

            StopExistingDurationRoutine();
            _overchargeDurationRoutine = StartCoroutine(EndOverchargeAfterDuration());
            UpdateState();
        }
        
        private void StopExistingDurationRoutine()
        {
            if (_overchargeDurationRoutine != null)
            {
                StopCoroutine(_overchargeDurationRoutine);
                _overchargeDurationRoutine = null;
            }
        }

        private void SetOverchargeTriggerPending(bool value)
        {
            if (_cachedContext != null)
            {
                _cachedContext.OverchargeTriggerPending = value;
                LogDebug($"OverchargeTriggerPending -> {value}");
            }
        }

        private void ResetAllState()
        {
            StopExistingDurationRoutine();

            _pendingShortBuff = false;
            _pendingOverchargeTrigger = false;
            _windowActive = false;
            _isReadyForOvercharge = false;
            _isInActivationPhase = false;
            _isOverchargeActive = false;

            if (_cachedContext != null)
            {
                _cachedContext.OverchargeTriggerPending = false;
            }

            UpdateState();
        }

        #endregion

        #region State Machine Integration

        public void SetShieldValue(ResourceStatType stat, float currentValue)
        {
            // No operation
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

            if (IsOverchargeState(state))
            {
                _isInActivationPhase = true; // страховка, если EndUse пришёл раньше входа

                if (context.OverchargeTriggerPending)
                    SetOverchargeTriggerPending(false);
            }

            UpdateState();
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            if (IsOverchargeState(state) && _isInActivationPhase)
            {
                // Выход из стейта активации = конец анимации = начало баффа
                StartOverchargeBuff();
            }

            UpdateState();
        }

        private bool IsOverchargeState(UnitState state)
        {
            return state.StateName == overChargeEnter.nextState.stateDisplayName;
        }

        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
            if (!enabled) return;

            switch (notifyType)
            {
                case StateMachineNotifyType.EndUse:
                    HandleEndUse();
                    break;

                case StateMachineNotifyType.Cancel:
                    HandleCancel();
                    break;
            }
        }

        private void HandleEndUse()
        {
            if (_pendingOverchargeTrigger)
            {
                LogDebug("EndUse -> applying overcharge");
                TriggerOvercharge();
            }

            _pendingOverchargeTrigger = false;
            UpdateState();
        }

        private void HandleCancel()
        {
            if (_pendingOverchargeTrigger)
            {
                LogDebug("Action cancelled - discarding pending overcharge");
            }

            _pendingOverchargeTrigger = false;
            UpdateState();
        }

        #endregion

        #region State Management

        private void UpdateState()
        {
            DetermineCurrentState();
            NotifyUI();
        }

        private void DetermineCurrentState()
        {
            if (_isOverchargeActive)
            {
                _currentState = OverchargeModuleState.Active;
            }
            else if (_isInActivationPhase)
            {
                _currentState = OverchargeModuleState.Activation;
            }
            else if (_pendingOverchargeTrigger || (_isReadyForOvercharge && !_windowActive))
            {
                _currentState = OverchargeModuleState.Ready;
            }
            else if (_windowActive)
            {
                _currentState = OverchargeModuleState.InSpendWindow;
            }
            else
            {
                _currentState = OverchargeModuleState.Idle;
            }
        }

        #endregion

        #region UI Management

        private void NotifyUI()
        {
            OnUIUpdate?.Invoke(CreateUISnapshot());
        }

        private OverchargeUISnapshot CreateUISnapshot()
        {
            float timeRemaining = _windowActive
                ? Mathf.Max(0f, overchargeTimeWindow - (Time.time - _windowStartTime))
                : 0f;

            return new OverchargeUISnapshot(
                currentEnergy: _lastEnergyCurrent,
                maxEnergy: _lastEnergyMax,
                threshold: overchargeLevelThreshold,
                windowSpentEnergy: _windowSpentAccumulator,
                requiredSpentEnergy: _lastEnergyMax * overchargeSpendFraction,
                windowTimeRemaining: timeRemaining,
                windowDuration: overchargeTimeWindow,
                currentState: _currentState,
                totalDuration:overchargeBuffDuration
            );
        }

        #endregion

        #region Debug Logging

        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[{nameof(TailsOverchargeModule)}] {message}");
            }
        }

        #endregion
    }
}