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

        private StatsEffect _cost;
        public StatsEffect GetCost => _cost != null ? _cost : null;
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
        public Weapon(WeaponSO cfg, BaseGameEntityComponent ow,SerializedStateTransition state) : base(cfg, ow)
        {
            UsableName = cfg.Description.Title;
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
            UseStrategy = cfg.WeaponUseStrategy.ProduceStrategy(Owner, cfg,DisplayItem);
            GetStateTransition = state.Build();
        }

        public bool StartUse()
        {
            return UseStrategy.UseUsable();
        }

        public StateTransition GetStateTransition { get; private set; }

        public bool UsableIsReady()
        {
            return UseStrategy.CanUseUsable();
        }

        public void DoUpdate(float delta)
        {
            UseStrategy.UpdateUsable(delta);
           // EventBus<UpdateIconEvent>.Raise(new UpdateIconEvent(this, Owner));
        }

        public void StopUse()
        {
            UseStrategy.StopUsingUsable();
        }

        public string UsableName { get; }

        #region UI
        public override float FillValue => UseStrategy.FillValue;
        public override string IconNumber => UseStrategy.IconNumber;

        #endregion
    }
}