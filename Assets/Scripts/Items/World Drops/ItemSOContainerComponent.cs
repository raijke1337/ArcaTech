using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Items
{
    [RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
    public class ItemSOContainerComponent : BaseEntity, IInteractible
    {
        [SerializeField] ItemSO c;
        public ItemSO Content { get => c; }
        public override void ApplyEffect(StatsEffect eff, IEquippable shield, out float current)
        {
            // item drops don't get destroyed
            current = 1;
        }

        public override void AcceptInteraction(IInteractible actor)
        {
            actor.AcceptInteraction(this);
            Destroy(gameObject);
        }

        private void OnEnable()
        {
            Instantiate(c.ItemPrefab,transform);
        }
    }
    
}