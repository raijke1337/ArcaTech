using System.Collections.Generic;
using System.Linq;
using Arcatech;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using UnityEngine;

public class EntityEffectController : MonoBehaviour, IPausableComponent, IKillableComponent
{
    private readonly Dictionary<EffectKey, List<ActiveEffectInstance>> _active = new();
    private readonly List<ActiveEffectInstance> _flat = new();
    private readonly IStackingResolver _stacking = new StackingResolver();
    private BaseGameEntityComponent _owner;
    private EffectContext _ctx;
    

    public bool Paused { get; set; }
    private bool _killed;

    private EffectsReceiverComponent _receiver;

    private void Awake()
    {
        TryGetComponent(out _owner);
        TryGetComponent(out _receiver);   // same GameObject, guaranteed by RequireComponent
        _ctx = new EffectContext();
    }

    /// <summary>
    /// Routes an incoming instance through the StackingResolver.
    /// </summary>
    public void AddEffect(ActiveEffectInstance instance, BaseGameEntityComponent source,
        EffectsReceiverComponent receiver, Vector3 place, Quaternion placeRot)
    
    {
        if (_killed) return;
        _active.TryGetValue(instance.Key, out var sameKey);

        var decision = _stacking.Resolve(sameKey, instance.StackType, instance.MaxStacks);
        _ctx.SetTarget(receiver, place, placeRot);
        instance.Tick(0f, _ctx);
        switch (decision)
        {
            case StackDecision.Reject:
                return; // e.g. stun already active, or stat-change at max stacks

            case StackDecision.Refresh:
                RefreshExisting(sameKey, place, placeRot);
                return;

            case StackDecision.Add:
                Register(instance);
                // zero-time tick preserves "instant on hit" for OneShot/AtStart & Before/offset=0
                _ctx.SetTarget(receiver, place, placeRot);
                instance.Tick(0f, _ctx);
                if (instance.IsFinished) Remove(instance);
                return;
        }
    }

    public bool HasEffect(string ID, out ActiveEffectInstance instance)
    {
        instance = _flat.FirstOrDefault(t => t.EffectId == ID);
        return instance != null;
    }
    
    private void RefreshExisting(List<ActiveEffectInstance> sameKey, Vector3 place, Quaternion placeRot)
    {
        // Refresh the most-recent live instance under this key.
        // For shield (Step 6) this also tops up the buffer; that lives in the Result,
        // so we re-run a zero-time tick after refresh to let it re-apply.
        var inst = sameKey[sameKey.Count - 1];
        inst.RefreshLifetime();
        _ctx.SetTarget(_receiver, place, placeRot);
        inst.Tick(0f, _ctx); // re-fire AtStart/Before tick so the buffer tops up (shield case)
    }

    private void Register(ActiveEffectInstance instance)
    {
        if (!_active.TryGetValue(instance.Key, out var list))
        {
            list = new List<ActiveEffectInstance>();
            _active[instance.Key] = list;
        }
        list.Add(instance);
        _flat.Add(instance);
    }

    private void Update()
    {
        if (_killed || Paused || _flat.Count == 0) return;

        float dt = Time.deltaTime;
        var pos = _owner.transform.position;

        for (int i = 0; i < _flat.Count; i++)
        {
            _ctx.SetTarget(_receiver, _owner.transform.position, Quaternion.identity);
            _flat[i].Tick(dt, _ctx);
        }

        for (int i = _flat.Count - 1; i >= 0; i--)
            if (_flat[i].IsFinished) RemoveAt(i);
    }

    private void Remove(ActiveEffectInstance inst)
    {
        _flat.Remove(inst);
        if (_active.TryGetValue(inst.Key, out var list))
        {
            list.Remove(inst);
            if (list.Count == 0) _active.Remove(inst.Key);
        }
    }

    private void RemoveAt(int flatIndex)
    {
        var inst = _flat[flatIndex];
        _flat.RemoveAt(flatIndex);
        if (_active.TryGetValue(inst.Key, out var list))
        {
            list.Remove(inst);
            if (list.Count == 0) _active.Remove(inst.Key);
        }
    }

    public void SetKilled(IKillerComponent c, bool value)
    {
        _killed = value;
        if (value) ClearAll();
    }

    private void ClearAll()
    {
        _ctx.SetTarget(_receiver, _owner.transform.position, Quaternion.identity);
        for (int i = 0; i < _flat.Count; i++)
            _flat[i].ForceExpire(_ctx);
        _flat.Clear();
        _active.Clear();
    }

    // ---- exposed for StackingResolver "total on target" counting (Step 5) ----
    internal int CountByEffectId(string effectId)
    {
        int n = 0;
        foreach (var kv in _active)
            if (string.Equals(kv.Key.EffectId, effectId, System.StringComparison.Ordinal))
                n += kv.Value.Count;
        return n;
    }
}