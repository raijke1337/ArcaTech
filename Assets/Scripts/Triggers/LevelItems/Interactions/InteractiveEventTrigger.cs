using Arcatech.Actions;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Level
{
    public class InteractiveEventTrigger : BaseLevelEventTrigger, IInteractible
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
            foreach (var r in UserActionOnInteract)
            {
                r.BuildActionResult().ProduceResult(actor as BaseEntity, null, transform);
            }
            actor.AcceptInteraction(this);
            // Debug.Log($"{UnitName} tried to interact with {actor.UnitName} ");
        }
        #endregion
    }
}