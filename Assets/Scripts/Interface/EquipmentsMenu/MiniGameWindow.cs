using Arcatech.Interactions;
using Arcatech.MiniGames;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator))]
    public class MiniGameWindow : MinigamePanel
    {
        [SerializeField] TextMeshProUGUI Title;
        [SerializeField] TextMeshProUGUI Description;
        public override void LoadGame(MiniGameBase prefab, UnityAction<InteractionState> resultCallback)
        {
            base.LoadGame(prefab, resultCallback);
            Title.text = prefab.Description.Title;
            Description.text = prefab.Description.Text;
        }
    }
}