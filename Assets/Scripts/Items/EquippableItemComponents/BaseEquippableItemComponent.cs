using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    public class BaseItemComponent : MonoBehaviour
    {
        [SerializeField] protected Transform _spawner;
        public Transform Spawner { get => _spawner; }

        protected virtual void OnValidate()
        {
            Assert.IsNotNull(_spawner);
            this.ValidateRefs();
        }
        public virtual void OnUse()
        {

        }
    }
}


