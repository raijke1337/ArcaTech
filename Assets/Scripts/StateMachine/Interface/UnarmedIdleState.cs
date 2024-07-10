﻿// a predicate is a function that tests a condition and then returns a bool
using Arcatech.Units;
using ECM.Components;
using UnityEngine;

namespace Arcatech.StateMachine
{
    public class UnarmedIdleState : BaseState
    {
        public UnarmedIdleState(CharacterMovement movement, ControlledUnit unit, Animator playerAnimator) : base(movement, unit, playerAnimator)
        {
        }

        public override void OnEnterState()
        {
            playerAnimator.CrossFade(UnarnedStandingIdleHash, crossFadeDuration);
        }
        public override void FixedUpdate(float d)
        {
        }
        public override void Update(float d)
        {

        }

        public override void HandleCombatAction(UnitActionType action)
        {

        }

        public override void OnLeaveState()
        {

        }

    }

}

