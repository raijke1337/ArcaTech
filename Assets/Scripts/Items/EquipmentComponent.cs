using System.Collections.Generic;
using System.Linq;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentComponent : MonoBehaviour,IUsableComponent
    {
        [SerializeField] private Transform spawner;
        public Transform EffectSpawn => spawner;

        private List<IEquipmentPart> _parts;
        
        protected void OnEnable()
        {
            if (spawner == null)
            {
                Debug.LogWarning($"Spawner not set in {this}");
                spawner = transform;
            }
            _parts = GetComponentsInChildren<IEquipmentPart>().ToList();
        }
        public void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            foreach (var part in _parts) part.TriggerState(notifyType);
        }
    }
}


