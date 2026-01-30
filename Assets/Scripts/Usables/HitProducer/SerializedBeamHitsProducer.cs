using System;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "hitProducer_beam_", menuName = "Usables/Hit Producer/Beam")]
    public class SerializedBeamHitsProducer : SerializedHitProducer
    {
        [Header("Beam Configuration")] public SerializedBeamShooterConfig beamConfig;

        [Header("Hit Settings")] [Min(0.01f)]
        public float HitsRefreshInterval = 0.1f; // how many seconds pass before the hits buffer is flushed, and they are reported as new

        public bool reportOnHitEnter = true;
        public bool reportOnHitContinuous = true;

        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new BeamHitProducer(owner, item, this);
        }
    }

    public class BeamHitProducer : HitProducer, ITriggerNotificationReceiver
    {
        
        private readonly BeamWeaponComponent _beamShooter;
        private readonly HashSet<BaseGameEntityComponent> _bufferedHits = new();
 
        private bool _beamActive;

        public BeamHitProducer(BaseGameEntityComponent owner, EquipmentComponent item,
            SerializedBeamHitsProducer cfg) : base(owner, item, cfg)
        {
            _beamShooter = item.GetComponentInChildren<BeamWeaponComponent>();
            _beamShooter.Initialize(owner,item,cfg.beamConfig);
            _beamShooter.RegisterReceiver(this);
            _beamActive = false;
        }
        
        public void TriggerEntered(TriggerHitInfo triggerHitInfo) // the buffering is done in the mono beam shooter
        {
            if (!_beamActive)
            {
                return;
            }
            // Prevent duplicate hits on the same target in the same frame
            if (!_bufferedHits.Add(triggerHitInfo.Target))
            {
                return;
            }

            HitsThisUse++;
            if (HitsThisUse <= MaxHits)
            {
                if (Owner.ShowingDebugs)
                {
                    Debug.Log($"{Item} beam hit {triggerHitInfo.Target.GetName} at {triggerHitInfo.Position}, " +
                              $"hits this use: {HitsThisUse}/{MaxHits}");
                }
            
                HitCallback(triggerHitInfo);
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (triggerExitInfo.Target != null)
            {
                _bufferedHits.Remove(triggerExitInfo.Target);
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
            _bufferedHits.Clear();
            _beamShooter.StartBeam(Owner.transform.forward);

        }

        private void StopBeam()
        {
            _beamActive = false;
            _beamShooter.StopBeam();
            _bufferedHits.Clear();
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

        [Header("Reporting")] public float interval; // how much time a target needs to be in a beam to be reported as a hit
        [Header("Reporting")] public float gracePeriod; // how much time a target can spend out of beam and not be excluded from report
        
        [Header("Hit Filtering")]
        [Min(0)] public float minDistanceBetweenHits; // Minimum distance on beam before same entity can be hit again
    }
}