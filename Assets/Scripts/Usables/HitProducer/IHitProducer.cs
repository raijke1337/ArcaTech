using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Usables
{
    /// <summary>
    /// delivery 
    /// </summary>
    public interface IHitProducer : ITriggerNotificationReceiver
    {
        void Initialize();
        event UnityAction<TriggerHitInfo> Hit;
        void Cleanup(); 
    }

    public abstract class SerializedHitProducer : ScriptableObject
    {
        public abstract IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item);
    }

}