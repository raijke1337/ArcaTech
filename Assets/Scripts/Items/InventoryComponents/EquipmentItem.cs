using Arcatech.Effects;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units;
using System;
using UnityEngine;
namespace Arcatech.Items
{


    [Serializable]
    public class EquipmentItem : InventoryItem, ICost
    {
        public SerializedStatModConfig[] ItemStats { get; protected set; }
        public SerizalizedSkillConfiguration Skill { get; protected set; }
        public EffectsCollection Effects { get; private set; }

        [SerializeField] protected BaseEquippableItemComponent _prefab;

        protected BaseEquippableItemComponent _instantiated;
        public RuntimeAnimatorController AnimatorController { get; protected set; }
        private SerializedStatsEffectConfig _cost;
        public StatsEffect Cost
        {
            get
            {
                return new StatsEffect(_cost);
            }
        }

        public EquipmentItem(Item cfg, BaseUnit ow) : base(cfg, ow)
        {
            if (cfg is Equip e)
            {
                Skill = e.Skill;
                _prefab = e.ItemPrefab;
                Effects = new EffectsCollection(e.Effects);
                AnimatorController = e.AnimatorController;
                _cost = e.CostTrigger;
                ItemStats = new SerializedStatModConfig[e.StatMods.Length];

                for (int i = 0; i < e.StatMods.Length; i++)
                {
                    ItemStats[i] = e.StatMods[i];
                }
                this.ItemPlaceType = e.ItemPlaceType;
            }
        }

        public ItemPlaceType ItemPlaceType { get; protected set; }

        public BaseEquippableItemComponent GetInstantiatedPrefab()
        {
            if (_instantiated == null)
            {
                GenerateObject();

                Effects.ParentTransform=_instantiated.transform;
            }
            if (!_instantiated.gameObject.activeSelf)
            {
                _instantiated.gameObject.SetActive(true);
            }
            return _instantiated;
        }
        protected virtual void GenerateObject()
        {
            _instantiated = GameObject.Instantiate(_prefab);
            _instantiated.Owner = Owner;

        }
        public void SetItemEmpty(Transform parent)
        {
            if (_instantiated == null)
            {
                GenerateObject();
            }
            _instantiated.transform.SetParent(parent, false);
        }


        public void OnEquip()
        {
            if (_instantiated != null) _instantiated.gameObject.SetActive(true);
            else
            {
                GenerateObject();
            }
        }        
        public void OnEquip(Transform parent)
        {
            if (_instantiated != null) _instantiated.gameObject.SetActive(true);
            else
            {
                GenerateObject();
            }
            _instantiated?.transform.SetParent(parent, false);
        }
        public void OnUnequip()
        {
            if (_instantiated != null) _instantiated.gameObject.SetActive(false);
        }



        public virtual void DoUpdates(float d)
        { }

    }


}