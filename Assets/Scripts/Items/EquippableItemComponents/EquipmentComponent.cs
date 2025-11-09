using System;
using Arcatech.Actions;
using Arcatech.Stats;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [RequireComponent(typeof(Animator))]
    public class EquipmentComponent : ValidatedMonoBehaviour,IActionStateItem, IDamageableComponent, IKillableComponent,ISpawnerProvider
    {
        [SerializeField] protected Transform spawner;
        [SerializeField,Self] protected Animator animator;
        public Transform SpawnPoint => spawner;
        
        [SerializeField] SerializedActionResult[] onDamaged;
        [SerializeField] SerializedActionResult[] onKilled;

        private IActionResult[] _killedRes;
        private IActionResult[] _damagedRes;

        [SerializeField] private string animatorStateStartedTrigger;
        [SerializeField] private string animatorStateExitTimeTrigger;
        [SerializeField] private string animatorStateCompletedTrigger;

        private int _startHash;
        int _exitHash;
        int _completedHash;
        
        /// <summary>
        /// should only get damaged if it has a stats component attached (costume parts)
        /// </summary>
        private void Start()
        {
            _startHash = Animator.StringToHash(animatorStateStartedTrigger);
            _exitHash = Animator.StringToHash(animatorStateExitTimeTrigger);
            _completedHash = Animator.StringToHash(animatorStateCompletedTrigger);
            
            if (onDamaged != null && onDamaged.Length > 0)
            {
                _damagedRes = new IActionResult[onDamaged.Length];
                for (int i = 0; i < onDamaged.Length; i++)
                {
                    _damagedRes[i] = onDamaged[i].BuildActionResult();
                }
            }

            if (onKilled != null && onKilled.Length > 0)
            {
                _killedRes = new IActionResult[onKilled.Length];
                for (int i = 0; i < onKilled.Length; i++)
                {
                    _killedRes[i] = onKilled[i].BuildActionResult();
                }
            }
        }

        public virtual void HandleActionState(UnitActionState s)
        {
            switch (s)
            {
                case UnitActionState.Started: animator.Play(_startHash);
                    break;
                case UnitActionState.ExitTime: animator.Play(_exitHash);
                    break;
                case UnitActionState.Completed: animator.Play(_completedHash);
                    break;
            }
        }
        #region IDamageableComponent
        public void Damage(float damage, ResourceStatType stat)
        {
            if (_damagedRes is not { Length: > 0 }) return;
            foreach (var r in _damagedRes)
            {
                r.ProduceResult(null,null,spawner);
            }
        }
    #endregion
        #region IKillableComponent
        private bool _k;

        public bool Killed
        {
            get => _k;
            set
            {
                _k = value;
                if (_killedRes is not { Length: > 0 }) return;
                foreach (var r in _killedRes)
                {
                    r.ProduceResult(null,null,spawner);
                }
                gameObject.SetActive(false);
            }

        }     
        #endregion


        protected virtual void OnEnable()
        {
            if (spawner == null)
            {
                Debug.LogWarning($"Spawner not set in {this}");
                spawner = transform;
            }
        }

    }

}


