using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [CreateAssetMenu (fileName = "Weapon swing Strategy",menuName = "Items/Use strategy/Melee weapon swing") ]
    public class SerializedMeleeSwingStrategy : SerializedWeaponUseStrategy
    {

        [SerializeField] SerializedActionResult[] OnColliderHit;

        private void OnValidate()
        {
            Assert.IsNotNull(OnColliderHit);
            Assert.IsTrue(OnColliderHit.Length > 0);
        }
        public override WeaponStrategy ProduceStrategy(BaseGameEntityComponent unit, WeaponSO cfg, EquipmentComponent comp)
        {
            return new MeleeSwingStrategy(OnColliderHit, state, unit, cfg, TotalCharges,ChargeRestoreTime,comp);
        }
    }


}