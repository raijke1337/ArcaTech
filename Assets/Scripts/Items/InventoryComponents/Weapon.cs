using System;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class Weapon : Equipment, IWeapon
    {

        private SerializedStatsEffectConfig _cost;
        protected BaseEquipmentComponent _weaponGameobject;
        public StatsEffect GetCost 
            {
                get
            {
                if (_cost != null) return new(_cost);
                else return null;
            }
        }
        public IDrawItemStrategy DrawStrategy { get; protected set; }
        public UnitActionType UseActionType { get; protected set; }
        public IWeaponUseStrategy UseStrategy { get; protected set; }

        protected override void CollectUsables(EquipSO cfg)
        {
            cachedUsables = new List<IUsable>();
            if (cfg.Skill != null)
            {
                GetUsables.Add(cfg.Skill.CreateSkill(Owner, DisplayItem, Type));
            }
            cachedUsables.Add(this);
        }
        public Weapon(WeaponSO cfg, BaseGameEntityComponent ow) : base(cfg, ow)
        {
            _weaponGameobject = DisplayItem as BaseEquipmentComponent;

            _cost = cfg.Cost;
           // AnimationSet = cfg.WeaponType;
            switch (Type)
            {
                case ItemType.MeleeWeap:
                    UseActionType = UnitActionType.Melee;
                    break;
                case ItemType.RangedWeap:
                    UseActionType = UnitActionType.Ranged;
                    break;
            }
            DrawStrategy = cfg.DrawStrategy;
            UseStrategy = cfg.WeaponUseStrategy.ProduceStrategy(Owner, cfg,_weaponGameobject);
        }

        public bool TryUseItem(EntityStatsComponent stats, out BaseUnitAction act)
        {
            act = null;
            bool ok = false;
            if (stats.CanApplyCost(GetCost) && UseStrategy.TryUseUsable(out act))
            {
                stats.ApplyCost(GetCost);
                ok = true;
            }

            return ok;
        }
        public bool CanUseItem(EntityStatsComponent stats)
        {
            return stats.CanApplyCost(GetCost) && UseStrategy.CanUseUsable();
        }
        public void DoUpdate(float delta)
        {
            UseStrategy.UpdateUsable(delta);
           // EventBus<UpdateIconEvent>.Raise(new UpdateIconEvent(this, Owner));
        }

        public string UsableName { get => Config.Description.Text; }

        #region UI

        public override Sprite Icon
        {
            get
            {
                try
                {
                    return Config.Description.Picture;

                }
                catch (Exception e)
                {
                    Console.WriteLine($"missing picture in {Config}");
                    return null;
                } 
            }
        }

        public override float FillValue => UseStrategy.FillValue;

        public override string IconValue => UseStrategy.IconValue;


        #endregion
    }
}