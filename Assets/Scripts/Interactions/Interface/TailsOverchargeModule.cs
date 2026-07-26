using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{
    [RequireComponent(typeof(EntityStatsComponent), typeof(UsablesCasterComponent))]
    public class TailsOverchargeModule : ValidatedMonoBehaviour, IUnitCommandPerformer, IStatUpdatesViewer, IStateAugmentor
    {
        [SerializeField,Self] EntityStatsComponent statsComponent;
        [Header("Overcharge!")]
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

            if (ShowingDebugs)Debug.Log($"[{nameof(TailsOverchargeModule)}] Ability energy spend: before={energyBeforeSpend}, after={statCurrent}, max={statMax}");

            if (energyBeforeSpend < overchargeLevelThreshold)
            {
                if (ShowingDebugs)Debug.Log($"[{nameof(TailsOverchargeModule)}] Below overcharge threshold ({overchargeLevelThreshold}) -> short buff pending");
                _pendingShortBuff = true;
                _windowActive = false;
                return;
            }

            if (!_windowActive || Time.time - _windowStartTime > overchargeTimeWindow)
            {
                _windowActive = true;
                _windowStartTime = Time.time;
                _windowSpentAccumulator = 0f;
                if (ShowingDebugs)Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge tracking window (re)started");
            }

            float spentThisTick = -statDelta; // statDelta отрицательный
            _windowSpentAccumulator += spentThisTick;

            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Spent in window: {_windowSpentAccumulator} / needed {statMax * overchargeSpendFraction}");

            if (_windowSpentAccumulator >= statMax * overchargeSpendFraction)
            {
                if (ShowingDebugs)Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge threshold reached -> overcharge pending");
                _pendingOverchargeTrigger = true;
                _windowActive = false;
            }
        }

        public void DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (!enabled) return;
            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] DoUnitCommand: type={type}, success={wasSuccessful}, pendingBuff={_pendingShortBuff}, pendingOvercharge={_pendingOverchargeTrigger}");

            if (wasSuccessful)
            {
                if (_pendingOverchargeTrigger)
                {
                    TriggerOvercharge();
                }
                else if (_pendingShortBuff)
                {
                    ApplyShortBuff();
                }
            }

            _pendingShortBuff = false;
            _pendingOverchargeTrigger = false;
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
        public void TriggerOvercharge()
        {
            if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge triggered!");
            OverChargeReady = true;
            ToggleOverchargeState(true);
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
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            if (context.OverchargeState)
            {
                ToggleOverchargeState(false);
                OverChargeReady = false;
                if (ShowingDebugs) Debug.Log($"[{nameof(TailsOverchargeModule)}] Overcharge state ended, flags reset");
            }
        }

        private void ToggleOverchargeState(bool state)
        {
            _cachedContext.OverchargeState = state;
            if (ShowingDebugs)  Debug.Log($"[{nameof(TailsOverchargeModule)}] ToggleOverchargeState -> {state}");
        }
    }
}