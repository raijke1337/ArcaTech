namespace Arcatech.Units
{
    /// <summary>
    /// interface indicates any class that is affected by changes in some stat
    /// </summary>
    public interface IDamageableComponent
    {
        public void Damage(int damage, BaseStatType stat);
    }
    /// <summary>
    /// interface will receive a command on 0 hp
    /// </summary>
    public interface IKillableComponent
    {
        public void Kill();
    }
/// <summary>
/// interface will receive a command at 0 stamina
/// </summary>
    public interface IStunnableComponent
    {
        public void Stun();
    }
/// <summary>
/// interface will handle the pause command
/// </summary>
    public interface IPausableComponent
    {
        public void Pause(bool pause);
    }
}