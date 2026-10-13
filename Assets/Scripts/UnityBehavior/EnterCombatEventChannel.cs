using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/EnterCombatEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "EnterCombatEventChannel", message: "Combat state is [value] for [group]", category: "Events", id: "423327d376e33f6735f67bd800016dbc")]
public partial class EnterCombatEventChannel : EventChannelBase
{
    public delegate void EnterCombatEventChannelEventHandler(bool value, string group);
    public event EnterCombatEventChannelEventHandler Event; 

    public void SendEventMessage(bool value,string group)
    {
        Event?.Invoke(value,group);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<bool> valueBlackboardVariable = messageData[0] as BlackboardVariable<bool>;
        var value = valueBlackboardVariable?.Value ?? false;

        BlackboardVariable<string> valueBlackboardVariable2 = messageData[1] as BlackboardVariable<string>;
        var value2 = valueBlackboardVariable2?.Value;
        
        Event?.Invoke(value,value2);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        EnterCombatEventChannelEventHandler del = (value,value2) =>
        {
            if (vars[0] is BlackboardVariable<bool> var0 && vars[1] is BlackboardVariable<string> var1)
            {
                var0.Value = value;
                var1.Value = value2;
            }

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

