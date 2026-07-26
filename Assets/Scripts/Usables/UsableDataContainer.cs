using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.Audio;
using Arcatech.Effects;
using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Usables
{
    [Serializable]
    public struct UsableDataContainer
    {
        [Header("Hit producer")]
        [SerializeField] public SerializedHitProducer hitProducer;
        public bool hitboxWarning;

        [SerializeField] public bool proceedOnSelfHit; // continue the logic into applier on self hit (probably should stick to false) 
        // struct start
        [Header("Apply effects of hits")]
        [SerializeField] public SerializedEffectApplier effectApplier;
        [SerializeField] public CFXR_Effect applicationEffect;
        [SerializeField] public SoundDefinition applicationSound;
        
        [Header("The effects to apply")]
        [SerializeField] public SerializedActionResult[] effects;
        // struct end
        // TODO!
        
        
        //Broken by switch to fixed trigger area?
        [Header("Visual")]
        [SerializeField] public CFXR_Effect onInvalidHit;
        
        public CompositeUsableApplication Deserialize(BaseGameEntityComponent owner, EquipmentComponent equipment)
        {
            return new CompositeUsableApplication(owner, equipment, this);
        }
    }

    public class CompositeUsableApplication : IUsableComponent
    {
        private readonly BaseGameEntityComponent _owner;
        private readonly EquipmentComponent _equipment;
        private readonly IHitProducer _hitProducer;
        private readonly IEffectApplier _effectApplier;
        private readonly List<ActionResult> _results;
        private readonly bool _proceedOnSelfHit;
        protected readonly bool indicateHitbox;
        private SoundDefinition _sound;

        private ParticlesEvent _particlesEventInvalidHit;
        
        public CompositeUsableApplication(BaseGameEntityComponent owner, EquipmentComponent equipment,
            UsableDataContainer config)
        {
            
            indicateHitbox = config.hitboxWarning;
            
            _hitProducer = config.hitProducer.Deserialize(owner, equipment,indicateHitbox);
            _effectApplier = config.effectApplier.Deserialize(config.applicationEffect);
            _results = config.effects.Select(t => t.Deserialize()).ToList();
            _equipment = equipment;
            _owner =  owner;
            _particlesEventInvalidHit = new ParticlesEvent(config.onInvalidHit)
            {
                Parent = equipment.EffectSpawn
            };
            _proceedOnSelfHit = config.proceedOnSelfHit;
            
            _hitProducer.EntityHit += HandleEntityHit;
            _hitProducer.EnvironmentHit += HandleEnvironmentHit;
            _sound = config.applicationSound;
        }

        private void HandleEnvironmentHit(TriggerHitInfo arg0)
        {
            if (_owner.ShowingDebugs) Debug.Log("Invalid hit");
            _particlesEventInvalidHit.Place = arg0.Position;
            EventBus<ParticlesEvent>.Raise(_particlesEventInvalidHit);
        }

        private void HandleEntityHit(TriggerHitInfo info)
        {
           if (_owner.ShowingDebugs) Debug.Log("Valid hit");
           // do not call application for self hit (happens because of collider issues)
           if (info.TryGetEntityTarget(out var e) && e == _owner && !_proceedOnSelfHit) return;
           AudioEvents.Play(_sound);
           _effectApplier.ApplyEffects(_owner,info,_results,_equipment.EffectSpawn.transform.position);
        }

        
        public void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            _hitProducer.OnChangeUsableState(notifyType);
        }

        public void Clear()
        {
            _hitProducer.EntityHit -= HandleEntityHit;
            _hitProducer.EnvironmentHit -= HandleEnvironmentHit;
        }
    }
}