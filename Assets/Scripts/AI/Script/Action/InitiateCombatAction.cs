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
[CreateAssetMenu(menuName = "AIConfig/Action/InitiateCombat")]
public class InitiateCombatAction : Action
{
    public override void Act(StateMachine controller)
    {
        controller.OnCombatInitiate();
    }
}
