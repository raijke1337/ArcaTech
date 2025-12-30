using System;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "hitProducer_beam_", menuName = "Usables/Hit Producer/Beam")]
    public class SerializedBeamHitsProducer : SerializedHitProducer
    {
        [Header("Beam Configuration")] public SerializedBeamShooterConfig beamConfig;

        [Header("Hit Report Settings")] [Min(0.01f)]
        public float hitReportFrequency = 0.1f; // Time between hit reports in seconds

        public bool reportOnHitEnter = true;
        public bool reportOnHitContinuous = true;

        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new BeamHitProducer(owner, item, this);
        }
    }

    public class BeamHitProducer : HitProducer
    {
        
        private readonly BeamWeaponComponent _beamShooter;
        private readonly HashSet<BaseGameEntityComponent> _hitThisFrame = new();
 
        private bool _beamActive;

        public BeamHitProducer(BaseGameEntityComponent owner, EquipmentComponent item,
            SerializedBeamHitsProducer cfg) : base(owner, item, cfg)
        {
            _beamShooter = item.GetComponentInChildren<BeamWeaponComponent>();
            _beamShooter.Initialize(owner,item,cfg.beamConfig);
            _beamShooter.RegisterReceiver(this);
            _beamActive = false;
        }
        
        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (!_beamActive)
            {
                return;
            }

            // Filter self-hits
            if (triggerHitInfo.Target == Owner && !SelfHitActivates)
            {
                return;
            }

            // Skip if not a valid hit and we're not counting environment hits
            if (!triggerHitInfo.IsValidHit)
            {
                return;
            }
            
            // Prevent duplicate hits on the same target in the same frame
            if (_hitThisFrame.Contains(triggerHitInfo.Target))
            {
                return;
            }
            _hitThisFrame.Add(triggerHitInfo.Target);
        
            HitsThisUse++;
            if (HitsThisUse <= MaxHits)
            {
                if (Owner.ShowingDebugs)
                {
                    Debug.Log($"{Item} beam hit {triggerHitInfo.Target.GetName} at {triggerHitInfo.Position}, " +
                              $"hits this use: {HitsThisUse}/{MaxHits}");
                }
            
                CallHit(triggerHitInfo);
            }
        }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (triggerExitInfo.Target != null)
            {
                _hitThisFrame.Remove(triggerExitInfo.Target);
            }

            if (Owner.ShowingDebugs)
            {
                Debug.Log($"{Item} beam no longer hitting {triggerExitInfo.Target?.GetName ?? "target"}");
            }
        }

        public override void OnChangeState(StateMachineNotifyType info)
        {
            base.OnChangeState(info);

            switch (info)
            {
                case StateMachineNotifyType.Use:
                    StartBeam();
                    break;
                case StateMachineNotifyType.EndUse:
                    StopBeam();
                    break;
            }
        }

        private void StartBeam()
        {
            _beamActive = true;
            _hitThisFrame.Clear();
            _beamShooter.StartBeam(Owner.transform.forward);
        
            if (Owner.ShowingDebugs)
            {
                Debug.Log($"{Item} beam started");
            }
        }

        private void StopBeam()
        {
            _beamActive = false;
            _beamShooter.StopBeam();
            _hitThisFrame.Clear();
        
            if (Owner.ShowingDebugs)
            {
                Debug.Log($"{Item} beam stopped. Total hits: {HitsThisUse}");
            }
        }
    }

[Serializable]
    public struct SerializedBeamShooterConfig
    {
        [Header("Beam Visual Settings")]
        [Min(0.01f)] public float beamWidth;
        [Min(0.01f)] public float beamLength;
        public Material beamMaterial;


        [Header("Hit Detection")] [Min(0.01f)] public float raycastFrequency; // Time between raycasts in seconds
        [Min(1)] public int raycastsPerFrame; // Number of raycasts to perform per frame
    
        [Header("Hit Filtering")]
        [Min(0)] public float minDistanceBetweenHits; // Minimum distance on beam before same entity can be hit again
    }
}