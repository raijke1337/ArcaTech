using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    public class BaseItemComponent : ValidatedMonoBehaviour,IActionStateItem
    {
        [SerializeField] protected Transform spawner;
        public Transform Spawner => spawner;
        public virtual void HandleActionState(UnitActionState s)
        {
            Debug.Log($"ActionState {s}");
        }

    }
}


