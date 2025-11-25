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
                _results.Add(r.BuildActionResult());
            }
            _hitProducer.Hit += HitProducerOnHit;
        }

        private void HitProducerOnHit(TriggerHitInfo hit)
        {
            if (hit.Target == _owner) return;
            _effectApplier.ApplyEffects(_owner,hit,_results,_equipment.EffectSpawn.transform.position);
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
                    _currentCharges = Mathf.Clamp(_currentCharges ++, 0, _maxCharges);
                    _reloadTimer = _chargeReloadTime;
                }
            }
        }

        private bool _active = false;
        public void StartUse()
        {
            Debug.Log($"Starting use {this.Description.Title}");
            _hitProducer.Initialize();
        }
        
        public void StopUse()
        {
        }
        
        
        public Description Description { get; }
        public float FillValue => _reloadTimer /  _chargeReloadTime;
        public string IconNumber => _currentCharges.ToString();
        public IDrawItemStrategy DrawStrategy { get; }
    }
}