using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Units.Control;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Arcatech.Units
{

    [CreateAssetMenu(menuName = "States/Actions/PerformCommand")]
    public class PerformCommandActionSo : SerializedActionResult
    {
        UnitActionType actionType;
        public override ActionResult BuildActionResult()
        {
            return new PerformCommandActionResult(actionType);
        }
    }

    public class PerformCommandActionResult : ActionResult
    {
        BaseGameEntityComponent c;
        UnitActionType actionType;

        private List<IUnitCommandPerformer> performers;
        public PerformCommandActionResult(UnitActionType actionType)
        {
            this.actionType = actionType;
        }

        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place)
        {
            if (user == null) return false;
            if (!c)
            {
                // new user, cache to bot look uop every time
                c = user;
                performers = user.GetComponentsInChildren<IUnitCommandPerformer>().ToList();
                if (performers == null || performers.Count == 0)
                {
                    Debug.LogWarning($"PerformCommandAction: no IUnitCommandHandler found on {user.name}");
                    return false;
                }
            }

            // Use the ctx.PendingCommand stored in data (or pass in from calling code)
            if (actionType  == UnitActionType.None)
            {
                // fallback: data not provided; caller must provide action via data parameter
                Debug.LogWarning("PerformCommandAction: missing UnitActionType in data");
                return false;
            }

            // Call handlers; require all to return true for overall success (adjust if needed)
            foreach (var h in performers)
            {
                bool ok = h.DoUnitCommand(actionType, true);
                if (!ok)
                {
                    Debug.LogWarning($"Handler {h} failed to perform action {actionType}");
                    return false;
                }
            }

            return true;
        }
    }
}