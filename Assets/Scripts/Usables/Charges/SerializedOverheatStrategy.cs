using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    /// <summary>
    /// Перезарядка-«перегрев» для обычной атаки оружия дальнего боя.
    /// Нагрев общий (см. <see cref="WeaponHeatComponent"/> на игроке), а параметры у каждого оружия свои.
    /// Поле cooldown (из базового класса) - интервал между выстрелами.
    /// Спецатаки перегрев не используют: им ставится обычная стратегия зарядов/кулдауна.
    /// </summary>
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Overheat", order = 3)]
    public class SerializedOverheatStrategy : SerializedGenericCooldownStrategy
    {
        [Header("Нагрев")]
        [Min(1f)] public float maxHeat = 100f;
        [Min(0f)] public float heatPerUse = 12f;

        [Header("Охлаждение")]
        [Min(0f)] public float coolPerSecond = 20f;
        [Tooltip("Сколько секунд после выстрела оружие не остывает")]
        [Min(0f)] public float coolDelay = 0.5f;
        [Tooltip("После перегрева стрельба возобновится, когда нагрев упадет ниже этой доли от максимума")]
        [Range(0f, 1f)] public float resumeThreshold = 0.5f;

        public override BasicChargesStrategy Deserialize() => new OverheatStrategy(this);
    }

    /// <summary>Стратегия, которой при создании нужен владелец (UsableStrategy передает его сам)</summary>
    public interface IOwnerAware
    {
        void SetOwner(BaseGameEntityComponent owner);
    }

    public class OverheatStrategy : BasicChargesStrategy, IOwnerAware
    {
        private readonly SerializedOverheatStrategy _cfg;
        private WeaponHeatComponent _weaponHeat; // null, если у владельца нет нагрева (враги): оружие не перегревается
        private bool _lastReady = true;

        public OverheatStrategy(SerializedOverheatStrategy cfg) : base(cfg)
        {
            _cfg = cfg;
            // Существующий UI показывает заряды: отдаем "запас до перегрева" как заряды
            MaxCharges = Mathf.CeilToInt(cfg.maxHeat);
            SyncView();
        }

        public void SetOwner(BaseGameEntityComponent owner)
        {
            // стратегия создается вместе с предметом (даже неэкипированным), поэтому активным оружие
            // становится не здесь, а в UsablesCasterComponent - по списку экипированных применений
            _weaponHeat = WeaponHeatComponent.Find(owner);
            SyncView();

            if (_weaponHeat != null)
            {
                _weaponHeat.DebugLog($"OverheatStrategy[{_cfg.name}] created for '{owner.name}': heat component found");
            }
            else if (owner != null && owner.CompareTag("Player"))
            {
                Debug.LogWarning($"[WeaponHeat] OverheatStrategy[{_cfg.name}]: у игрока '{owner.name}' нет " +
                                 "WeaponHeatComponent - перегрев отключен", owner);
            }
        }

        public SerializedOverheatStrategy Config => _cfg;

        protected override bool ReadyCheck()
        {
            bool blockedByHeat = _weaponHeat != null && _weaponHeat.IsOverheated;
            bool ready = CurrentCooldown <= 0f && !blockedByHeat;

            // пишем только смену состояния, иначе лог забьется проверками каждый кадр
            if (ready != _lastReady && _weaponHeat != null)
            {
                _weaponHeat.DebugLog($"OverheatStrategy[{_cfg.name}] ready: {_lastReady} -> {ready} " +
                                     $"(shotCooldownLeft={Mathf.Max(0f, CurrentCooldown):F2}s, " +
                                     $"blockedByHeat={blockedByHeat}, heat={_weaponHeat.Heat:F1})");
            }

            _lastReady = ready;
            return ready;
        }

        public override void Tick(float delta)
        {
            base.Tick(delta); // интервал между выстрелами
            if (_weaponHeat != null) _weaponHeat.Cool(_cfg, delta);
            SyncView();
        }

        public override void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            base.OnChangeUsableState(notifyType); // взводит CurrentCooldown
            if (notifyType != StateMachineNotifyType.Use) return;

            if (_weaponHeat != null)
            {
                _weaponHeat.DebugLog($"OverheatStrategy[{_cfg.name}] Use notification -> adding heat");
                _weaponHeat.AddHeat(_cfg, _cfg.heatPerUse);
            }
            SyncView();
        }

        private void SyncView()
        {
            var heat = _weaponHeat != null ? _weaponHeat.Heat : 0f;
            CurrentCharges = Mathf.Max(0, Mathf.RoundToInt(_cfg.maxHeat - heat));
        }
    }
}
