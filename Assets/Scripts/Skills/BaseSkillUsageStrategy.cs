using Arcatech.Items;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Skills
{
    public class SkillUsageStrategy : IUsableStrategy , IIconContent
    {
        public EntityStateMachineComponent Owner {get;protected set;}
        SerializedUnitState SkillState { get; }

        protected Transform Spawner;
        readonly Description _desc;

        int MaxCharges { get; }
        float ChargeReload { get; }
        float InternalDelay { get; }

        Queue<CountDownTimer> _chargesTimers;
        CountDownTimer _internalCdTimer;
        int _remainingCharges;



        public SkillUsageStrategy(EquipmentComponent item, SerializedUnitState useaction, EntityStateMachineComponent unit, Description desc, int charges, float reload)
        {
            OnInit();

            Owner = unit;
            _desc = desc;
            ChargeReload = reload;
            InternalDelay = 0.1f; // placeholder?
            MaxCharges = charges;
            SkillState = useaction;

            _remainingCharges = MaxCharges;
            _chargesTimers = new Queue<CountDownTimer>(charges);
            _internalCdTimer = new CountDownTimer(InternalDelay);
            _internalCdTimer.Start();
            Spawner = item.EffectSpawn;

        }

        public UnitState UseUsable()
        {
            if (_remainingCharges > 0)
            {
                var t = new CountDownTimer(ChargeReload);
                _internalCdTimer.Start();
                t.Start();
                _chargesTimers.Enqueue(t);
                _remainingCharges--;
                t.OnTimerStopped += OnTimerComplete;
            }

            return SkillState.Build();
        }
        public virtual void UpdateUsable(float delta)
        {
            foreach (var t in _chargesTimers.ToList())
            {
                t?.Tick(delta);
            }
            _internalCdTimer?.Tick(delta);
        }

        void OnTimerComplete()
        {
            _chargesTimers.Dequeue();
            _remainingCharges++;
            Mathf.Clamp(_remainingCharges, 0, MaxCharges); // just in case
        }

        public bool CanUseUsable()
        {
            if (!_internalCdTimer.IsReady) return false;
            else
            {
                if (_remainingCharges > 0)
                {
                    return true;
                }
            }
            return false;
        }

        #region UI

        public Description Description => _desc;

        public float FillValue
        {
            get
            {
                if (_remainingCharges > 0) return _internalCdTimer.Progress - 1;
                else
                {
                    return _chargesTimers.TryPeek(out var p) ? p.Progress : _internalCdTimer.Progress - 1;
                }
            }
        }

        public string IconNumber => _remainingCharges > 0 ? "OK" : "CHARGING";



        #endregion

        public void OnInit()
        {
        }

        public void OnCleanUp()
        {
            throw new System.NotImplementedException();
        }
    }
}