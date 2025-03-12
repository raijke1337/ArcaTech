using Arcatech.Actions;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Level
{

    public class InteractiveItemComponent : MonoBehaviour , IInteractible
    {
        #region interface
        [Header("Interactive trigger"),Space,SerializeField] protected string _displayName = "Interactive item";
        [SerializeField] protected SerializedActionResult[] UserActionOnInteract;
        [SerializeField] string AnimatorTrigger;

        public string UnitName => _displayName;
        public IReadOnlyDictionary<BaseStatType, StatValueContainer> GetDisplayValues => null;

        public Vector3 Position => transform.position;

        public virtual void AcceptInteraction(IInteractible actor)
        {
            foreach (var cond in _list.Keys)
            {
                bool rse = cond.PerformConditionChecks(actor, this, transform);
                foreach (ConditionControlledItem item in _list[cond])
                {
                    Debug.Log(item);
                    item.SetState(rse);
                }
            }
            foreach (var r in UserActionOnInteract)
            {
                r.BuildActionResult().ProduceResult(actor as BaseEntity, null, transform);
            }
            actor.AcceptInteraction(this);
        }
        #endregion
        #region conditions
        [Space, Header("Condition checker")]
        [SerializeField] SerializedDictionary<EventCondition, ConditionControlledItem[]> _list;

    }
    #endregion
}