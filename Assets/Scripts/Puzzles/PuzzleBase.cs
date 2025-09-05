using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Puzzles
{
    public abstract class BasePuzzleComponent : MonoBehaviour
    {
        [SerializeField] protected Transform _gamePanel;
        public SimpleEventsHandler<bool> PuzzleResult;
        public bool PuzzleBusy { get; protected set; } = false;

        protected void ResultCallback(bool isSolved)
        {
            PuzzleResult?.Invoke(isSolved);
        }

        public void UI_ExitButton()
        {
            ResultCallback(false);
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            Assert.IsNotNull(_gamePanel);
#endif
            SetUpPuzzle();
        }
        protected abstract void SetUpPuzzle();


    }
}