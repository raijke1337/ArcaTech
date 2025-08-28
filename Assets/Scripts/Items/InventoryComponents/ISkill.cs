using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Units;

namespace Arcatech.Skills
{

    public interface IAffectsItemDisplay
    {
        public IDrawItemStrategy DrawStrategy { get; }
    }
   
}