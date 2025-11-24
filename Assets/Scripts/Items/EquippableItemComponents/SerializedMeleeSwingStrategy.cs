// using Arcatech.Actions;
// using UnityEngine;
// using UnityEngine.Assertions;
//
// namespace Arcatech.Items
// {
//     [CreateAssetMenu (fileName = "Weapon swing Strategy",menuName = "Items/Use strategy/Melee weapon swing") ]
//     public class SerializedMeleeSwingStrategy : SerializedWeaponUseStrategy
//     {
//
//         [SerializeField] private SerializedActionResult[] OnInvalidHit;
//         [SerializeField] SerializedActionResult[] OnValidHit;
//
//         
//         public override WeaponStrategy ProduceStrategy(BaseGameEntityComponent unit, EquipWithUsablesSO cfg, EquipmentComponent comp)
//         {
//             return new MeleeSwingStrategy(OnValidHit, OnInvalidHit, unit, cfg, TotalCharges,ChargeRestoreTime,comp);
//         }
//     }
//
//
// }