using Arcatech.Skills;
using Arcatech.UI;
using Arcatech.Units;

namespace Arcatech
{
    public interface 
        IUsable : ICosted, IActionIconContent,IAffectsItemDisplay,IHasDescription
    {
        public bool UsableIsReady();
        public StateTransition GetStateTransition { get; }
        void DoUpdate(float delta);
        void Notify(StateMachineNotifyType notifyType);
        void CleanUp();
    }

    public interface IUsableComponent
    {
        public void OnChangeUsableState(StateMachineNotifyType notification);
    }
   
}