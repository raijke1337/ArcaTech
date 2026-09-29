using System;
using UnityEngine;

namespace Arcatech.Usables
{
    /// <summary>
    /// Общий нагрев оружия дальнего боя. Вешается на игрока (у врагов его нет - их это не касается).
    /// Хранит абсолютное значение нагрева; лимиты и скорость остывания берутся
    /// из настроек активного оружия - экипированного в слот применения с OverheatStrategy
    /// (см. UsablesCasterComponent.UpdateWeaponHeat). Оружие в инвентаре не считается.
    /// Сброс из любого кода: WeaponHeatComponent.Find(owner)?.ResetHeat()
    /// </summary>
    public class WeaponHeatComponent : MonoBehaviour
    {
        [Header("Behaviour")]
        [SerializeField, Tooltip("Обнулять нагрев при любой смене оружия с перегревом (в том числе когда оружие снято). " +
                                 "Выключено: нагрев переходит на новое оружие")]
        private bool resetHeatOnWeaponChange = false;

        [Header("Debug")]
        [SerializeField, Tooltip("Подробный лог в консоль: смена оружия, выстрелы, остывание, переходы перегрева, сбросы")]
        private bool debugLog = false;

        [SerializeField, Min(0f), Tooltip("Как часто (сек, реальное время) писать в лог тики остывания. 0 - каждый тик")]
        private float debugCoolLogInterval = 0.25f;

        public float Heat { get; private set; }
        public bool IsOverheated { get; private set; }
        public SerializedOverheatStrategy Active { get; private set; }
        public float CoolDelayLeft => _coolDelayLeft;
        public bool DebugEnabled => debugLog;

        public float MaxHeat => Active != null ? Active.maxHeat : 0f;
        public float Heat01 => MaxHeat > 0f ? Mathf.Clamp01(Heat / MaxHeat) : 0f;

        /// <summary>Для UI-индикатора</summary>
        public event Action Changed;

        private float _coolDelayLeft;
        private float _nextCoolLogTime;

        /// <summary>Ищет компонент нагрева у владельца: сам объект, родители, затем дочерние</summary>
        public static WeaponHeatComponent Find(Component from)
        {
            if (from == null) return null;
            var found = from.GetComponentInParent<WeaponHeatComponent>();
            if (found == null) found = from.GetComponentInChildren<WeaponHeatComponent>();
            return found;
        }

        private void Awake()
        {
            DebugLog($"Awake on '{name}': resetHeatOnWeaponChange={resetHeatOnWeaponChange}, " +
                     $"coolLogInterval={debugCoolLogInterval:F2}s");
        }

        /// <summary>
        /// Задает экипированное оружие с перегревом (null - такого оружия нет).
        /// Его настройки определяют лимит и остывание; от null / не null зависит показ индикатора.
        /// Вызывается при каждой смене экипировки (UsablesCasterComponent).
        /// </summary>
        public void SetActiveWeapon(SerializedOverheatStrategy cfg)
        {
            if (Active == cfg)
            {
                DebugLog($"SetActiveWeapon: no change ({Name(cfg)}), heat={Heat:F1}");
                return;
            }

            var prev = Active;
            Active = cfg;
            DebugLog($"SetActiveWeapon: {Name(prev)} -> {Name(cfg)}; heat={Heat:F1}, overheated={IsOverheated}" +
                     (cfg != null ? $"; {Describe(cfg)}" : ""));

            if (resetHeatOnWeaponChange && (Heat > 0f || IsOverheated))
            {
                DebugLog($"SetActiveWeapon: resetHeatOnWeaponChange -> heat {Heat:F1} => 0");
                Heat = 0f;
                _coolDelayLeft = 0f;
                IsOverheated = false;
            }
            else if (cfg != null && Heat > cfg.maxHeat)
            {
                DebugLog($"SetActiveWeapon: heat {Heat:F1} exceeds new max {cfg.maxHeat:F1}, clamped");
                Heat = cfg.maxHeat;
            }

            if (cfg == null) IsOverheated = false; // оружия нет - блокировать нечего

            Refresh("SetActiveWeapon");
        }

        public void AddHeat(SerializedOverheatStrategy cfg, float amount)
        {
            if (cfg != Active)
            {
                DebugWarn($"AddHeat from NON-ACTIVE weapon {Name(cfg)} (active: {Name(Active)}) - " +
                          "проверьте, что оружие экипировано и UpdateWeaponHeat вызывается");
            }

            float before = Heat;
            Heat = Mathf.Min(cfg.maxHeat, Heat + amount);
            _coolDelayLeft = cfg.coolDelay;

            DebugLog($"AddHeat [{Name(cfg)}]: +{amount:F1}  {before:F1} -> {Heat:F1} / {cfg.maxHeat:F1} " +
                     $"({Heat / cfg.maxHeat:P0}){(before + amount > cfg.maxHeat ? " CAPPED" : "")}; " +
                     $"cool starts in {cfg.coolDelay:F2}s");
            Refresh("AddHeat");
        }

        /// <summary>Вызывается из Tick стратегии. Остывает только активное оружие, чтобы не остывать дважды</summary>
        public void Cool(SerializedOverheatStrategy cfg, float delta)
        {
            if (cfg != Active)
            {
                DebugCoolLog($"Cool ignored: {Name(cfg)} is not active (active: {Name(Active)})");
                return;
            }

            if (Heat <= 0f) return;

            if (_coolDelayLeft > 0f)
            {
                _coolDelayLeft -= delta;
                DebugCoolLog($"Cool waiting: delay left {Mathf.Max(0f, _coolDelayLeft):F2}s, heat={Heat:F1}");
                return;
            }

            float before = Heat;
            Heat = Mathf.Max(0f, Heat - cfg.coolPerSecond * delta);
            DebugCoolLog($"Cool [{Name(cfg)}]: {before:F2} -> {Heat:F2} / {cfg.maxHeat:F1} " +
                         $"(rate {cfg.coolPerSecond:F1}/s, dt {delta:F3}s, overheated={IsOverheated})");
            if (Heat <= 0f) DebugLog("Cool: fully cooled down");

            Refresh("Cool");
        }

        /// <summary>Полный сброс (добивание)</summary>
        public void ResetHeat()
        {
            DebugLog($"ResetHeat: {Heat:F1} => 0 (was overheated: {IsOverheated}, delay left {_coolDelayLeft:F2}s)");
            Heat = 0f;
            _coolDelayLeft = 0f;
            if (Active == null) IsOverheated = false;
            Refresh("ResetHeat");
        }

        /// <summary>Частичное снижение (на будущее: перегрузка и т.п.)</summary>
        public void ReduceHeat(float amount)
        {
            float before = Heat;
            Heat = Mathf.Max(0f, Heat - amount);
            DebugLog($"ReduceHeat: -{amount:F1}  {before:F1} -> {Heat:F1}");
            Refresh("ReduceHeat");
        }

        private void Refresh(string reason)
        {
            bool was = IsOverheated;

            if (Active != null)
            {
                float resumeAt = Active.maxHeat * Active.resumeThreshold;
                if (Heat >= Active.maxHeat) IsOverheated = true;
                else if (IsOverheated && Heat <= resumeAt) IsOverheated = false;
            }

            if (was != IsOverheated)
            {
                DebugLog(IsOverheated
                    ? $"*** OVERHEAT ENTERED ({reason}): heat {Heat:F1} >= max {Active.maxHeat:F1}; " +
                      $"shooting blocked until heat <= {Active.maxHeat * Active.resumeThreshold:F1}"
                    : $"*** OVERHEAT CLEARED ({reason}): heat {Heat:F1}; shooting allowed again");
            }

            Changed?.Invoke();
        }

        #region debug

        private static string Name(SerializedOverheatStrategy cfg) => cfg != null ? cfg.name : "none";

        private static string Describe(SerializedOverheatStrategy c) =>
            $"max={c.maxHeat:F1}, perShot={c.heatPerUse:F1}, cool={c.coolPerSecond:F1}/s after {c.coolDelay:F2}s, " +
            $"resume<={c.maxHeat * c.resumeThreshold:F1} ({c.resumeThreshold:P0}), shotInterval={c.cooldown:F2}s";

        /// <summary>Общий лог нагрева (пишет, только если включена опция debugLog)</summary>
        public void DebugLog(string message)
        {
            if (!debugLog) return;
            Debug.Log($"[WeaponHeat] t={Time.time:F2} f={Time.frameCount} | {message}", this);
        }

        public void DebugWarn(string message)
        {
            if (!debugLog) return;
            Debug.LogWarning($"[WeaponHeat] t={Time.time:F2} f={Time.frameCount} | {message}", this);
        }

        // остывание идет каждый кадр - пишем не чаще заданного интервала
        private void DebugCoolLog(string message)
        {
            if (!debugLog) return;
            if (debugCoolLogInterval > 0f && Time.unscaledTime < _nextCoolLogTime) return;
            _nextCoolLogTime = Time.unscaledTime + debugCoolLogInterval;
            DebugLog(message);
        }

        [ContextMenu("Heat: dump state")]
        private void DumpState()
        {
            // пишет всегда, независимо от debugLog
            Debug.Log($"[WeaponHeat] STATE '{name}': active={Name(Active)}, heat={Heat:F2}/{MaxHeat:F1} ({Heat01:P0}), " +
                      $"overheated={IsOverheated}, coolDelayLeft={_coolDelayLeft:F2}s, " +
                      $"Changed subscribers={(Changed != null ? Changed.GetInvocationList().Length : 0)}" +
                      (Active != null ? $"\n{Describe(Active)}" : ""), this);
        }

        [ContextMenu("Heat: reset (like finisher)")]
        private void DebugReset() => ResetHeat();

        [ContextMenu("Heat: add one shot")]
        private void DebugAddShot()
        {
            if (Active == null)
            {
                Debug.LogWarning("[WeaponHeat] no active weapon, nothing to add", this);
                return;
            }

            AddHeat(Active, Active.heatPerUse);
        }

        #endregion
    }
}
