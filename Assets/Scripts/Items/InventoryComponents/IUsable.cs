using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.UI;
using Arcatech.Units;

namespace Arcatech
{
    public interface IUsable : ICosted, IActionTypeItem, IIconContent
    {
        public string UsableName { get; }
        public bool UsableIsReady();
        public UnitState Use();
        void DoUpdate(float delta);
    }

   
}