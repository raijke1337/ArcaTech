using System;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Queue input action", story: "[Wrapper] uses [inputAction], wait [Timer]", category: "Action/Game", id: "ad61333688a7a94462659bcbe4546dfd")]
public partial class QueueAction : Action
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<UnitActionType> InputAction;
    [SerializeReference] public BlackboardVariable<float> Timer;

    private float m_Timer;
    private bool qd;
    protected override Status OnStart()
    {
        m_Timer = 0;
        qd = false;
        if (Wrapper.Value == null || !Wrapper.Value.ActionAvailable(InputAction.Value)) return Status.Failure;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!qd)
        {
            Wrapper.Value.RequestAction(InputAction.Value);
            qd = true;
        }

        m_Timer += Time.deltaTime;
        if (m_Timer >= Timer.Value) return Status.Success;
        return Status.Running;
    }
}

