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

public class SkillAreaComp : MonoBehaviour
{
    public event SimpleEventsHandler<BaseUnit> TargetHitEvent;
    public event SimpleEventsHandler SkillAreaDoneEvent;


    [HideInInspector]public SkillData Data;
    protected virtual void OnTriggerEnter(Collider other)
    {
        var comp = other.GetComponent<BaseUnit>();
        if (comp == null) return;
        TargetHitEvent?.Invoke(comp);
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }
    private void Start()
    {
        StartCoroutine(ScalerCor());
    }

    private IEnumerator ScalerCor()
    {
        Vector3 originalScale = transform.localScale;
        float time = 0f;
        while (time < Data.PersistTime)
        {
            time += Time.deltaTime;
            float K = time / Data.PersistTime;
            transform.localScale = Vector3.Slerp(originalScale * Data.StartArea, originalScale * Data.FinalArea, K);
            yield return null;
        }
        Destroy(gameObject);
        SkillAreaDoneEvent?.Invoke();
        yield return null;
    }
}

