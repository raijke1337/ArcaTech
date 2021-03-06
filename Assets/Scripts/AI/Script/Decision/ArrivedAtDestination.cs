using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
[CreateAssetMenu(menuName = "AIConfig/Decision/ArrivedAtDestination")]
public class ArrivedAtDestination : Decision
{
    public override bool Decide(StateMachine controller)
    {
        var result = controller.CheckIsInStoppingRange();
        if (result) controller.OnPatrolPointReached();
        return result;
    }
}

