using Arcatech.Stats;
using Arcatech.Triggers;
using CartoonFX;
using UnityEngine;

namespace Arcatech.EventBus
{
    public interface IEvent { }

    #region UI events


    public struct DrawDamageEvent : IEvent
    {
        public EntityStatsComponent Unit { get; }
        public float Damage { get; }
        public DrawDamageEvent (EntityStatsComponent unit, float damage)
        {
            Unit = unit; this.Damage = damage;
        }
    }

    #endregion

    public struct PauseToggleEvent : IEvent
    {
        public bool Value { get; }
        public PauseToggleEvent (bool value) => Value = value;
    }



}