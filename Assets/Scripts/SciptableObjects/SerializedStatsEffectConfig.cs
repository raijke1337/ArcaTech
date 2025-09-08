using System;
using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Triggers
{
    [CreateAssetMenu(fileName = "New Serialized Stats change effect", menuName = "Actions/Stat Change trigger cfg")]
    public class SerializedStatsEffectConfig : ScriptableObject
    {
        public SerializableGuid ID { get; private set; }
        public BaseStatType ChangedStat;
        public int InitialValue; // value change

        public int OverTimeValue; // how much dot or hot will be done
        public int OverTimeValueDuration; // over how much time
        public SerializedActionResult OnApplyResult;

        private void Awake()
        {
            ID =  new SerializableGuid();
        }

        private void OnValidate()
        {
            if (InitialValue ==0 &&  OverTimeValue ==0 && OverTimeValueDuration ==0)
            Debug.LogWarning($"{this} has 0 values");
        }
    }

}