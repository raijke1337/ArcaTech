using Arcatech.Interactions;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.MiniGames
{
    public abstract class MiniGameBase : MonoBehaviour
    {
        public abstract void StartGame();
        public abstract void EndGame();
        public UnityEvent<InteractionState> OnGameCompleteResult;

        /// <summary>
        /// make sure to use only the success, fail, cancel here
        /// </summary>
        /// <param name="result"></param>
        protected void ReportResult(InteractionState result)
        {
            OnGameCompleteResult?.Invoke(result);
            OnGameCompleteResult?.RemoveAllListeners();
        }
    }

    [CreateAssetMenu(fileName = "MiniGameBase", menuName = "Game/MiniGame Package")]
    public class MiniGame : ScriptableObject
    {
        public MiniGameBase prefab;
        
    }
}