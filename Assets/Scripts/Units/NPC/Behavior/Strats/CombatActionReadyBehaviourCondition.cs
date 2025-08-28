using Arcatech.Skills;

namespace Arcatech.Units.Behaviour
{
    public class CombatActionReadyBehaviourCondition : IBehaviorTreeStrategy
    {
        SkillsControllerOLD _skills;
        WeaponControllerOLD _weapon;
       // UnitStatsController _stats;
        UnitActionType _action;
        public CombatActionReadyBehaviourCondition (SkillsControllerOLD skills, WeaponControllerOLD weapon,UnitActionType actionType)
        {
            _action = actionType;
            _skills = skills;
            _weapon = weapon;
           // _stats = stats;
        }
        public Node.NodeStatus Process(NPCUnitComponent actor)
        {
            switch (_action)
            {
                case UnitActionType.Melee:
                    if (_weapon.CanUseAction(_action)) return Node.NodeStatus.Success;
                    else return Node.NodeStatus.Fail;
                case UnitActionType.Ranged:
                    if (_weapon.CanUseAction(_action)) return Node.NodeStatus.Success;
                    else return Node.NodeStatus.Fail;
                case UnitActionType.DodgeSkill:
                    if (_skills.CanUseAction(_action)) return Node.NodeStatus.Success;
                    else return Node.NodeStatus.Fail;
                case UnitActionType.MeleeSkill:
                    if (_skills.CanUseAction(_action)) return Node.NodeStatus.Success;
                    else return Node.NodeStatus.Fail;
                case UnitActionType.RangedSkill:
                    if (_skills.CanUseAction(_action)) return Node.NodeStatus.Success;
                    else return Node.NodeStatus.Fail;
                case UnitActionType.ShieldSkill:
                    if (_skills.CanUseAction(_action)) return Node.NodeStatus.Success;
                    else return Node.NodeStatus.Fail;

                default: return Node.NodeStatus.Fail;
            }

        }

        public void Reset()
        {
        }
    }
}