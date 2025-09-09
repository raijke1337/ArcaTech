using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public interface IActionStateItem
    {
        public void HandleActionState(UnitActionState s);
    }
}