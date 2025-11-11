using System;
using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(menuName = "States/Actions/ApplyJump")]
    public class ApplyJumpResultSO : SerializedActionResult
    {
        [SerializeField] bool jumpState = true;
        public override ActionResult BuildActionResult()
        {
            return new ApplyJumpResult(jumpState);
        }
    }


    public class ApplyJumpResult : ActionResult
    {
        
        public ApplyJumpResult(bool jumpState) =>  this.jumpState = jumpState;
        bool jumpState;
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target,
            Transform place)
        {
            if (user == null) return false;
            var mover = user.GetComponent<IJump>();
            if (mover == null) return false;
            mover.JumpCommand = jumpState;
            return true;
        }
    }
}