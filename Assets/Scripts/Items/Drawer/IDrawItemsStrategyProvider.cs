namespace Arcatech.Items
{
    public interface IDrawItemsStrategyProvider
    {
        public IDrawItemStrategy GetDrawStrategy { get ;}
        public bool NeedsRedraw { get; }
    }
}