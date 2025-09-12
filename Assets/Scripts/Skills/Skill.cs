using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Skills
{
    public class Skill : IUsable,IAffectsItemDisplay
    {
        #region interface
        public ActiveGameUnitComponent Owner { get ; set; }
       // protected SerializedSkill Config { get; }
        public UnitActionType UseActionType { get;  }
        public StatsEffect GetCost => new(_cost);
        protected SerializedStatsEffectConfig _cost;
        public IDrawItemStrategy DrawStrategy { get; }

        #endregion

        protected SkillUsageStrategy Strategy { get; }

        public string UsableName { get; }
        public Skill(IDrawItemStrategy s, SerializedSkill settings, BaseGameEntityComponent owner, BaseItemComponent item, ItemType type)
        { 

            Owner = owner.GetComponent<ActiveGameUnitComponent>();
            if (settings == null) return; // placeholder maybe TODO - for items without skills

            switch (type)
            {
                case ItemType.None:
                    break;
                case ItemType.MeleeWeap:
                    UseActionType = UnitActionType.MeleeSkill;
                    break;
                case ItemType.RangedWeap:
                    UseActionType = UnitActionType.RangedSkill;
                    break;
                case ItemType.Shield:
                    UseActionType = UnitActionType.ShieldSkill;
                    break;
                case ItemType.Booster:
                    UseActionType = UnitActionType.DodgeSkill;
                    break;
                    default :
                    Debug.LogWarning($"Failed to assign action type for {settings.Description.Title} assigned to {item} {type}");
                    break;
            }

            _cost = settings.Cost;
            Strategy = settings.UseStrategy.ProduceStrategy(Owner, settings, item);
            DrawStrategy = s;
            UsableName = settings.Description.Text;
        }

        public bool TryUseItem(EntityStatsComponent stats, out BaseUnitAction onUse)
        {
            onUse = null;
            if (stats.CanApplyCost(GetCost) && Strategy.TryUseUsable(out onUse))
            {
                stats.ApplyCost(GetCost);
                return true;
            }
            else return false;
        }
        public bool CanUseItem(EntityStatsComponent stats)
        {
            return stats.CanApplyCost(GetCost) && Strategy.CanUseUsable();
        }


        public void DoUpdate(float delta)
        {
            Strategy.UpdateUsable(delta);
         //   EventBus<UpdateIconEvent>.Raise(new UpdateIconEvent(this, Owner));
        }




        #region UI


        public Sprite Icon => Strategy.Icon;

        public float FillValue => Strategy.FillValue;

        public string IconValue => Strategy.IconValue;


        #endregion
    }

}
