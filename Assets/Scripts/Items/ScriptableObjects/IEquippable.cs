public interface IEquippable
{
    /// <summary>
    /// item was equipped on player
    /// </summary>
    void OnEquip();
    /// <summary>
    /// item removed from inventory
    /// </summary>
    void OnRemove();
}