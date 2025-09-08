using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "Null result", menuName = "Actions/Action Result/Dummy null result")]
    public class NullResult : SerializedActionResult
    {
        public override IActionResult BuildActionResult()
        {
            return new  NullActionResult();
        }
    }
    
    public class NullActionResult : IActionResult
    {
        public void ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place)
        {
            // Nothing happens
        }
    }
}