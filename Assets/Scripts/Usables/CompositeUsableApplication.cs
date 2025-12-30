using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.Effects;
using Arcatech.Items;
using Arcatech.Units;
using CartoonFX;
using UnityEngine;
using Arcatech.EventBus;
namespace Arcatech.Usables
{
    [Serializable]
    public struct CompositeUsableApplicationSerialized
    {
        [Header("Hit producer")]
        [SerializeField] public SerializedHitProducer hitProducer;
        
        [Header("Apply effects of hits")]
        [SerializeField] public SerializedEffectApplier effectApplier;
        
        [Header("The effects to apply")]
        [SerializeField] public SerializedActionResult[] effects;

        [Header("Visual")]
        [SerializeField] public CFXR_Effect onValidHit;
        [SerializeField] public CFXR_Effect onInvalidHit;
        
        public CompositeUsableApplication Deserialize(BaseGameEntityComponent owner, EquipmentComponent equipment)
        {
            return new CompositeUsableApplication(owner, equipment, this);
        }
    }

    public class CompositeUsableApplication : IStateMachineNotificationReceiver
    {
        private readonly BaseGameEntityComponent _owner;
        private readonly EquipmentComponent _equipment;
        private IHitProducer _hitProducer;
        private IEffectApplier _effectApplier;
        private List<ActionResult> _results;

       // private ParticlesEvent _particlesEventValidHit;
        private ParticlesEvent _particlesEventInvalidHit;
        CFXR_Effect _effect;
        
        public CompositeUsableApplication(BaseGameEntityComponent owner, EquipmentComponent equipment,
            CompositeUsableApplicationSerialized config)
        {
            _hitProducer = config.hitProducer.Deserialize(owner, equipment);
            _effectApplier = config.effectApplier.Deserialize();
            _results = config.effects.Select(t => t.Deserialize()).ToList();
            _equipment = equipment;
            _owner =  owner;
           // _particlesEventValidHit = new ParticlesEvent(new CFXR_Effect[]{config.onValidHit});
          //  _particlesEventValidHit.Parent = equipment.EffectSpawn;
             _effect = config.onValidHit;
            _particlesEventInvalidHit = new ParticlesEvent(new []{config.onInvalidHit});
            _particlesEventInvalidHit.Parent = equipment.EffectSpawn;
            
            _hitProducer.Hit += Hit;
        }

        private void Hit(TriggerHitInfo info)
        {
           if (_owner.ShowingDebugs) Debug.Log(info.IsValidHit? "Valid hit" : "Invalid hit");
            
            if (info.IsValidHit)
            {
                _effectApplier.ApplyEffects(_owner,info,_results,_equipment.EffectSpawn.transform.position, _effect);
            }

            // moved into applier so particles are placed on the targets
            // if (info.IsValidHit && info.Target!= _owner)
            // {
            //     _particlesEventValidHit.Place = info.Position;
            //     EventBus<ParticlesEvent>.Raise(_particlesEventValidHit);
            // }
            
            else
            {
                _particlesEventInvalidHit.Place = info.Position;
                EventBus<ParticlesEvent>.Raise(_particlesEventInvalidHit);
            }
        }


        
        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
            _hitProducer.OnChangeState(notifyType);
        }

        public void Clear()
        {
            _hitProducer.Hit -= Hit;
        }
    }
}