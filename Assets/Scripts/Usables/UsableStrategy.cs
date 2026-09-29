using System;
using System.Linq;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using Arcatech.Usables.Effects;

namespace Arcatech.Usables
{
    public class UsableStrategy : IUsable
    {
        public UsableStrategy(SerializedUsableStrategy config, BaseGameEntityComponent owner, EquipmentComponent equipment)
        {
            GetActivationTransition = config.useStateTransition?.Build();
            Description = config.description;
            GetCost = config.settings.useCost;
            _owner =  owner;
            _equipment = equipment;
            DrawStrategy = config.settings.drawItemsStrategy;
            
            
            _usableEffects = config.usableData.Select(t=>
                t.Deserialize(owner, equipment)).ToArray();
            
            _reload = config.settings.charge.Deserialize();
            // стратегиям, которым нужен владелец (например, перегрев), сообщаем его
            if (_reload is IOwnerAware ownerAware) ownerAware.SetOwner(owner);
        }

        public Description Description { get; }
        public float Cooldown => _reload.Cooldown;
        public float CurrentCooldown => _reload.CurrentCooldown;
        public int MaxCharges => _reload.MaxCharges;
        public int CurrentCharges => _reload.CurrentCharges;
        public (ResourceStatType, int) GetCostDescription => GetCost.PlaceholderData;
        public AppliedStatsDeltaEffect GetCost { get; }

        private readonly BaseGameEntityComponent _owner;
        private readonly EquipmentComponent _equipment;
        public bool UsableIsReady()
        {
            var ok = _reload.Ready;
            return ok;
        }

        public StateTransition GetActivationTransition { get; }

        /// <summary>Стратегия перезарядки (нужна, чтобы узнать, есть ли у применения перегрев)</summary>
        public IReloadStrategy Reload => _reload;
        
        private readonly CompositeUsableApplication[] _usableEffects;
        private readonly IReloadStrategy _reload;
        public IDrawItemStrategy DrawStrategy { get; }

        public void DoUpdate(float delta)
        {
            _reload.Tick(delta);
        }

        public void CleanUp()
        {
            // стратегии с подписками (перегрев) отписываются от событий
            if (_reload is IDisposable disposable) disposable.Dispose();
            foreach (var effect in _usableEffects)
            {
                effect.Clear();
            }
        }
        public void Notify(StateMachineNotifyType notifyType)
        {
            foreach (var effect in _usableEffects)
            {
                effect.OnChangeUsableState(notifyType);
            }
            _reload.OnChangeUsableState(notifyType);
            _equipment.OnChangeUsableState(notifyType);
        }
    }
}