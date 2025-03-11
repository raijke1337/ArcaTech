using Arcatech.Units.Behaviour;
using UnityEngine;
namespace Arcatech.Units
{
    public class MetalGolemUnit : NPCUnit
    {
        [Space, Header("Metal golem unit behaviour")]
        [SerializeField,Range(1,180)] float _playerInFrontAngle = 15f;
        [SerializeField,Range(1,20)] float _chargeRange = 5f;

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            UnityEditor.Handles.color = Color.gray;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _chargeRange);
        }

        protected override void SetupBehavior()
        {


            Leaf resumeAgent = new Leaf(new BehaviourAction(() => agent.isStopped = false), "resume agent in case it was stopped by combat");
            Leaf stopAgent = new Leaf(new BehaviourAction(() => agent.isStopped = true), "stop agent to perform combat");


            Sequence combatSequence = new Sequence("combat actions " + UnitName, 80);
            Leaf checkCombat = new Leaf(new SimpleBehaviourCondition(() => UnitInCombatState == true), "check combat state", 100);
            BehaviourPrioritySelector combatPriority = new BehaviourPrioritySelector("select combat action", 0);
            Leaf combatSequenceDone = new Leaf(new BehaviourAction(() => combatSequence.Reset()), "Reset combat");

            combatSequence.AddChild(checkCombat);
            combatSequence.AddChild(combatPriority);
            combatSequence.AddChild(combatSequenceDone);




            Sequence aimAndAttack = new Sequence("aim at player and use weapon", 50);

            Leaf checkDistance = new Leaf(new SimpleBehaviourCondition(() => CheckDistance(_player.transform, Comparer.Less, _attackingRange)), "check if player is in attack range ");
            Leaf rotate = new Leaf(new AimAtTransform(agent, _player, 1f, movementStats.Stats[Stats.MovementStatType.TurnSpeed]), "aim at player");
            Leaf weapReady = new Leaf(new CombatActionReadyBehaviourCondition(_skills, _weapons, UnitActionType.Melee), "Check attack ready");
            Leaf weapUse = new Leaf(new BehaviourAction(() => HandleUnitAction(UnitActionType.Melee)), "Melee attack");

            aimAndAttack.AddChild(checkDistance);
            aimAndAttack.AddChild(stopAgent);
            aimAndAttack.AddChild(rotate);
            aimAndAttack.AddChild(weapReady);
            aimAndAttack.AddChild(weapUse);
            aimAndAttack.AddChild(combatSequenceDone);

            combatPriority.AddChild(aimAndAttack);

            Sequence aimAndCharge = new Sequence("Aim at player and use charge attack", 90);

            BehaviorInverter inverseDistance = new BehaviorInverter("Check if player is too far to attack");
            inverseDistance.AddChild(checkDistance);
            Leaf checkChargeRange = new Leaf(new SimpleBehaviourCondition(() => CheckDistance(_player.transform, Comparer.Less, _chargeRange)), "check if player is in charge range ");
            Leaf skillReady = new Leaf(new CombatActionReadyBehaviourCondition(_skills, _weapons, UnitActionType.MeleeSkill), "Check skill ready");
            Leaf skillUse = new Leaf(new BehaviourAction(() => HandleUnitAction(UnitActionType.MeleeSkill)), "Melee skill");

            aimAndCharge.AddChild(inverseDistance);
            aimAndCharge.AddChild(checkChargeRange);
            aimAndCharge.AddChild(skillReady);
            aimAndCharge.AddChild(skillUse);
            aimAndCharge.AddChild(combatSequenceDone);

            combatPriority.AddChild(aimAndCharge);


            Sequence chasePlayer = new Sequence("chase player", 80);

            Leaf setStoppingDistance = new Leaf(new BehaviourAction(() => agent.stoppingDistance = _attackingRange), "set stopping distance to attack range");
            BehaviorInverter checkNeedsChase = new BehaviorInverter("invert distance check");
            checkNeedsChase.AddChild(checkDistance);
            Leaf chase = new Leaf(new MoveToTransformStrategy(agent, _player), "move to player");

            chasePlayer.AddChild(setStoppingDistance);
            chasePlayer.AddChild(checkNeedsChase);
            chasePlayer.AddChild(resumeAgent);
            chasePlayer.AddChild(chase);
            chasePlayer.AddChild(combatSequenceDone);

            combatPriority.AddChild(chasePlayer);




            Sequence idleSequence = new Sequence("idling", 10);
            BehaviorInverter noCombatState = new BehaviorInverter("no combat state");
            noCombatState.AddChild(checkCombat);

            idleSequence.AddChild(noCombatState);
            if (patrolPointVariants != null && patrolPointVariants.Count > 0)
            {
                idleSequence.AddChild(SetupPatrolPoints());
            }
            idleSequence.AddChild(SetupIdleRoaming());


            tree.AddChild(idleSequence);
            tree.AddChild(combatSequence);


        }

    }
}

