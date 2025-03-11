using Arcatech.Units.Behaviour;
using UnityEngine;

namespace Arcatech.Units
{
    public class TurretUnit : NPCUnit
    {
        [Header("Turret settings")] 
        [SerializeField,Range(0,180)] int frontAngle = 90;
        public override void ApplyForceResultToUnit(float speed, float distance)
        {
            
        }
        protected override void SetupBehavior()
        {

            Sequence combatSequence = new Sequence("combat actions " + UnitName);
            Leaf checkCombat = new Leaf(new SimpleBehaviourCondition(() => UnitInCombatState == true), "check combat state", 100);
            BehaviourPrioritySelector combatPriority = new BehaviourPrioritySelector("select combat action");
            Leaf combatSequenceDone = new Leaf(new BehaviourAction(() => combatSequence.Reset()), "Reset combat");

            combatSequence.AddChild(checkCombat);
            combatSequence.AddChild(combatPriority);
            combatSequenceDone.AddChild(new Leaf(new BehaviourAction(() => agent.stoppingDistance = initStoppingDistance), "Reset stopping distacne"));
            combatSequence.AddChild(combatSequenceDone);

            Sequence aimAndShoot = new Sequence("aim at player and use weapon", 80);

            Leaf checkDistance = new Leaf(new SimpleBehaviourCondition(() => CheckDistance(_player.transform, Comparer.Less, _attackingRange)), "check if player is in attack range ");
            Leaf rotate = new Leaf(new AimAtTransform(agent, _player, 1f, movementStats.Stats[Stats.MovementStatType.TurnSpeed]), "aim at player");
            Leaf shoot = new Leaf(new CombatActionReadyBehaviourCondition(_skills, _weapons, UnitActionType.Ranged), "prepare to shoot");
            Leaf shoot2 = new Leaf(new BehaviourAction(() => HandleUnitAction(UnitActionType.Ranged)), "Shoot");

            aimAndShoot.AddChild(checkDistance);
            aimAndShoot.AddChild(rotate);
            aimAndShoot.AddChild(shoot);
            aimAndShoot.AddChild(shoot2);
            aimAndShoot.AddChild(combatSequenceDone);

            combatPriority.AddChild(aimAndShoot);

            Sequence idleSequence = new Sequence("idling", 10);
            BehaviorInverter noCombatState = new BehaviorInverter("no combat state");
                       
            noCombatState.AddChild(checkCombat);
            idleSequence.AddChild(noCombatState);

            tree.AddChild(idleSequence);
            tree.AddChild(combatSequence);
        }
    }

}