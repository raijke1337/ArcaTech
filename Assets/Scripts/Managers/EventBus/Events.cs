using Arcatech.Stats;
using Arcatech.Triggers;
using CartoonFX;
using UnityEngine;

namespace Arcatech.EventBus
{
    public interface IEvent { }

    #region UI events

    public record BaseEntityMouseOverEvent : IEvent
    {
        public ITargetable Target { get; set; }
        public bool IsSelected { get; set; }
        public override string ToString()
        {
            return $"{Target.GetEntity.GetName} {(IsSelected? "selected" : "deselected")}";
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
        public StatsEffectTriggerEvent(BaseGameEntityComponent target, StatsEffect toApply, Transform place, BaseGameEntityComponent source)
        {
            Target = target;
            Applied = toApply;
            Place = place;
            Source = source;    
        }

        public BaseGameEntityComponent Target { get; }
        public StatsEffect Applied { get; }
        public Transform Place { get; }
        public BaseGameEntityComponent Source { get; }  
        public override string ToString()
        {
            return string.Concat(Applied," on ", Target?.GetName, " at ", Place.position);
        }
    }

    public struct PauseToggleEvent : IEvent
    {
        public bool Value { get; }
        public PauseToggleEvent (bool value) => Value = value;
    }



}