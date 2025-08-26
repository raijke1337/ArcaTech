using Arcatech.Triggers;
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

        public string GetName { get => _name; }
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;

        public bool Paused { get => _paused; }
        protected bool _paused;

    }
}