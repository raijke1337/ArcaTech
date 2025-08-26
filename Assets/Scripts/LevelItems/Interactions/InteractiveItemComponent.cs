using Arcatech.Actions;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;
using Arcatech.Level.Conditions;
namespace Arcatech.Level
{

    public class InteractiveItemComponent : MonoBehaviour , IInteractible
    {
        #region interface
        [Header("Interactive trigger"),Space,SerializeField] protected string _displayName = "Interactive item";
        [SerializeField] protected SerializedActionResult[] UserActionOnInteract;
        [SerializeField] string AnimatorTrigger;
        Animator _a;
        public string UnitName => _displayName;
        public IReadOnlyDictionary<BaseStatType, StatValueContainer> GetDisplayValues => null;

        public Vector3 Position => transform.position;

        public bool Triggered => throw new System.NotImplementedException();

        private void OnEnable()
        {
            _a = GetComponent<Animator>();
        }
        public virtual void AcceptInteraction(IInteractible actor)
        {
            foreach (var cond in _list.Keys)
            {
                ConditionCheckResult rse = cond.PerformConditionChecks(actor, this, transform);
                foreach (ConditionControlledItemComponent item in _list[cond])
                {
                    item.SetState(rse);
                }
                if (rse == ConditionCheckResult.Success && _a != null)
                {
                    _a.SetTrigger(AnimatorTrigger);
                }
            }
            foreach (var r in UserActionOnInteract)
            {
                r.BuildActionResult().ProduceResult(actor as BaseGameEntityComponent, null, transform);
            }
            actor.AcceptInteraction(this);
        }
        #endregion
        #region conditions
        [Space, Header("Condition checker")]
        [SerializeField] SerializedDictionary<EventCondition, ConditionControlledItemComponent[]> _list;

    }
    #endregion
}