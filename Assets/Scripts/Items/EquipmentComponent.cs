using System;
using Arcatech.Units;
using Drakkar.GameUtils;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentComponent : MonoBehaviour,IStateMachineNotificationReceiver
    {
        [SerializeField] protected Transform spawner;

        public Transform EffectSpawn
        {
            get
            {
                Debug.Log($"Get spawner position: {spawner.position} at {Time.time}");
                return spawner;
            }
        }
        private EquipmentAnimator _equipmentAnimator;
        private DrakkarTrail _trail;

        protected virtual void OnEnable()
        {
            if (spawner == null)
            {
                Debug.LogWarning($"Spawner not set in {this}");
                spawner = transform;
            }
            TryGetComponent(out _equipmentAnimator);
            TryGetComponent(out _trail);
        }


        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
            switch (notifyType)
            {
                case StateMachineNotifyType.Starting:
                {
                    if (_trail) _trail.Begin();
                    break;
                }
                case StateMachineNotifyType.EndUse:
                {
                    if (_trail) _trail.End();
                    break;
                }
            }
            if (_equipmentAnimator) _equipmentAnimator.StateMachineNotification(notifyType);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(spawner.position, spawner.position+spawner.transform.forward);
            Gizmos.DrawWireSphere(spawner.position, 0.5f);
        }
    }
}


