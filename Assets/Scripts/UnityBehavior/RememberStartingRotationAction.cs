using System;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Assign scanning variables", story: "[Agent] remembers starting Y into [startRotation]. Calculates min [minRotation] and max [maxRotation] based on Wrapper data View Angle", category: "Action/Game/Turret", id: "740140fad540176f95437bff8b77a9b8")]
public partial class RememberStartingRotationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> startRotation;
    [SerializeReference] public BlackboardVariable<float> minRotation;
    [SerializeReference] public BlackboardVariable<float> maxRotation;
    
    NPCBehaviorWrapper _wrapper;
    private float _viewAngle;
    
    protected override Status OnStart()
    {
        var go = Agent?.Value;
        if (go == null)
        {
            LogFailure("Agent == null"); return Status.Failure;
        }

        if (_wrapper == null && !go.TryGetComponent(out _wrapper))
        {
            LogFailure("Wrapper not found"); return Status.Failure;
        }

        _viewAngle = _wrapper.Config.ViewAngle;
        
        startRotation.Value = Agent.Value.transform.eulerAngles.y;
        maxRotation.Value = startRotation.Value + _viewAngle;
        minRotation.Value = startRotation.Value - _viewAngle;
        return Status.Success;
    }
}

