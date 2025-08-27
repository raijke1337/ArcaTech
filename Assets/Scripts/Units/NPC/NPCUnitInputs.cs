using Arcatech.BlackboardSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units.Inputs
{
    public class NPCUnitInputs : ControlInputsBaseOLD
    {
        protected void SetLookVector(Vector3 v) => InputsLookVector = v;
        protected void SetMoveVector(Vector3 v) => InputsMovementVector = v;
        protected override ControlInputsBaseOLD ControllerBindings(bool start)
        {
            return this;
        }

    }
}