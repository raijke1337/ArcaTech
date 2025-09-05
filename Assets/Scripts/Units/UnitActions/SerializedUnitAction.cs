using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Units
{
    [CreateAssetMenu(fileName = "Unit animation action", menuName = "Actions/Animation action")]
    public class SerializedUnitAction : ScriptableObject
    {
        [SerializeField] protected bool _locksMovement;
        [SerializeField] string _animationName;
        [SerializeField, Range(0.01f, 1f)] float _crossFadeTime = 0.1f;

        [SerializeField, Range(0.01f, 0.99f),
         Tooltip("at what percent of animation time action is considered complete")]
        protected float _exitTime = 0.75f;

        [SerializeField] NextActionSettings _nextAct;
        [SerializeField] SerializedActionResult[] _onStart;
        [SerializeField] SerializedActionResult[] _onExit;
        [SerializeField] SerializedActionResult[] _onFinish;

        public BaseUnitAction ProduceAction(ActiveGameUnitComponent unit, Transform place)
        {
            return BaseUnitAction.BuildAction(unit, _locksMovement, _nextAct, _animationName, _exitTime, _onStart,
                _onFinish, _onExit, place, _crossFadeTime);
        }

        private void OnValidate()
        {
            Assert.IsFalse((_onStart == null && _onExit == null && _onFinish == null));
        }
    }
}