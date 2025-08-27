using Arcatech.Units.Behaviour;
using AYellowpaper.SerializedCollections.KeysGenerators;
using UnityEngine;

namespace Arcatech.Units
{
    public class AnnoyingDroneUnit : NPCUnitComponent
    {

        [Header("NPC Behaviour : Drone")]
        [SerializeField, Range(0,25),Tooltip(" range at which unit tries to run away")] float _runAwayDistance = 4f;

        //protected override void SetupBehavior()
        //{
        //    Leaf resumeAgent = new Leaf(new BehaviourAction(() => agent.isStopped = false), "resume agent in case it was stopped by combat");
        //    Leaf stopAgent = new Leaf(new BehaviourAction(() => agent.isStopped = true), "stop agent to perform combat");


        //    Sequence combatSequence = new Sequence("combat actions " + UnitName);
        //    Leaf checkCombat = new Leaf(new SimpleBehaviourCondition(() => UnitInCombatState == true), "check combat state", 100);
        //    BehaviourPrioritySelector combatPriority = new BehaviourPrioritySelector("select combat action");
        //    Leaf combatSequenceDone = new Leaf(new BehaviourAction(() => combatSequence.Reset()), "Reset combat");

        //    combatSequence.AddChild(checkCombat);
        //    combatSequence.AddChild(combatPriority);
        //    combatSequenceDone.AddChild(new Leaf(new BehaviourAction(() => agent.stoppingDistance = initStoppingDistance),"Reset stopping distacne"));
        //    combatSequence.AddChild(combatSequenceDone);


        //    Sequence runAwayFromPlayer = new Sequence("run away if player too close", 60);
        //    Leaf checkIfPlayerIsTooClose = new Leaf(new SimpleBehaviourCondition(() => CheckDistance(_player.transform, Comparer.Less, _runAwayDistance)),"check distance to player");
        //    Leaf flee = new Leaf(new RoamAroundPoint(20f, transform.position, agent), "placeholder pick a point to run to");

        //    runAwayFromPlayer.AddChild(checkIfPlayerIsTooClose);
        //    runAwayFromPlayer.AddChild(resumeAgent);
        //    runAwayFromPlayer.AddChild(flee);


        //    Sequence aimAndShoot = new Sequence("aim at player and use weapon", 50);

        //    Leaf checkDistance = new Leaf(new SimpleBehaviourCondition(() => CheckDistance(_player.transform,Comparer.Less,_attackingRange)), "check if player is in attack range ");
        //    Leaf rotate = new Leaf(new AimAtTransform(agent, _player, 1f, movementStats.Stats[Stats.MovementStatType.TurnSpeed]), "aim at player");
        //    Leaf shoot = new Leaf(new CombatActionReadyBehaviourCondition(_skills, _weapons, UnitActionType.Ranged), "prepare to shoot");
        //    Leaf shoot2 = new Leaf(new BehaviourAction(()=>HandleUnitAction(UnitActionType.Ranged)),"Shoot");

        //    aimAndShoot.AddChild(checkDistance);
        //    aimAndShoot.AddChild(stopAgent);
        //    aimAndShoot.AddChild(rotate);
        //    aimAndShoot.AddChild(shoot);
        //    aimAndShoot.AddChild(shoot2);
        //    aimAndShoot.AddChild(combatSequenceDone);


        //    Sequence chasePlayer = new Sequence("chase player",20);
        //    Leaf setStoppingDistance = new Leaf(new BehaviourAction(() => agent.stoppingDistance = _attackingRange),"set stopping distance to attack range");
        //    Leaf chase = new Leaf(new MoveToTransformStrategy(agent, _player), "move to player");

        //    chasePlayer.AddChild(setStoppingDistance);
        //    chasePlayer.AddChild(resumeAgent);
        //    chasePlayer.AddChild(chase);
        //    chasePlayer.AddChild(combatSequenceDone);


        //    combatPriority.AddChild(chasePlayer);
        //    combatPriority.AddChild(aimAndShoot);
        //    combatPriority.AddChild(runAwayFromPlayer);

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
        protected override void OnDrawGizmos()
        {
            //base.OnDrawGizmos();
            //if (!UnitDebug) return;
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(agent.transform.position, agent.transform.position + agent.transform.forward);

        }
    }

}