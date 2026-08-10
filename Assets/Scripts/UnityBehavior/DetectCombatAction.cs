using System;
using Arcatech;
using Arcatech.Units;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Detect Combat",
    story: "[Self] evaluates combat state with [Player] into [CombatState]",
    category: "Action/Game",
    id: "55d9f32c2f19c7f9d0068ed9fc5a6531")]
public partial class DetectCombatAction : Action
{
    // ── интерфейс узла с графом (только это остаётся на Blackboard) ──
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform>  Player;      // из Shared PlayerData
    [SerializeReference] public BlackboardVariable<bool>       CombatState; // результат
    [SerializeReference] public BlackboardVariable<bool>       StateChanged; // сигнал для Branch On

    // ── контекст (не на Blackboard) ──
    private NPCBehaviorWrapper _wrapper;
    private EnemyData_SO       _cfg;

    // ── внутреннее состояние гистерезиса ──
    private bool  _hasPending;
    private bool  _pending;
    private float _pendingSince;   // Time.time, когда кандидат стал стабильным

    protected override Status OnStart()
    {
        StateChanged.Value = false;

        var self = Self.Value;
        if (self == null)
        {
            LogFailure("[DetectCombat] Self == null");
            return Status.Failure;
        }

        // контекст берём с агента, а НЕ из Blackboard
        if (_wrapper == null && !self.TryGetComponent(out _wrapper))
        {
            LogFailure("[DetectCombat] NPCBehaviorWrapper not found on Self");
            return Status.Failure;
        }
        _cfg = _wrapper.Config;
        if (_cfg == null)
        {
            LogFailure("[DetectCombat] Config (EnemyData_SO) is null");
            return Status.Failure;
        }

        var player = Player.Value;
        // игрока нет или он мёртв → форсируем выход из боя
        bool playerValid = player != null && (Player_Alive(player));

        bool current = CombatState.Value;
        bool desired;

        if (!playerValid)
        {
            desired = false;
        }
        else
        {
            Vector3 toPlayer = player.position - self.transform.position;
            float dist = toPlayer.magnitude;

            if (!current)
            {
                bool inRange = dist <= _cfg.AggroRad;
                bool seen = !_cfg.NeedLoS || CanSee(self, player, toPlayer, dist);
                desired = inRange && seen;
            }
            else
            {
                desired = dist <= _cfg.DeaggroRad; // гистерезис: вторая граница
            }
        }

        ApplyWithCooldown(current, desired);
        return Status.Success; // отдаём управление Sequence дальше
    }

    private void ApplyWithCooldown(bool current, bool desired)
    {
        if (desired == current)
        {
            _hasPending = false;
            return;
        }

        // новый/сменившийся кандидат — сбрасываем отсчёт
        if (!_hasPending || _pending != desired)
        {
            _hasPending   = true;
            _pending      = desired;
            _pendingSince = Time.time;
            return;
        }

        // кандидат держится непрерывно ≥ AggroCooldown — фиксируем
        if (Time.time - _pendingSince >= _cfg.AggroCooldown)
        {
            CombatState.Value  = desired;
            StateChanged.Value = true;
            _hasPending = false;
        }
    }

    private bool CanSee(GameObject self, Transform player, Vector3 toPlayer, float dist)
    {
        float half = _cfg.ViewAngle * 0.5f;
        if (Vector3.Angle(self.transform.forward, toPlayer) > half)
            return false;

        if (Physics.Raycast(self.transform.position, toPlayer.normalized,
                out RaycastHit hit, dist, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            return hit.collider.gameObject == player.gameObject ||
                   hit.collider.transform.IsChildOf(player);
        }
        return true; // ничего не мешает лучу
    }

    private static bool Player_Alive(Transform player)
    {
        return !player.TryGetComponent(out BaseGameEntityComponent bc) || bc.EntityAlive;
    }
}