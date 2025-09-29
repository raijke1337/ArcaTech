using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/EnterCombatEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "EnterCombatEventChannel", message: "Combat state is [value]", category: "Events", id: "423327d376e33f6735f67bd800016dbc")]
public partial class EnterCombatEventChannel : EventChannelBase
{
    public delegate void EnterCombatEventChannelEventHandler(bool value);
    public event EnterCombatEventChannelEventHandler Event; 

    public void SendEventMessage(bool value)
    {
        Event?.Invoke(value);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<bool> valueBlackboardVariable = messageData[0] as BlackboardVariable<bool>;
        var value = valueBlackboardVariable != null ? valueBlackboardVariable.Value : default(bool);

        Event?.Invoke(value);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        EnterCombatEventChannelEventHandler del = (value) =>
        {
            BlackboardVariable<bool> var0 = vars[0] as BlackboardVariable<bool>;
            if(var0 != null)
                var0.Value = value;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as EnterCombatEventChannelEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as EnterCombatEventChannelEventHandler;
    }
}

