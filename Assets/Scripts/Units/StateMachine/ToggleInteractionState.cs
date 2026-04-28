using System.Numerics;
using Arcatech.Actions;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/Actions/InteractStateChange")]
    public class ToggleInteractionState : SerializedActionResult
    {
        [SerializeField] bool SetInInteractionStateTo;
    
        public override ActionResult Deserialize()
        {
            return new ToggleInteractionStateResult(SetInInteractionStateTo);
        }
    }

    public class ToggleInteractionStateResult : ActionResult
    {
        public ToggleInteractionStateResult(bool value) => setTo = value;
        private bool setTo;
        private StateMachineContext ctx;
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            if (ctx == null)
            {
                if (user.TryGetComponent(out EntityStateMachineComponent smc))
                {
                    ctx = smc.Context;
                }
                else return false;
            }

            ctx.InInteraction = setTo;
            return true;
        }
    }
}