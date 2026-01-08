using Arcatech.Stats;
using Arcatech.Triggers;

namespace Arcatech.Units
{

    /// <summary>
    /// called in stat update strategy by statsComp and maybe more
    /// </summary>
    public interface IKillableComponent
    {
        public void SetKilled(IKillerComponent component, bool value);
    }
/// <summary>
/// this is used to track death sources in entities 
/// </summary>
    public interface IKillerComponent
    {
        public string KilledBy { get; }
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
/// takes the applied effect
/// </summary>
    public interface IAppliedEffectsTakerComponent<in T> where T : BaseAppliedEffect
    {
        /// <param name="effect"></param>
        /// <param name="source"></param>
        /// <returns>true if applied successfully</returns>
        public bool ApplyEffect(T effect,BaseGameEntityComponent source);
    }
}