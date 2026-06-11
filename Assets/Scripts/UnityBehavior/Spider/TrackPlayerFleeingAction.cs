using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Track player fleeing", story: "Track flee time of [Target] from [Self] into [FleeTimer]", category: "Action/Game/Spider", id: "48ffcb6cf1a4247051f6f94efe68bf81")]
public partial class TrackFleeTimeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> MeleeRadius;
    [SerializeReference] public BlackboardVariable<float> FleeTimer;

    protected override Status OnUpdate()
    {
        var self = Self.Value;  var target = Target.Value;
        if (self == null || target == null) return Status.Running;

        float dist = Vector3.Distance(self.transform.position, target.transform.position);
        if (dist > MeleeRadius.Value) FleeTimer.Value += Time.deltaTime;
        else                           FleeTimer.Value = 0f;

        return Status.Running;   // живёт постоянно
    }
}


