using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.UI;
using Arcatech.Units;

namespace Arcatech
{
    public interface 
        IUsable : ICosted, IActionTypeItem, IIconContent
    {
        public string UsableName { get; }
        public bool UsableIsReady();
        public bool StartUse();
        public StateTransition GetStateTransition { get; }
        void DoUpdate(float delta);
        public void StopUse();
    }

   
}