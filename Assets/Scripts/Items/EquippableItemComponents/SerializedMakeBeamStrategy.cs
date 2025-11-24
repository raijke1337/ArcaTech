// using System;
// using Arcatech.Actions;
// using UnityEngine;
//
// namespace Arcatech.Items
// {
//     [CreateAssetMenu(fileName = "Make a beam strategy", menuName = "Items/Use strategy/Make beam")]
//     public class SerializedMakeBeamStrategy : SerializedWeaponUseStrategy
//     {
//
//     [Header ("Config")]
//     [SerializeField] BeamSettings _beamSettings;
//     
//         [Header("Results")]
//         [SerializeField] private SerializedActionResult[] onBeamHit;
//
//         public override WeaponStrategy ProduceStrategy(BaseGameEntityComponent unit, EquipWithUsablesSO cfg, EquipmentComponent comp)
//         {
//             return new ShootBeamStrategy(_beamSettings,onBeamHit,unit, cfg, TotalCharges,ChargeRestoreTime,comp);
//         }
//     }
//
//     [Serializable]
//     public struct BeamSettings
//     {
//         [Header("Beam Properties")] public float MaxRange;
//         public float BeamWidth;
//         public LayerMask CollisionMask  ;
//     
//         [Header("Duration Settings")]
//         public float DefaultDuration;
//         public bool UseInfiniteDuration;
//         public float BurnIntervals;
//     
//         [Header("Visual Settings")]
//         public Material LaserMaterial;
//         public Color LaserColor;
//         public AnimationCurve IntensityCurve;
//         [Header("Audio")]
//         public AudioClip FireSound;
//         public AudioClip LoopSound;
//         public AudioClip StopSound;
//     }
// }