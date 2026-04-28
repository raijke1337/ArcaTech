using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Queue", order = 2)]
    public class SerializedQueueChargesStrategy : SerializedChargesStrategy
    {

        [Min(1)] public int maxCharges = 3;
        [Min(0f)] public float regenTime = 3f;

        public override BasicChargesStrategy Deserialize()
        {
            return new ChargesQueueStrategy(this);
        }
    }

    public class ChargesQueueStrategy : BasicChargesStrategy
    {
        private readonly int _maxCharges;
        private readonly float _regenTime;

        private int _available;

        // Per-slot remaining times; 0 means the slot is available
        private readonly float[] _cooldowns;

        public ChargesQueueStrategy(SerializedQueueChargesStrategy charges) : base(charges)
        {
            _maxCharges = Mathf.Max(1, charges.maxCharges);
            _regenTime = Mathf.Max(0f, charges.regenTime);

            _cooldowns = new float[_maxCharges];
            _available = _maxCharges; // start full
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (_available >= _maxCharges) return;

            for (int i = 0; i < _cooldowns.Length; i++)
            {
                if (_cooldowns[i] > 0f)
                {
                    _cooldowns[i] -= delta;
                    if (_cooldowns[i] <= 0f)
                    {
                        _cooldowns[i] = 0f;
                        _available = Mathf.Min(_available + 1, _maxCharges);
                    }
                }
            }
        }

        protected override bool ReadyCheck()
        {
            return _available > 0 && base.ReadyCheck();
        }

        public override void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            base.OnChangeUsableState(notifyType);

            switch (notifyType)
            {
                case StateMachineNotifyType.Use:
                {
                    if (_available <= 0) break;

                    // Consume one available charge and start its individual cooldown
                    // Choose the first available slot (cooldown == 0)
                    for (int i = 0; i < _cooldowns.Length; i++)
                    {
                        if (_cooldowns[i] <= 0f)
                        {
                            if (_regenTime > 0f)
                            {
                                _cooldowns[i] = _regenTime;
                                _available--;
                            }

                            // If regenTime == 0, the charge refills instantly; effectively no change
                            break;
                        }
                    }

                    break;
                }
            }
        }

        public override float FillValue
        {
            get
            {
                // Only show progress when completely empty; otherwise ready (0 cover)
                if (_available > 0) return 0f;

                // If regen is instant, treat as ready
                if (_regenTime <= 0f) return 0f;

                // Find the soonest finishing charge and show remaining/total
                float minRemaining = float.MaxValue;
                for (int i = 0; i < _cooldowns.Length; i++)
                {
                    float t = _cooldowns[i];
                    if (t > 0f && t < minRemaining) minRemaining = t;
                }

                // Inverted for UI cover: 1 = not ready, 0 = ready
                // When empty: fill = remaining / regenTime
                
                var fill = Mathf.Clamp01(minRemaining / _regenTime);
               // Debug.Log(fill);
                return fill;
            }
        }

        public override string DisplayText => _available.ToString();
    }
}