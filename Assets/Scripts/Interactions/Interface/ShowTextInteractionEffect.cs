using System;
using Arcatech.Managers;
using Arcatech.SaveSystem;
using Arcatech.Texts;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ShowTextInteractionEffect : InteractionEffect
    {
        [SerializeField] private SerializedDictionary<InteractionState, DialoguePart> texts;
        [SerializeField] bool blockUntilTextCompletes = false;

        public override bool IsBlocking => blockUntilTextCompletes;

        public override void Play(InteractionContext ctx)
        {
            if (texts != null && texts.TryGetValue(ctx.State, out DialoguePart txt))
            {
                GameInterfaceManager.Instance.ShowDialoguePart(txt);
            }
        }

        public override bool IsBlockingComplete => !GameInterfaceManager.Instance.IsDialogueShowing;
        public override void OnLoadLevelState(ProgressItemState stateToLoad)
        {
            // noop 
        }
    }
}