using System.Linq;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
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
            IconDrawType = config.settings.charge.drawType;
            _owner =  owner;
            _equipment = equipment;
            DrawStrategy = config.settings.drawItemsStrategy;
            
            _usableEffects = config.compositeUsableEffects.Select(t=>
                t.Deserialize(owner, equipment)).ToArray();
            
            //
            // _hitProducer = config.hitProducer.Deserialize(owner,equipment);
            // _effectApplier = config.effectApplier.Deserialize();
            // _results = new List<ActionResult>();            
            // foreach (var r in config.effects)
            // {
            //     _results.Add(r.Deserialize());
            // }
            // _hitProducer.Hit += HitProducerOnHit;
            // this now happens inside composite effect
            
            _reload = config.settings.charge.Deserialize();
            _hasTrail = _equipment.Trail;

        }
        

        private readonly BaseGameEntityComponent _owner;
        private readonly EquipmentComponent _equipment;
        public bool UsableIsReady()
        {
            var ok = _reload.Ready;
            return ok;
        }

        public StateTransition GetStateTransition { get; }
        public AppliedStatsDeltaEffect GetCost { get; }

        
        private readonly IReloadStrategy _reload;
        public Description Description { get; }
        public ActionIconDrawType IconDrawType { get; }
        public float FillValue => _reload.FillValue;
        public string StringInfo => _reload.DisplayText;
        public IDrawItemStrategy DrawStrategy { get; }

        private bool _hasTrail;
        private readonly CompositeUsableApplication[] _usableEffects;
        
        // private readonly IHitProducer _hitProducer;
        // private readonly IEffectApplier _effectApplier;
        // private readonly List<ActionResult> _results;
        //
        
        
        public void DoUpdate(float delta)
        {
            _reload.Tick(delta);
        }

        public void CleanUp()
        {
            foreach (var effect in _usableEffects)
            {
                effect.Clear();
            }
        }
        public void Notify(StateMachineNotifyType notifyType)
        {

            foreach (var effect in _usableEffects)
            {
                effect.StateMachineNotification(notifyType);
            }
            //  _hitProducer.OnChangeState(notifyType);
            _reload.StateMachineNotification(notifyType);
            
            switch (notifyType)
            {
                
                case StateMachineNotifyType.Starting:
                {
                    if (_hasTrail) _equipment.Trail.Begin();
                    break;
                }
                case StateMachineNotifyType.EndUse:
                {
                    if (_hasTrail) _equipment.Trail.End();
                    break;
                }

            }
        }
    }
}