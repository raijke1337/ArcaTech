using Arcatech.Stats;
using Arcatech.Triggers;
using CartoonFX;
using UnityEngine;

namespace Arcatech.EventBus
{
    public interface IEvent { }

    public struct PauseToggleEvent : IEvent
    {
        public bool Value { get; }
        public PauseToggleEvent (bool value) => Value = value;
    }



}