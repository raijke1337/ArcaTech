using System.Collections.Generic;
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

        private AnimatorOverrideController _aoc;
        private readonly List<KeyValuePair<AnimationClip, AnimationClip>> _overrides = new();
        private readonly Dictionary<AnimationClip, int> _indexByClip = new();
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

            RuntimeAnimatorController source = animator.runtimeAnimatorController;
            if (source == null)
            {
                Debug.LogWarning($"[{name}] у аниматора нет контроллера.", this);
                return;
            }

            // Override-контроллер нельзя вкладывать в другой override-контроллер.
            if (source is AnimatorOverrideController existing) source = existing.runtimeAnimatorController;

            // Свой экземпляр на каждого юнита: общий ассет не трогаем.
            _aoc = new AnimatorOverrideController(source) { name = source.name + " (stances)" };
            animator.runtimeAnimatorController = _aoc;

            _aoc.GetOverrides(_overrides);
            for (int i = 0; i < _overrides.Count; i++)
                _indexByClip[_overrides[i].Key] = i;

            foreach (var entry in slotMap.Entries)
            {
                if (entry.baseClip != null && !_indexByClip.ContainsKey(entry.baseClip))
                {
                    Debug.LogWarning(
                        $"[{name}] слот {entry.slot}: клипа '{entry.baseClip.name}' нет в контроллере '{source.name}'. " +
                        "Проверьте StanceSlotMap.", this);
                }
            }

            _initialized = true;
        }

        /// <summary>
        /// null - стойка без оружия (оригинальные клипы).
        /// </summary>
        public void SetStance(WeaponStanceSO stance)
        {
            Initialize();
            if (!_initialized || stance == _current) return;
            _current = stance;

            // Сбрасываем всё к оригиналам, затем применяем слоты стойки.
            for (int i = 0; i < _overrides.Count; i++)
                _overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(_overrides[i].Key, null);

            if (stance != null)
            {
                foreach (var entry in slotMap.Entries)
                {
                    if (entry.baseClip == null) continue;
                    if (!stance.TryGetClip(entry.slot, out AnimationClip replacement)) continue;
                    if (!_indexByClip.TryGetValue(entry.baseClip, out int index)) continue;

                    _overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(entry.baseClip, replacement);
                }
            }

            _aoc.ApplyOverrides(_overrides);   // одним пакетом
        }
    }
}
