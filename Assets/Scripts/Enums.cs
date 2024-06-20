public enum Side
{
    PlayerSide,
    EnemySide
}

public enum BaseStatType : byte
{
    Health,
    Stamina,
    MoveSpeed,
    TurnSpeed,
}
public enum TriggerChangedValue : byte
{
    Health = 0,
    Energy = 10,
    Stamina = 20,
    MoveSpeed = 30,
    //TurnSpeed = 40,
    //Stagger = 50,
    //DodgeCharge = 60,
}

public enum GeneratorStatType
{
    Capacity,
    CapacityRegen,
    DamageAbsorbMultiplier
}

public enum EquipmentType
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

public enum UnitActionType
{
    Melee,
    Ranged,
    DodgeSkill,
    MeleeSkill,
    RangedSkill,
    ShieldSkill,
    Jump
}

public enum TriggerTargetType
{
    TargetsEnemies,
    TargetsUser,
    TargetsAllies
}


#region interface
public enum CursorType
{
    Menu,
    Explore,
    EnemyTarget,
    Item,
}
public enum DisplayValueType : byte
{
    Health = 0,
    Energy = 1,
    Stamina = 2
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


public enum EffectMoment
{
    OnStart,
    OnUpdate,
    OnCollision,
    OnExpiry
}
public enum SkillState
{
    Placer,
    AoE
}

#region AI
public enum ReferenceUnitType : byte
{
    Small = 0,
    Big = 1,
    Boss = 2,
    Focus = 252,
    Self = 253,
    Any = 254,
    Player = 255
}


#endregion
public enum ControlledItemState : byte
{
    None,
    Negative,
    NegativeToPositive,
    Positive,
    PositiveToNegative,
}

public enum FaceExpression : byte
{
    Neutral,
    Happy,
    Angry,
    Action,
    Bothered
}

