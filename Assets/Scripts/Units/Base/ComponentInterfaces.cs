using Arcatech.Stats;
using Arcatech.Triggers;

namespace Arcatech.Units
{
    /// <summary>
    /// called in stat update strategy by statsComp and maybe more
    /// </summary>
    public interface IDamageableComponent
    {
        public void Damage(float damage, ResourceStatType stat);
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
/// used by entity stats  mainly
/// </summary>
    public interface IEffectsTakerComponent
    {
        public void ApplyEffect(UsableEffect effect,BaseGameEntityComponent source);
    }
}