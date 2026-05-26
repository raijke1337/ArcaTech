namespace Arcatech.EventBus
{
    public interface IEvent { }

    public struct PauseToggleEvent : IEvent
    {
        public bool Value { get; }
        public PauseToggleEvent (bool value) => Value = value;
    }



}