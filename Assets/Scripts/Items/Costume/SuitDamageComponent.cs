using System;
using System.Collections.Generic;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    public enum SuitDamageStage
    {
        Intact = 0,    // Целый
        Damaged = 1,   // Повреждённый
        Destroyed = 2  // Разрушенный
    }

    /// <summary>
    /// Хранит стадию повреждения костюма персонажа (диздок NSFW-механики, "Повреждение костюма").
    /// Визуал (куски брони) подписывается на StageChanged через EquipmentCostumeDamageVisuals.
    /// Эффекты стадий накладываются через обычный пайплайн эффектов (EffectFactory -> EntityEffectController).
    /// Источники повреждений (провал QTE, телеграфированные атаки) вызывают Damage(); ремонтные станции — Repair().
    /// </summary>
    [RequireComponent(typeof(EffectsReceiverComponent))]
    public class SuitDamageComponent : ValidatedMonoBehaviour, IKillableComponent
    {
        public const int MaxStageIndex = (int)SuitDamageStage.Destroyed;

        [SerializeField, Self] private BaseGameEntityComponent entity;
        [SerializeField, Self] private EffectsReceiverComponent receiver;

        [SerializeField] private SuitDamageStage startingStage = SuitDamageStage.Intact;

        [Header("Эффекты стадий (заменяют друг друга, не суммируются)")]
        [Tooltip("Эффекты должны быть с infiniteDuration = true, иначе истекут сами по таймеру.")]
        [SerializeField] private BaseAppliedEffect[] intactEffects;
        [SerializeField] private BaseAppliedEffect[] damagedEffects;
        [SerializeField] private BaseAppliedEffect[] destroyedEffects;

        private readonly EffectFactory _factory = new();
        private readonly List<ActiveEffectInstance> _live = new();

        private SuitDamageStage _stage;
        private bool _started;
        private bool _reapplyPending;

        public SuitDamageStage Stage => _stage;

        /// <summary> (предыдущая стадия, новая стадия) </summary>
        public event Action<SuitDamageStage, SuitDamageStage> StageChanged = delegate { };

        private void Awake()
        {
            // Экипировка костюма может произойти раньше Start, поэтому стартовую стадию выставляем здесь
            // и сообщаем подписчикам, если она отличается от дефолтной.
            SetStageInternal(startingStage);
        }

        private void Start()
        {
            _started = true;
            ApplyStageEffects(_stage);
        }

        private void Update()
        {
            // после воскрешения контроллер эффектов мог быть очищен — накладываем эффекты стадии заново
            if (!_reapplyPending) return;
            _reapplyPending = false;
            ApplyStageEffects(_stage);
        }

        private void OnDestroy()
        {
            ClearLiveEffects();
        }

        #region public api

        /// <summary> +steps ступеней повреждения. Возвращает true, если стадия изменилась. </summary>
        public bool Damage(int steps = 1) => SetStage((SuitDamageStage)Mathf.Clamp(_stage.AsInt() + Mathf.Max(0, steps), 0, MaxStageIndex));

        /// <summary> Ремонтная станция: -steps ступеней. </summary>
        public bool Repair(int steps = 1) => SetStage((SuitDamageStage)Mathf.Clamp(_stage.AsInt() - Mathf.Max(0, steps), 0, MaxStageIndex));

        /// <summary> Полный ремонт (мастерская / конец биома). </summary>
        public bool FullRepair() => SetStage(SuitDamageStage.Intact);

        /// <summary> Переход между уровнями: «Разрушенный» автоматически возвращается в «Повреждённый». </summary>
        public bool RestoreBetweenLevels() => _stage == SuitDamageStage.Destroyed && SetStage(SuitDamageStage.Damaged);

        public bool SetStage(SuitDamageStage stage)
        {
            if (stage == _stage) return false;
            SetStageInternal(stage);
            if (_started) ApplyStageEffects(_stage);
            return true;
        }

        #endregion

        private void SetStageInternal(SuitDamageStage stage)
        {
            var previous = _stage;
            _stage = stage;
            if (previous != stage) StageChanged.Invoke(previous, stage);
        }

        private BaseAppliedEffect[] EffectsFor(SuitDamageStage stage) => stage switch
        {
            SuitDamageStage.Intact => intactEffects,
            SuitDamageStage.Damaged => damagedEffects,
            SuitDamageStage.Destroyed => destroyedEffects,
            _ => null
        };

        private void ApplyStageEffects(SuitDamageStage stage)
        {
            ClearLiveEffects();

            var defs = EffectsFor(stage);
            if (defs == null || defs.Length == 0) return;

            var controller = receiver.Controller;
            var pos = transform.position;
            foreach (var def in defs)
            {
                if (def == null) continue;
                var instance = _factory.Create(def, entity);
                controller.AddEffect(instance, entity, receiver, pos, Quaternion.identity);
                _live.Add(instance); // если стакер отклонил эффект, RemoveEffect его просто проигнорирует
            }
        }

        private void ClearLiveEffects()
        {
            if (_live.Count == 0) return;
            var controller = receiver != null ? receiver.Controller : null;
            if (controller != null)
                foreach (var instance in _live)
                    controller.RemoveEffect(instance);
            _live.Clear();
        }

        // Смерть очищает контроллер эффектов сама; при воскрешении порядок вызовов компонентов не определён,
        // поэтому перенакладываем эффекты в следующем Update.
        public void SetKilled(IKillerComponent component, bool value)
        {
            if (value)
                _live.Clear();
            else
                _reapplyPending = true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Validate(intactEffects, nameof(intactEffects));
            Validate(damagedEffects, nameof(damagedEffects));
            Validate(destroyedEffects, nameof(destroyedEffects));
        }

        private void Validate(BaseAppliedEffect[] defs, string label)
        {
            if (defs == null) return;
            foreach (var d in defs)
                if (d != null && !d.infiniteDuration)
                    Debug.LogWarning($"{nameof(SuitDamageComponent)}: эффект '{d.name}' ({label}) не бесконечный — " +
                                     "он истечёт по таймеру, не дожидаясь смены стадии.", this);
        }
    }

    internal static class SuitDamageStageExtensions
    {
        public static int AsInt(this SuitDamageStage stage) => (int)stage;
    }
}
