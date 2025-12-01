using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arcatech.Triggers;
using Arcatech.Units;
using Arcatech.Usables;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items
{
    public class BeamHitsProducer : IHitProducer
    {
        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            throw new NotImplementedException();
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
            throw new NotImplementedException();
        }

        public void OnChangeState(StateMachineNotifyType info)
        {
            throw new NotImplementedException();
        }

        public event UnityAction<TriggerHitInfo> Hit;
    }
    
    
    
}