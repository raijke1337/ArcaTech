using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    public abstract class BaseWeaponComponent : EquipmentComponent, IWeaponHitSource
    {
        public ITriggerNotificationProvider GetTriggerNotificationProvider
        {
            get
            {
                if (_triggerNotificationProvider == null)
                {
                    _triggerNotificationProvider = GetComponentInChildren<ITriggerNotificationProvider>();
                    if (_triggerNotificationProvider == null)
                    { 
                        Debug.LogWarning($"No trigger notification provider found on {gameObject.name}");
                    }
                }
                return _triggerNotificationProvider;
            }
        }
        ITriggerNotificationProvider _triggerNotificationProvider;
    }
}