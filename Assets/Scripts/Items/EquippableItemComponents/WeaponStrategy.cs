using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Texts;
using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Items
{

    public class WeaponStrategy : IWeaponUseStrategy,ITriggerNotificationReceiver
    {
        public EntityStateMachineComponent Owner { get; }
        public WeaponSO Config { get; }
        protected EquipmentComponent GameObjectComponent { get; }
        
        protected readonly IWeaponHitSource HitSource;
        public Transform SpawnPoint => GameObjectComponent.EffectSpawn;
        
        public WeaponStrategy (SerializedUnitState act, BaseGameEntityComponent unit, WeaponSO cfg, int charges, float reload, float intcd,EquipmentComponent comp)
        {
            Owner = unit.GetComponent<EntityStateMachineComponent>();
            Config = cfg;
            ChargeReload = reload;
            InternalDelay = intcd;
            MaxCharges = charges;
            GameObjectComponent = comp;
            if (!comp.TryGetComponent(out HitSource))
            {
                Debug.LogWarning($"{comp} has no Hit source!");
            }
            else
            {
                HitSource.GetTriggerNotificationProvider.RegisterReceiver(this);
            }

            InitialState = act.Build();

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
        #region trigger notification
        
        
        public virtual void TriggerEntered(BaseGameEntityComponent enterComponent, ITriggerNotificationProvider trigger)
        {
            Debug.Log("Implement me!");
        }

        public virtual void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
            Debug.Log("Implement me!");
        }
        #endregion
        
        
        #region usable

        protected UnitState InitialState { get; }

        public virtual UnitState UseUsable()
        {
            ChargesLogicOnUse();
            return InitialState;
        }
        
        public virtual void UpdateUsable(float delta)
        {
            foreach (var t in _chargesTimers.ToList()) 
            { 
                t?.Tick(delta); 
            }
            _internalCdTimer?.Tick(delta);
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
        #endregion
        #region UI

        public Description Description => Config.Description;

        public float FillValue
        {
            get
            {
                if (_remainingCharges > 0) return _internalCdTimer.Progress-1;
                else
                {
                    return _chargesTimers.TryPeek(out var p) ? p.Progress : _internalCdTimer.Progress-1;
                }
            }
        }

        public string IconNumber => _remainingCharges.ToString("D");

        #endregion


        public void OnInit()
        {
        }

        public void OnCleanUp()
        {
            HitSource.GetTriggerNotificationProvider.UnregisterReceiver(this);
        }
    }


}