using Arcatech.Units.Inputs;
using UnityEngine;
namespace Arcatech.AI
{
    [CreateAssetMenu(menuName = "AIConfig/Action/Attack")]
    public class AttackAction : Action
    {
        public CombatActionType CombatActionType;
        public override void Act(StateMachine controller)
        {
            controller.OnAttackRequest(CombatActionType);
        }

    }

}