using System;
using Arcatech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detect Combat ", story: "[Self] evaluates combat state with [Player] into [CombatState]", category: "Action/Game", id: "55d9f32c2f19c7f9d0068ed9fc5a6531")]
public partial class DetectCombatAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<bool> CombatState;
    
    
    [SerializeReference] public BlackboardVariable<float> AggroRadius;
    [SerializeReference] public BlackboardVariable<float> DeaggroRadius;
    [SerializeReference] public BlackboardVariable<float> AggroCooldown;
    [SerializeReference] public BlackboardVariable<bool>  RequiresLineOfSight;
    [SerializeReference] public BlackboardVariable<float> FieldOfViewAngle;

    [SerializeReference] public BlackboardVariable<float> BehaviorUpdateInterval;
    
    // признак того, что состояние изменилось на этом тике (читает Conditional Branch)
    [SerializeReference] public BlackboardVariable<bool> StateChanged;

    // внутренние (не на Blackboard)
    private float _timer;
    private bool  _pending;        // состояние-кандидат
    private bool  _hasPending;
    
    
    protected override Status OnStart()
    {
        
        StateChanged.Value = false;

        var self = Self.Value;
        var player = Player.Value;
        

        if (self == null || player == null)
        {
            Debug.LogWarning("[DetectCombat] Self or Player == null");
            return Status.Success;
        }

        
        Vector3 toPlayer = player.transform.position - self.transform.position;
        float dist = toPlayer.magnitude;
        bool current = CombatState.Value;

        // желаемое состояние с гистерезисом (две границы)
        bool desired;

        if (!current)
        {
            bool inRange = dist <= AggroRadius.Value;
            bool seen = !RequiresLineOfSight.Value || CanSee(self, player, toPlayer, dist);
            desired = inRange && seen;
        }
        else
        {
            desired = dist <= DeaggroRadius.Value; // в бою, пока не вышли за деаггро
        }
                
        
        if (player.TryGetComponent(out BaseGameEntityComponent bc) && !bc.EntityAlive)
        {
            desired = false;
            // player dead, combat exit
        }

        // выдержка условия в течение AggroCooldown.
        // шаг = период опроса, т.к. узел вызывается раз в BehaviorUpdateInterval
        float step = Mathf.Max(BehaviorUpdateInterval.Value, 0.0001f);

        if (desired != current)
        {
            if (!_hasPending || _pending != desired)
            {
                _hasPending = true;
                _pending = desired;
                _timer = 0f;
            }

            _timer += step;
            if (_timer >= AggroCooldown.Value)
            {
                CombatState.Value = desired;
                StateChanged.Value = true;
                _hasPending = false;
                _timer = 0f;
            }
        }
        else
        {
            _hasPending = false;
            _timer = 0f;
        }

        return Status.Success;   // <-- ГЛАВНОЕ: отдаём управление Sequence дальше
    }

    private bool CanSee(GameObject self, GameObject player, Vector3 toPlayer, float dist)
    {
        float half = FieldOfViewAngle.Value * 0.5f;
        if (Vector3.Angle(self.transform.forward, toPlayer) > half)
            return false;

        if (Physics.Raycast(self.transform.position, toPlayer.normalized,
                out RaycastHit hit, dist, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))       
        {
            return hit.collider.gameObject == player ||
                   hit.collider.transform.IsChildOf(player.transform);
        }
        return true;
    }
    protected override void OnEnd()
    {
    }
}

