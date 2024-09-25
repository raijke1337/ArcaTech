﻿using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Units
{
    [CreateAssetMenu(fileName = "Unit animation action",menuName = "Actions/Animation action")]
    public class SerializedUnitAction : ScriptableObject
    {
        [SerializeField] protected bool _locksMovement;
        [SerializeField] string _animationName;
        [SerializeField,Range(0f,1f),Tooltip("at what percent of animation time action is considered complete")] protected float _exitTime;
        [SerializeField] NextActionSettings _nextAct;
        [SerializeField] SerializedActionResult _onStart;
        [SerializeField] SerializedActionResult _onExit;
        [SerializeField] SerializedActionResult _onFinish;
        public BaseUnitAction ProduceAction(BaseEntity unit)
        {
            return BaseUnitAction.BuildAction(unit,_locksMovement,_nextAct,_animationName,_exitTime,_onStart,_onFinish);
        }

        private void OnValidate()
        {
            Assert.IsNotNull(_animationName);
        }

    }
}