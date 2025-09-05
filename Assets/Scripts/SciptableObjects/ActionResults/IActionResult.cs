using UnityEngine;

namespace Arcatech.Actions
{
    public interface IActionResult
    {
        void ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place);
    }

}