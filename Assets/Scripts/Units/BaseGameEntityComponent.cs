using UnityEngine;
namespace Arcatech
{/// <summary>
/// new component that defines any game enitity that does something
/// </summary>
    public class BaseGameEntityComponent : MonoBehaviour
    {
        [SerializeField] string _name;
        [SerializeField] Side entitySide;
        [Space,SerializeField] protected bool _showDebugs = false;

        public string GetUnitName { get => _name; }
        public Side GetEntitySide => entitySide;
    }
}