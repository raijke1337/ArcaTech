using Unity.Behavior;

/// <summary>
/// Категория токена атаки. У каждой категории свой лимит в CombatCoordinator.
/// </summary>
[BlackboardEnum]
public enum AttackTokenCategory
{
    /// <summary>Обычные атаки ближнего боя (удар паука, серия Боилера).</summary>
    Melee,

    /// <summary>Тяжёлые атаки и рывки (таран, вращение, рывок Боилера).</summary>
    Heavy,

    /// <summary>Атаки дальнего боя (луч дрона).</summary>
    Ranged,

    /// <summary>Захват Теилс. Выдаётся только во время её нокдауна, сильнейшему по тиру.</summary>
    Grab,
}
