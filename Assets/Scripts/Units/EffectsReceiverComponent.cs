using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// Marks an entity as a valid target for applied effects and exposes the
    /// receiver interfaces. Pulls in the whole effect infrastructure via
    /// RequireComponent so it can never be half-set-up.
    ///
    /// Absence of this component == "effects cannot be applied here"
    /// (e.g. invulnerable decor, pure triggers).
    /// Stats are OPTIONAL: an entity may receive auras/modifiers without stats,
    /// and a stat entity without this component cannot be targeted by effects.
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    [RequireComponent(typeof(EntityEffectController))]
    [RequireComponent(typeof(ModifierAggregator))]
    [RequireComponent(typeof(EntityStatusComponent))]
    public class EffectsReceiverComponent : MonoBehaviour
    {
        private BaseGameEntityComponent _owner;
        private EntityEffectController _controller;
        private IModifierAggregator _modifiers;
        private IStatusReceiver _status;

        // optional — present only if the entity also has stats
        private IStatReceiver _stats;
        private IShieldReceiver _shields;

        public BaseGameEntityComponent Owner => _owner;
        public EntityEffectController Controller => _controller;

        private void Awake()
        {
            // RequireComponent guarantees these exist on the same GameObject.
            TryGetComponent(out _owner);
            TryGetComponent(out _controller);
            _modifiers = GetComponent<IModifierAggregator>();
            _status = GetComponent<IStatusReceiver>();

            // optional stat-based receivers (same component implements both)
            _stats = GetComponent<IStatReceiver>();
            _shields = GetComponent<IShieldReceiver>();
        }

        // --- receiver access (effects ask the receiver, not the entity) ---

        public bool TryGetStatReceiver(out IStatReceiver r)
        {
            r = _stats;
            return r != null;
        }

        public bool TryGetShieldReceiver(out IShieldReceiver r)
        {
            r = _shields;
            return r != null;
        }

        public bool TryGetModifierAggregator(out IModifierAggregator r)
        {
            r = _modifiers;
            return r != null;
        }

        public bool TryGetStatusReceiver(out IStatusReceiver r)
        {
            r = _status;
            return r != null;
        }
    }
}