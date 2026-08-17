using System.Collections.Generic;
using System.Linq;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    public interface IStatusReceiver : IEffectReceiver
    {
        void ApplyStun(EffectKey key, float stunSeconds);
        void ClearStun(EffectKey key);
        bool IsStunned { get; }
    }

    /// <summary>
    /// Tracks status states (stun for now). The 'stunned' flag is read by
    /// movement / AI / input. Stun windows have their own timers; the longest
    /// active window keeps the entity stunned.
    /// </summary>
    public class EntityStatusComponent : MonoBehaviour, IStatusReceiver, IPausableComponent, IKillableComponent
    {
        private readonly Dictionary<EffectKey, float> _stunUntil = new(); // key -> world time end
        public bool Paused { get; set; }
        private bool _killed;

        public bool IsStunned { get; private set; }
        private List <IStunnable> _stunnables = new();

        public void ApplyStun(EffectKey key, float stunSeconds)
        {
            if (_killed) return;
            float end = Time.time + stunSeconds;
            // a repeating tick extends/overwrites this key's window with the latest
            if (!_stunUntil.TryGetValue(key, out var cur) || end > cur)
                _stunUntil[key] = end;
            RecomputeStunned();
        }

        public void ClearStun(EffectKey key)
        {
            if (_stunUntil.Remove(key)) RecomputeStunned();
        }

        private void Update()
        {
            if (_killed || Paused || _stunUntil.Count == 0) return;

            float now = Time.time;
            bool changed = false;
            // remove expired windows
            var toRemove = _tmp; toRemove.Clear();
            foreach (var kv in _stunUntil)
                if (now >= kv.Value) toRemove.Add(kv.Key);
            for (int i = 0; i < toRemove.Count; i++) { _stunUntil.Remove(toRemove[i]); changed = true; }

            if (changed) RecomputeStunned();
        }
        private readonly List<EffectKey> _tmp = new();

        private void RecomputeStunned()
        {
            bool any = _stunUntil.Count > 0;
            if (any != IsStunned)
            {
                IsStunned = any;
                foreach (var s in _stunnables)
                {
                    s.Stunned = IsStunned;
                }
            }
        }

        private void Start()
        {
            _stunnables = GetComponentsInChildren<IStunnable>().ToList();
        }

        public void SetKilled(IKillerComponent c, bool value)
        {
            _killed = value;
            if (value) { _stunUntil.Clear(); RecomputeStunned(); }
        }
    }
}