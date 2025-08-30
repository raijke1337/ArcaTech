using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.UI;
using Arcatech.Units;
using Arcatech.Units.Inputs;
using CartoonFX;
using UnityEngine;

namespace Arcatech.EventBus
{
    public interface IEvent { }

    #region UI events

    public struct PlayerTargetUpdateEvent : IEvent
    {
        public PlayerTargetUpdateEvent(ITargetable target)
        {
            Target = target;
        }
        public override string ToString()
        {
            if (Target == null)
            {
                return $"no target";
            }
            else return $"{Target.UnitName}";
            
        }
        public ITargetable Target { get; }

    }

    public struct PlayerStatsChangedUIEvent : IEvent
    {
        public BaseStatType StatType { get; }
        public StatValueContainer Container { get; }
        public PlayerStatsChangedUIEvent (BaseStatType statType, StatValueContainer container)
        { 
             StatType = statType; Container = container;
        }
    }


    public struct DrawDamageEvent : IEvent
    {
        public BaseGameEntityComponent Unit { get; }
        public float Damage { get; }
        public DrawDamageEvent (BaseGameEntityComponent unit, float damage)
        {
            Unit = unit; this.Damage = damage;
        }
    }

    #endregion
    public struct StatsEffectTriggerEvent : IEvent
    {
        public StatsEffectTriggerEvent(BaseGameEntityComponent target, StatsEffect toApply, Transform place)
        {
            Target = target;
            Applied = toApply;
            Place = place;
        }

        public BaseGameEntityComponent Target { get; }
        public StatsEffect Applied { get; }
        public Transform Place { get; }
        public override string ToString()
        {
            return string.Concat(Applied," on ", Target?.GetName, " at ", Place.position);
        }
    }
    public struct VFXEvent : IEvent
    {
        public CFXR_Effect Effect;
        public Transform Place;

        public VFXEvent(CFXR_Effect effect, Transform place)
        {
            Effect = effect;
            Place = place;
        }
    }

    public struct PauseToggleEvent : IEvent
    {
        public bool Value { get; }
        public PauseToggleEvent (bool value) => Value = value;
    }



}