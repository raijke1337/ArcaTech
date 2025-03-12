using Arcatech.Stats;
using Arcatech.Triggers;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Items
{
    [RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
    public class ItemSOContainerComponent : MonoBehaviour, IInteractible
    {
        ItemSO c;
        public ItemSO Content
        {
            get { return c; }
            set
            { 
                c = value;
                Instantiate(c.ItemPrefab, transform);
            }
        }
        Rigidbody _r;
        public Vector3 Position => transform.position;

        public string UnitName => Content.Description.Title;

        public IReadOnlyDictionary<BaseStatType, StatValueContainer> GetDisplayValues => null;

        public void AcceptInteraction(IInteractible actor)
        {
            actor.AcceptInteraction(this);
            Destroy(gameObject);
        }
        private void Awake()
        {
            _r = GetComponent<Rigidbody>();
            _r.AddForce(Vector3.up*10,ForceMode.Impulse);
        }
    }
    
}