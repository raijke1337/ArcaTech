using Arcatech.Shaders;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DrawScanningCircle", story: "Draw a scanner circle of radius with [color] using [shader] over [time]", category: "Action/Game/ScannerShaderController", id: "f31d11a506a914a895cfb835a5491aab")]
public partial class DrawScanningCircleAction : Action
{
    [SerializeReference] public BlackboardVariable<Color> Color;
    [SerializeReference] public BlackboardVariable<ScannerShaderController> Shader;
    [SerializeReference] public BlackboardVariable<float> Time;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

