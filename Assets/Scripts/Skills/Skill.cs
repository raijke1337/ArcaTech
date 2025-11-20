using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Skills
{
    public class Skill : IUsable,IAffectsItemDisplay
    {
        #region interface
        public EntityStateMachineComponent Owner { get ; set; }
       // protected SerializedSkill Config { get; }
        public UnitActionType UseActionType { get;  }
        public StatsEffect GetCost { get; }

        public IDrawItemStrategy DrawStrategy { get; }

        #endregion

        protected SkillUsageStrategy Strategy { get; }

        public string UsableName { get; }
        public Skill(SerializedStateTransition state, IDrawItemStrategy s, SerializedSkill settings, BaseGameEntityComponent owner, EquipmentComponent item, ItemType type)
        { 
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

            GetCost = settings.Cost;
            Strategy = settings.UseStrategy.ProduceStrategy(Owner, settings, item);
            DrawStrategy = s;
            UsableName = settings.Description.Text;
            GetStateTransition = state.Build();
        }


        public bool UsableIsReady()
        {
            return Strategy.CanUseUsable();
        }

        public bool StartUse()
        {
            return Strategy.UseUsable();
        }

        public StateTransition GetStateTransition { get; }


        public void DoUpdate(float delta)
        {
            Strategy.UpdateUsable(delta);
        }

        public void StopUse()
        {
            Strategy.StopUsingUsable();
        }


        #region UI


        public Description Description => Strategy.Description;

        public float FillValue => Strategy.FillValue;

        public string IconNumber => Strategy.IconNumber;


        #endregion
    }

}
