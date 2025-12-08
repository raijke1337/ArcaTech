using Arcatech.Skills;
using Arcatech.UI;
using Arcatech.Units;

namespace Arcatech
{
    public interface 
        IUsable : ICosted, IIconContent,IAffectsItemDisplay
    {
        public bool UsableIsReady();
        public StateTransition GetStateTransition { get; }
        void DoUpdate(float delta);
        void Notify(StateMachineNotifyType notifyType);
    }

   
}