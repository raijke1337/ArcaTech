﻿using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Skills
{
    public class Skill : ISkill
    {
        #region interface
        public BaseEntity Owner { get ; set; }
       // protected SerializedSkill Config { get; }
        public UnitActionType UseActionType { get;  }
        public StatsEffect GetCost => new(_cost);
        protected SerializedStatsEffectConfig _cost;
        public IDrawItemStrategy DrawStrategy { get; }

        #endregion

        protected SkillUsageStrategy Strategy { get; }


        public Skill(IDrawItemStrategy s, SerializedSkill settings, BaseEntity owner, BaseEquippableItemComponent item)
        { 

            Owner = owner;
            if (settings == null) return; // placeholder maybe TODO - for items without skills

            UseActionType = settings.UnitActionType;
            _cost = settings.Cost;
            Strategy = settings.UseStrategy.ProduceStrategy(Owner, settings, item);
            DrawStrategy = s;
        }

        public bool TryUseItem(UnitStatsController stats, out BaseUnitAction onUse)
        {
            onUse = null;
            if (stats.CanApplyCost(GetCost) && Strategy.TryUseUsable(out onUse))
            {
                stats.ApplyCost(GetCost);
                return true;
            }
            else return false;
        }
        public bool CanUseItem(UnitStatsController stats)
        {
            return stats.CanApplyCost(GetCost) && Strategy.CanUseUsable();
        }


        public void DoUpdate(float delta)
        {
            Strategy.UpdateUsable(delta);
            EventBus<UpdateIconEvent>.Raise(new UpdateIconEvent(this, Owner));
        }




        #region UI


        public Sprite Icon => Strategy.Icon;

        public float FillValue => Strategy.FillValue;

        public string Text => Strategy.Text;


        #endregion
    }

}
