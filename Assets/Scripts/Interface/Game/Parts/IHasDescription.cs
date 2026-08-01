using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Usables.Effects;

namespace Arcatech.UI
{
    public interface IHasDescription
    {
        public Description Description { get; }
    }

    public interface IActionIconContent : IHasDescription
    {
        public float Cooldown { get; }
        public float CurrentCooldown { get; }
        public int MaxCharges { get; }
        public int CurrentCharges { get; }
        public (ResourceStatType,int) GetCostDescription { get; }
    }
}