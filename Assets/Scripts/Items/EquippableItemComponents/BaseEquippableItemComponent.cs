using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    public class BaseEquippableItemComponent : MonoBehaviour
    {
        [SerializeField] protected Transform _spawner;
        public Transform Spawner { get => _spawner; }

        private void OnValidate()
        {
            Assert.IsNotNull(_spawner);
            this.ValidateRefs();
        }
    }
}


