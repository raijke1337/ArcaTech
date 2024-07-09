﻿using Arcatech.Units.Inputs;
using UnityEngine;

namespace Arcatech.AI
{
    [CreateAssetMenu(menuName = "AIConfig/Action/LookForPlayer")]
    public class LookForPlayerAction : Action
    {
        public override void Act(EnemyStateMachine controller)
        {
            controller.OnLookSphereCast();
        }
    }
}