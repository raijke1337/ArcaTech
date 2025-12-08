using System;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Units;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Usables
{
    public class UsableStrategy : IUsable
    {
        public UsableStrategy(SerializedUsableStrategy config, BaseGameEntityComponent owner, EquipmentComponent equipment)
        {
            GetStateTransition = config.useStateTransition.Build();
            Description = config.description;
            GetCost = config.settings.useCost;
            _owner =  owner;
            _equipment = equipment;
            DrawStrategy = config.settings.drawItemsStrategy;
            _hitProducer = config.hitProducer.Deserialize(owner,equipment);
            _effectApplier = config.effectApplier.Deserialize();
            _results = new List<ActionResult>();
            _maxCharges = config.settings.charges;
            _currentCharges = config.settings.charges;
            _chargeReloadTime = config.settings.chargeReload;
            foreach (var r in config.effects)
            {
                _results.Add(r.Deserialize());
            }
            _hitProducer.Hit += HitProducerOnHit;
        }
        

        private readonly BaseGameEntityComponent _owner;
        private readonly EquipmentComponent _equipment;
        public StateTransition GetStateTransition { get; }
        public StatsEffect GetCost { get; }

        private readonly IHitProducer _hitProducer;
        private readonly IEffectApplier _effectApplier;
        private readonly List<ActionResult> _results;
        
        private readonly int _maxCharges;
        private readonly float _chargeReloadTime;
        private float _reloadTimer;
        private int _currentCharges;

        public float FillValue
        {
            get
            {
                if (_currentCharges < _maxCharges)
                {
                    return _reloadTimer / _chargeReloadTime;
                }
                return 0;
            }
        }
        public string IconNumber => _currentCharges.ToString();
        public bool UsableIsReady()
        {
            return _currentCharges > 0;
        }



        public void DoUpdate(float delta)
        {
            if (_currentCharges < _maxCharges)
            {
                if (_reloadTimer > 0)
                {
                    _reloadTimer -= delta;
                }

                if (_reloadTimer <= 0)
                {
                    _currentCharges++;
                    _currentCharges = Mathf.Clamp(_currentCharges, 0, _maxCharges);
                    _reloadTimer = _chargeReloadTime;
                }
            }
        }

        public void Notify(StateMachineNotifyType notifyType)
        {
            if (notifyType == StateMachineNotifyType.Use)
            {
                _currentCharges--;
            }
            _hitProducer.OnChangeState(notifyType);
        }
        
        private void HitProducerOnHit(TriggerHitInfo hit)
        {
            _effectApplier.ApplyEffects(_owner,hit,_results,_equipment.EffectSpawn.transform.position);
        }
        

        public Description Description { get; }
        public IDrawItemStrategy DrawStrategy { get; }
    }
}