using System;
using Arcatech.Managers;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ShowTextInteractionEffect : InteractionEffect
    {
        [SerializeField] DialoguePart textSuccess;
        [SerializeField] DialoguePart textFailure;
        [SerializeField] DialoguePart textCancel;

        public override void Play(InteractionContext ctx)
        {
            switch (ctx.State)
            {
                case InteractionState.Success:
                    GameInterfaceManager.Instance.HandleDialoguePart(textSuccess, true);
                    break;
                case InteractionState.Failure:
                    GameInterfaceManager.Instance.HandleDialoguePart(textFailure, true);
                    break;
                case InteractionState.Cancelled:
                    GameInterfaceManager.Instance.HandleDialoguePart(textCancel, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}