using Arcatech.Triggers;

namespace Arcatech.Units
{
    /// <summary>
    /// called in stat update strategy by statsComp and maybe more
    /// </summary>
    public interface IDamageableComponent
    {
        public void Damage(float damage, BaseStatType stat);
    }
    /// <summary>
    /// called in stat update strategy by statsComp and maybe more
    /// </summary>
    public interface IKillableComponent
    {
        public bool Killed { get; set; }
    }
    /// <summary>
    /// called in stat update strategy by statsComp and maybe more
    /// </summary>
    public interface IStunnableComponent
    {
        public bool Stunned { get; set; }
    }
/// <summary>
/// called by the pause helper on base entity
/// </summary>
    public interface IPausableComponent
    {
        public bool Paused { get; set; }
    }

/// <summary>
/// used by base entity  mainly
/// </summary>
    public interface IEffectsTakerComponent
    {
        public void ApplyEffect(StatsEffect effect,BaseGameEntityComponent source);
    }
}