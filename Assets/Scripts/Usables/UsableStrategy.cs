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
            _reload = config.settings.charge.Deserialize();
            
            foreach (var r in config.effects)
            {
                _results.Add(r.Deserialize());
            }
            _hitProducer.Hit += HitProducerOnHit;
        }
        

        private readonly BaseGameEntityComponent _owner;
        private readonly EquipmentComponent _equipment;
        public bool UsableIsReady()
        {
            return _reload.Ready;
        }

        public StateTransition GetStateTransition { get; }
        public StatsEffect GetCost { get; }

        private readonly IHitProducer _hitProducer;
        private readonly IEffectApplier _effectApplier;
        private readonly List<ActionResult> _results;
        private readonly IReloadStrategy _reload;
        public Description Description { get; }
        public float FillValue => _reload.FillValue;
        public string IconNumber => _reload.DisplayText;
        public IDrawItemStrategy DrawStrategy { get; }

        public void DoUpdate(float delta)
        {
            _reload.Tick(delta);
        }

        public void Notify(StateMachineNotifyType notifyType)
        {
            if (notifyType == StateMachineNotifyType.Use)
            {
                _reload.Use();
            }
            _hitProducer.OnChangeState(notifyType);
        }
        
        private void HitProducerOnHit(TriggerHitInfo hit)
        {
            _effectApplier.ApplyEffects(_owner,hit,_results,_equipment.EffectSpawn.transform.position);
        }
        


    }
}