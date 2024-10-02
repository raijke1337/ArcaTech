﻿using Arcatech.Actions;
using Arcatech.AI;
using Arcatech.UI;
using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Items
{

    public class WeaponStrategy : IWeaponUseStrategy
    {
        public BaseEntity Owner { get; }
        public WeaponSO Config { get; }
        protected BaseWeaponComponent WeaponComponent { get; }


        public WeaponStrategy (SerializedUnitAction act,EquippedUnit unit, WeaponSO cfg, int charges, float reload, float intcd,BaseWeaponComponent comp)
        {
            Owner = unit;
            Config = cfg;
            ChargeReload = reload;
            InternalDelay = intcd;
            MaxCharges = charges;
            WeaponComponent = comp;

            Action = act.ProduceAction(unit,comp.Spawner);

            _remainingCharges = MaxCharges;
            _chargesTimers = new Queue<CountDownTimer>(charges);
            _internalCdTimer = new CountDownTimer(InternalDelay);
            _internalCdTimer.Start();
        }


        // charges
        #region charges and cds
        protected int MaxCharges { get; }
        protected float ChargeReload { get; }
        protected float InternalDelay { get; }

        Queue<CountDownTimer> _chargesTimers;
        CountDownTimer _internalCdTimer;
        protected int _remainingCharges { get; private set; }
        protected bool CheckTimersAndCharges()
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
        private void ReplenishCharge()
        {
            _chargesTimers.Peek().OnTimerStopped -= ReplenishCharge;
            _chargesTimers.Dequeue();
            _remainingCharges++;
        }
        protected void ChargesLogicOnUse()
        {
            var t = new CountDownTimer(ChargeReload);
            _internalCdTimer.Start();
            t.Start();
            _chargesTimers.Enqueue(t);
            _remainingCharges--;
            t.OnTimerStopped += ReplenishCharge;
        }


        #endregion
        #region usable

        protected BaseUnitAction Action { get; }

        public virtual bool TryUseUsable(out BaseUnitAction action)
        {
            action = Action;
            if (CheckTimersAndCharges())
            {
                ChargesLogicOnUse(); return true;
            }
            else return false;
        }
        
        public virtual void UpdateUsable(float delta)
        {
            foreach (var t in _chargesTimers.ToList()) 
            { 
                t?.Tick(delta); 
            }
            _internalCdTimer?.Tick(delta);
        }
        #endregion
        #region UI
        public Sprite Icon => Config.Description.Picture;

        public float FillValue => _internalCdTimer.Progress;

        public string Text => _remainingCharges > 0 ? "Ready" : "Wait";


        #endregion
    }


}