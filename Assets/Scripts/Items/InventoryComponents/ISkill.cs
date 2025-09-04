using Arcatech.Items;

namespace Arcatech.Skills
{

    public interface IAffectsItemDisplay
    {
        public IDrawItemStrategy DrawStrategy { get; }
    }
   
}