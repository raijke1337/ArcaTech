using Arcatech.Interactions;
using Arcatech.MiniGames;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator))]
    public class MiniGameWindow : ValidatedMonoBehaviour
    {
        [SerializeField,Self] public PanelAnimator Animator;
        [SerializeField] TextMeshProUGUI Title;
        [SerializeField] TextMeshProUGUI Description;
        [SerializeField] private Transform gamePanelParent;
        private MiniGameBase _instantiatedGame;
        private UnityAction<InteractionState> cb;
        
        public void LoadGame(MiniGameBase prefab, UnityAction<InteractionState> resultCallback)
        {
            if (_instantiatedGame != null) Destroy(_instantiatedGame.gameObject);
            _instantiatedGame = Instantiate(prefab,gamePanelParent);
            cb = resultCallback;
            _instantiatedGame.StartGame();
            Title.text = prefab.Description.Title;
            Description.text = prefab.Description.Text;
            _instantiatedGame.onGameCompleteResult.AddListener(HandleGameResult);
        }

        void HandleGameResult(InteractionState state)
        {
            cb.Invoke(state);
            cb = null;
        }
    }
}