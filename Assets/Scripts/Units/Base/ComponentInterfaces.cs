using Arcatech.Triggers;

namespace Arcatech.Units
{
    /// <summary>
    /// called by active game unit component because it is a statsupdatehanlder for stats component
    /// </summary>
    public interface IDamageableComponent
    {
        public void Damage(int damage, BaseStatType stat);
    }
    /// <summary>
    /// called by active game unit component because it is a statsupdatehanlder for stats component
    /// </summary>
    public interface IKillableComponent
    {
        public void Kill();
        public bool Killed { get; }
    }
    /// <summary>
    /// called by active game unit component because it is a statsupdatehanlder for stats component
    /// </summary>
    public interface IStunnableComponent
    {
        public void Stun();
    }
/// <summary>
/// called by the pause helper on base entity
/// </summary>
    public interface IPausableComponent
    {
        public bool Paused { get; set; }
    }

    public interface IEffectsTakerComponent
    {
        public void ApplyEffect(StatsEffect effect,BaseGameEntityComponent source);
    }
}