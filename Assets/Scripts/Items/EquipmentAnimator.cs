using System.Collections.Generic;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    [RequireComponent(typeof(Animator))]
    public class EquipmentAnimator : ValidatedMonoBehaviour,IEquipmentPart
    {
        [SerializeField,Self] Animator animator;
        [SerializeField] private SerializedDictionary<StateMachineNotifyType, string> _animatorStrings;
        private Dictionary<StateMachineNotifyType, int> _hashes;

        private void Awake()
        {
            if (_animatorStrings == null) return;
            _hashes = new Dictionary<StateMachineNotifyType, int>();
            foreach (var pair in _animatorStrings)
            {
                _hashes[pair.Key] = Animator.StringToHash(pair.Value);
            }
        }
        
        public void TriggerState(StateMachineNotifyType notification)
        {
            if (_hashes.TryGetValue(notification, out var hash)) animator.SetTrigger(hash);
        }
    }
}