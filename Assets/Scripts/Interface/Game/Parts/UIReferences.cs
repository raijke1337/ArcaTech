using System.Collections.Generic;

namespace Arcatech.UI
{
    public static class UIReferences
    {
        public static readonly IReadOnlyDictionary<UnitActionType, string> Hotkeys =
            new Dictionary<UnitActionType, string>
            {
                { UnitActionType.None, string.Empty },

                { UnitActionType.Melee, "LMB" },
                { UnitActionType.MeleeSkill, "Q" },

                { UnitActionType.Ranged, "RMB" },
                { UnitActionType.RangedSkill, "E" },

                { UnitActionType.ShieldSkill, "R" },
                { UnitActionType.DodgeSkill, "SHIFT" },
                { UnitActionType.Jump, "SPACE" },
                { UnitActionType.Use, "H" }
            };


        public static readonly List<UnitActionType> ShownUsableTypes =
            new List<UnitActionType>()
            {
                UnitActionType.MeleeSkill,
                UnitActionType.RangedSkill,
                UnitActionType.ShieldSkill,
                UnitActionType.DodgeSkill
            };
    }
}