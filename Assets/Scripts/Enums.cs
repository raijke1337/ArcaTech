using System;
using System.Collections.Generic;

public enum Side
{
    Unassigned,
    PlayerSide,
    EnemySide
}
public enum Comparer
{
    Greater,
    Less
}
public enum BaseStatType : byte
{
    Health,
    Stamina,
    Energy
}

public enum ItemType
{
    None,
    MeleeWeap,
    RangedWeap,
    Shield,
    Booster,
    Costume,
    Modifier,
    Other = 255
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
    None = 255
}

public enum TargetingType
{
    None,
    OnlyUser,
    AnyUnit,
    AnyEnemy,
    AnyAlly
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
