using Arcatech.Items;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units.Control
{
    /// <summary>
    /// Подменяет клипы idle и бега под оружие в руках через AnimatorOverrideController.
    /// Стейты и переходы машины состояний не меняются: подменяются только клипы внутри них.
    /// Стойку сообщает EntityInventoryDrawerComponent (HeldStanceChanged).
    /// </summary>
    public class StanceClipSwitcher : ValidatedMonoBehaviour
    {
        [SerializeField, Child] private Animator animator;
        [SerializeField, Self] private EntityInventoryDrawerComponent drawer;
        [SerializeField] private StanceSlotMap slotMap;

        private AnimatorClipOverrides _overrides;
        private WeaponStanceSO _current;
        private bool _initialized;

        public WeaponStanceSO CurrentStance => _current;

        private void Awake() => Initialize();

        private void OnEnable()
        {
            Initialize();
            if (drawer == null) return;

            drawer.HeldStanceChanged += SetStance;
            SetStance(drawer.HeldStance);   // событие могло прийти раньше подписки
        }

        private void Start()
        {
            if (drawer != null && !drawer.HandPlacesConfigured)
            {
                Debug.LogWarning(
                    $"[{name}] у EntityInventoryDrawerComponent не заполнен handPlaces: стойка меняться не будет.", this);
            }
        }

        private void OnDisable()
        {
            if (drawer != null) drawer.HeldStanceChanged -= SetStance;
        }

        private void Initialize()
        {
            if (_initialized) return;

            if (animator == null || slotMap == null)
            {
                Debug.LogWarning($"[{name}] не заданы animator или slotMap.", this);
                return;
            }

            _overrides = AnimatorClipOverrides.For(animator);
            _initialized = _overrides != null;
        }

        /// <summary>
        /// null - стойка без оружия (оригинальные клипы).
        /// </summary>
        public void SetStance(WeaponStanceSO stance)
        {
            Initialize();
            if (!_initialized || stance == _current) return;
            _current = stance;

            foreach (var entry in slotMap.Entries)
            {
                AnimationClip replacement = null;
                if (stance != null) stance.TryGetClip(entry.slot, out replacement);
                _overrides.SetClip(entry.baseClip, replacement);
            }
            _overrides.Apply();
        }
    }
}
