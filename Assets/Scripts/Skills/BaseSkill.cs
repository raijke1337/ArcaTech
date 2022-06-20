using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Collider))]
public abstract class BaseSkill : MonoBehaviour, IAppliesTriggers
{
    public string SkillID;
    public BaseUnit Source;
    public SkillData SkillData;
    public GameObject EffectPrefab;

    public event TriggerEventApplication TriggerApplicationRequestEvent;

    private Collider _coll;

    protected virtual void OnTriggerEnter(Collider other)
    {
        PlaceAndSubEffect(other.transform);
    }
    

    private void Awake()
    {
        if (EffectPrefab == null || EffectPrefab.GetComponent<SkillAreaComp>() == null) Debug.LogError($"Set prefab for {this}");
        _coll = GetComponent<Collider>();
        _coll.isTrigger = true;
    }

    // use this to apply all trigger effects set in SKillData
    protected virtual void CallTriggerHit(BaseUnit target)
    {
        if (SkillData.TriggerIDs == null) return;
        foreach (var id in SkillData.TriggerIDs)
        {
            TriggerApplicationRequestEvent?.Invoke(id, target, Source);
        }
    }
    protected SkillAreaComp PlaceAndSubEffect(Transform tr)
    {
        var item = Instantiate(EffectPrefab).GetComponent<SkillAreaComp>();
        item.Data = SkillData;
        item.Source = Source;
        item.transform.position = tr.position;
        item.TargetHitEvent += (t) => CallTriggerHit(t);
        return item;
    }

}

