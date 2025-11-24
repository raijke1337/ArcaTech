using System;
using System.Collections.Generic;

public enum Side
{
    Unassigned,
    PlayerSide,
    EnemySide
}

public enum ItemSlot
{
    None,
    MeleeWeap,
    RangedWeap,
    Shield,
    Booster,
    Costume,
}

public enum UnitActionType : byte
{
    Melee,
    Ranged,
    DodgeSkill,
    MeleeSkill,
    RangedSkill,
    ShieldSkill,
    Jump,
    Movement,
    Use,
    None = 255
}

public enum TargetingType
{
    None,
    ApplyToSource,
    ApplyToEnemyTarget,
    ApplyToAlliedTarget,
    ApplyToAnyTargetExceptSource,
    ApplyToAnyTarget
}


#region interface
public enum CursorType
{
    Menu,
    Explore,
    EnemyTarget,
    Item,
}

public enum FontType
{
    Text,
    Button,
    Title
}



#endregion

public enum LevelType
{
    Menu,
    Scene,
    Game
}
