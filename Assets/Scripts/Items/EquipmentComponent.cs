using Arcatech.Units;
using Drakkar.GameUtils;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentComponent : MonoBehaviour,IUsableComponent
    {
        [SerializeField] private Transform spawner;
        public Transform EffectSpawn => spawner;
        private EquipmentAnimator _equipmentAnimator;
        [SerializeField] DrakkarTrail _trail;
        
        protected void OnEnable()
        {
            if (spawner == null)
            {
                Debug.LogWarning($"Spawner not set in {this}");
                spawner = transform;
            }
            TryGetComponent(out _equipmentAnimator);
        }


        public void OnChangeUsableState(StateMachineNotifyType notifyType)
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
                case StateMachineNotifyType.Use:
                {
                    //if (useParticleEffect)  useParticleEffect.Play(); 
                    break;
                }
            }
            if (_equipmentAnimator) _equipmentAnimator.OnChangeUsableState(notifyType);
        }
    }
}


