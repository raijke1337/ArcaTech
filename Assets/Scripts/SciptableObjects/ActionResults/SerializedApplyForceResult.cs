﻿using Arcatech.Units;
using ECM.Components;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New apply force result ", menuName = "Actions/Action Result/ApplyForce", order = 2)]
    public class SerializedApplyForceResult : SerializedActionResult
    {
        [SerializeField, Tooltip("Negative for backwards movement")] float Strength;
        [SerializeField] float Duration;
        public override IActionResult GetActionResult()
        {
            return new ApplyForceResult(Strength, Duration);
        }
    }

    public class ApplyForceResult : ActionResult
    {
        float _imp;
        float _t;
        public ApplyForceResult (float impulse, float dur)
        {
            _imp = impulse;
            _t = dur;
        }
        public override void ProduceResult(BaseEntity user, BaseEntity target, Transform place)
        {
            user.ApplyForceResultToUnit(_imp, _t);  
        }
    }


}