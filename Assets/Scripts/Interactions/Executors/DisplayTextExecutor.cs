using Arcatech.Managers;
using Arcatech.Texts;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class DisplayTextExecutor : InteractionExecutor
    {
        [SerializeField] private DialoguePart text;

        public override void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete)
        {
            GameInterfaceManager.Instance.ShowDialoguePart(text, () => onComplete.Invoke(InteractionState.Success));
        }
    }
}