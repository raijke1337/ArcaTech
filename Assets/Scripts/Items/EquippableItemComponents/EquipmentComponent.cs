using System;
using Arcatech.Actions;
using Arcatech.Stats;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    public class EquipmentComponent : ValidatedMonoBehaviour,IActionStateItem, IDamageableComponent, IKillableComponent
    {
        [SerializeField] protected Transform spawner;
        public Transform Spawner => spawner;
        
        [SerializeField] SerializedActionResult[] onDamaged;
        [SerializeField] SerializedActionResult[] onKilled;

        private IActionResult[] _killedRes;
        private IActionResult[] _damagedRes;

        /// <summary>
        /// should only get damaged if it has a stats component attached.
        /// so costume parts. weapons do not get damage.
        /// </summary>
        private void Start()
        {
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
            
        }
        #region IDamageableComponent
        public void Damage(float damage, BaseStatType stat)
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


        private void OnEnable()
        {
            if (spawner == null)
            {
                Debug.LogWarning($"Spawner not set in {this}");
                spawner = transform;
            }
        }
    }

}


