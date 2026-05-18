using System.Collections;
using Arcatech.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    
    public class WaitForTextsExecutor : InteractionExecutor
    {
        [Header("Will wait for game ui texts to clear")] [SerializeField]
        private float extraDelay = 0f;

        [SerializeField] private InteractionState finalInteractionState = InteractionState.Success;
        
        private UnityAction<InteractionState> _onComplete;
        private GameInterfaceManager _ui;
        private Coroutine _cor;
        private void Start()
        {
            _ui = GameInterfaceManager.Instance;
        }

        private void OnDisable()
        {
            if (_cor != null) StopCoroutine(_cor);
        }

        public override void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete)
        {
            _onComplete =  onComplete;
            _cor = StartCoroutine(WaitForTextsToClear());
        }

        private IEnumerator WaitForTextsToClear()
        {
            yield return new WaitForSeconds(extraDelay);
            while (_ui.IsDialogueShowing) yield return null;
            _onComplete?.Invoke(finalInteractionState);
        }
        
    }
}