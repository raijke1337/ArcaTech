﻿using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Skills
{
    public  class SkillUsageStrategy : IUsablesStrategy    , IIconContent
    {
        public BaseEntity Owner {get;protected set;}
        SerializedUnitAction SkillAction { get; }

        protected Transform Spawner;
        readonly ExtendedText _desc;

        int MaxCharges { get; }
        float ChargeReload { get; }
        float InternalDelay { get; }

        Queue<CountDownTimer> _chargesTimers;
        CountDownTimer _internalCdTimer;
        int _remainingCharges;



        public SkillUsageStrategy(BaseEquippableItemComponent item, SerializedUnitAction useaction, BaseEntity unit, ExtendedText desc, int charges, float reload)
        {

            Owner = unit;
            _desc = desc;
            ChargeReload = reload;
            InternalDelay = 0.1f; // placeholder?
            MaxCharges = charges;
            SkillAction = useaction;

            _remainingCharges = MaxCharges;
            _chargesTimers = new Queue<CountDownTimer>(charges);
            _internalCdTimer = new CountDownTimer(InternalDelay);
            _internalCdTimer.Start();
            Spawner = item.Spawner;

        }

        public bool TryUseUsable(out BaseUnitAction action)
        {
            action = SkillAction.ProduceAction(Owner,Spawner);
            if (!_internalCdTimer.IsReady) return false;
            else
            {
                if (_remainingCharges > 0)
                {
                    var t = new CountDownTimer(ChargeReload);
                    _internalCdTimer.Start();
                    t.Start();
                    _chargesTimers.Enqueue(t);
                    _remainingCharges--;
                    t.OnTimerStopped += OnTimerComplete;
                   // DoSkillLogic();
                    return true;
                }
            }
            return false;
        }

        // moved to skill action

        //protected virtual void DoSkillLogic()
        //{
        //    foreach (var r in SkillUsageResults)
        //    {
        //        r.ProduceResult(Owner, null, Spawner.transform);
        //    }
        //}

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

        #region UI
        public Sprite Icon => _desc.Picture;

        public float FillValue => _chargesTimers.TryPeek(out var p)? p.Progress : 0 ;

        public string Text => _remainingCharges > 0 ? "Ready" : "Recharge";

        #endregion

    }
}