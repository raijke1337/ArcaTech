using System.Collections.Generic;
using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.Level
{
    [CreateAssetMenu(fileName = "when unit is killed, activate",
        menuName = "Interactions/Level Event Condition/BaseEntity is Kill()")]
    public class ActivateWhenCheckedIsKilled : CheckedLevelEventCondition
    {
        public override bool CheckCondition(LevelEventPairContainer pair)
        {
            foreach (var check in pair.Check)
            {
                if (check.TryGetComponent<BaseGameEntityComponent>(out var a))
                {
                    if (!a) return true;
                    if (a.EntityAlive) return false;
                }
            }

            return true;
        }
    }

}