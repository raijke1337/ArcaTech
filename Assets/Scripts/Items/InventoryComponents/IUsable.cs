using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.UI;
using Arcatech.Units;

namespace Arcatech
{
    public interface 
        IUsable : ICosted, IIconContent,IAffectsItemDisplay
    {
        public bool UsableIsReady();
        public void StartUse();
        public StateTransition GetStateTransition { get; }
        void DoUpdate(float delta);
        public void StopUse();
    }

   
}