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
    public class EquipmentComponent : ValidatedMonoBehaviour,IDamageableComponent, IKillableComponent,ISpawnerProvider
    {
        [SerializeField] protected Transform spawner;
        [SerializeField,Self] protected Animator animator;
        public Transform EffectSpawn => spawner;
        
        [SerializeField] SerializedActionResult[] onDamaged;
        [SerializeField] SerializedActionResult[] onKilled;

        private ActionResult[] _killedRes;
        private ActionResult[] _damagedRes;

        [SerializeField] private string animatorStateStartedTrigger;
        [SerializeField] private string animatorStateExitTimeTrigger;

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
            
            if (onDamaged != null && onDamaged.Length > 0)
            {
                _damagedRes = new ActionResult[onDamaged.Length];
                for (int i = 0; i < onDamaged.Length; i++)
                {
                    _damagedRes[i] = onDamaged[i].BuildActionResult();
                }
            }

            if (onKilled != null && onKilled.Length > 0)
            {
                _killedRes = new ActionResult[onKilled.Length];
                for (int i = 0; i < onKilled.Length; i++)
                {
                    _killedRes[i] = onKilled[i].BuildActionResult();
                }
            }
        }

        #region IDamageableComponent
        public void Damage(float damage, ResourceStatType stat)
        {
            if (_damagedRes is not { Length: > 0 }) return;
            foreach (var r in _damagedRes)
            {
                r.ProduceResult(null,null,spawner.position,spawner.rotation);
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
                    r.ProduceResult(null,null,spawner.position,spawner.rotation);
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


