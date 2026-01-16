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
            
            _reload = config.settings.charge.Deserialize();
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

        private readonly CompositeUsableApplication[] _usableEffects;

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
            Debug.Log($"{this.Description.Title} state {notifyType}");
            foreach (var effect in _usableEffects)
            {
                effect.StateMachineNotification(notifyType);
            }
            _reload.StateMachineNotification(notifyType);
            _equipment.StateMachineNotification(notifyType);
        }
    }
}