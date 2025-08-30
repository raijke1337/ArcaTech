using Arcatech.AI;
using UnityEngine;

namespace Arcatech.Units
{
    public class CrawlerUnit : NPCUnitComponent
    {

        [Header("NPC Behaviour : Crawler")]
        [SerializeField, Range(0, 1)] float healAtPercent = 0.6f;
        [SerializeField] UnitActionType healAction = UnitActionType.MeleeSkill;

        ITacticsRequest checkLowHPAlly;
        Transform assistedAlly;
        bool CheckAllyCondition()
        {
            if (_group == null) return false;
            var unit = _group.ProcessTacticsRequest(checkLowHPAlly);
            if (unit != null) 
            {
                assistedAlly = unit.transform;
                agent.SetDestination(assistedAlly.position); 
                return true;
            }
            else return false;
        }

        //protected override void SetupBehavior()
        //{
        //    Leaf resumeAgent = new Leaf(new BehaviourAction(() => agent.isStopped = false), "resume agent in case it was stopped by combat");
        //    Leaf stopAgent = new Leaf(new BehaviourAction(() => agent.isStopped = true), "stop agent to perform combat");


        //    Sequence combatSequence = new Sequence("combat actions " + UnitName, 80);
        //    Leaf checkCombat = new Leaf(new SimpleBehaviourCondition(() => UnitInCombatState == true), "check combat state", 100);
        //    BehaviourPrioritySelector combatPriority = new BehaviourPrioritySelector("select combat action", 0);
        //    Leaf combatSequenceDone = new Leaf(new BehaviourAction(() => combatSequence.Reset()), "Reset combat");

        //    combatSequence.AddChild(checkCombat);
        //    combatSequence.AddChild(combatPriority);
        //    combatSequence.AddChild(combatSequenceDone);

        //    checkLowHPAlly = new TacticsRequestLowStatAllyAction(BaseStatType.Health, Comparer.Less, healAtPercent, healAction);
        //    Sequence assistAlly = new Sequence("assist damaged ally", 100);

        //    Leaf allyAvailable = new Leaf(new SimpleBehaviourCondition(() => CheckAllyCondition()), "tactics request");
        //    Leaf checkSkill = new Leaf(new CombatActionReadyBehaviourCondition(_skills, _weapons, healAction), "is skill ready");
        //    Leaf arrived = new Leaf(new SimpleBehaviourCondition(() => (agent.remainingDistance <= agent.stoppingDistance)),"has arrived at ally");
        //    Leaf useSkill = new Leaf(new BehaviourAction(() => HandleUnitAction(healAction)), "use heal");

        //    assistAlly.AddChild(allyAvailable);
        //    assistAlly.AddChild(checkSkill);
        //    assistAlly.AddChild(resumeAgent);
        //    assistAlly.AddChild(arrived);
        //    assistAlly.AddChild(useSkill);
        //    assistAlly.AddChild(combatSequenceDone);

        //    combatPriority.AddChild(assistAlly);


        //    Sequence aimAndAttack = new Sequence("aim at player and use weapon", 50);

        //    Leaf checkDistance = new Leaf(new SimpleBehaviourCondition(() => CheckDistance(_player.transform, Comparer.Less, _attackingRange)), "check if player is in attack range ");
        //    Leaf rotate = new Leaf(new AimAtTransform(agent, _player, 1f, movementStats.Stats[Stats.MovementStatType.TurnSpeed]), "aim at player");
        //    Leaf shoot = new Leaf(new CombatActionReadyBehaviourCondition(_skills, _weapons, UnitActionType.Melee), "Check attack ready");
        //    Leaf shoot2 = new Leaf(new BehaviourAction(() => HandleUnitAction(UnitActionType.Melee)), "Melee attack");

        //    aimAndAttack.AddChild(checkDistance);
        //    aimAndAttack.AddChild(stopAgent);
        //    aimAndAttack.AddChild(rotate);
        //    aimAndAttack.AddChild(shoot);
        //    aimAndAttack.AddChild(shoot2);
        //    aimAndAttack.AddChild(combatSequenceDone);

        //    combatPriority.AddChild(aimAndAttack);


        //    Sequence chasePlayer = new Sequence("chase player", 80);

        //    Leaf setStoppingDistance = new Leaf(new BehaviourAction(() => agent.stoppingDistance = _attackingRange), "set stopping distance to attack range");
        //    BehaviorInverter checkNeedsChase = new BehaviorInverter("invert distance check");
        //    checkNeedsChase.AddChild(checkDistance);
        //    Leaf chase = new Leaf(new MoveToTransformStrategy(agent, _player), "move to player");

        //    chasePlayer.AddChild(setStoppingDistance);
        //    chasePlayer.AddChild(checkNeedsChase);
        //    chasePlayer.AddChild(resumeAgent);
        //    chasePlayer.AddChild(chase);
        //    chasePlayer.AddChild(combatSequenceDone);

        //    combatPriority.AddChild(chasePlayer);




        //    Sequence idleSequence = new Sequence("idling", 10);
        //    BehaviorInverter noCombatState = new BehaviorInverter("no combat state");
        //    noCombatState.AddChild(checkCombat);

        //    idleSequence.AddChild(noCombatState);
        //    if (patrolPointVariants != null && patrolPointVariants.Count > 0)
        //    {
        //        idleSequence.AddChild(SetupPatrolPoints());
        //    }
        //    idleSequence.AddChild(SetupIdleRoaming());


        //    tree.AddChild(idleSequence);
        //    tree.AddChild(combatSequence);
        //}
    }
}