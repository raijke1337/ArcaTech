using Arcatech.Stats;
using Arcatech.Triggers;
using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
namespace Arcatech.Level.Conditions
{
    public class PassiveEventTrigger : MonoBehaviour
    {
        [SerializeField] protected EventCondition check;
        [SerializeField] protected ConditionControlledItemComponent affected;

        public bool Triggered { get; protected set; } = false;

        IInteractible checkdObject;

        private void Start()
        {
            checkdObject = GetComponent<IInteractible>();
        }
        public void CheckEventTrigger()
        {
            if (checkdObject == null || Triggered) return;
            affected.SetState(check.PerformConditionChecks(null, checkdObject, transform));
            Triggered = true;
        }
    }
}