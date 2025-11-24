using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "Null result", menuName = "Actions/Action Result/Dummy null result")]
    public class NullResult : SerializedActionResult
    {
        public override ActionResult BuildActionResult()
        {
            return new NullActionResult();
        }
    }
    
    public class NullActionResult : ActionResult
    {
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place,
            Quaternion placeRot)
        {
            return true;
            // Nothing happens
        }
    }
}