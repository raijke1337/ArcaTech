using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Actions
{
    public interface IActionResult
    {
        void ProduceResult(BaseEntityOLD user, BaseEntityOLD target, Transform place);
    }

}