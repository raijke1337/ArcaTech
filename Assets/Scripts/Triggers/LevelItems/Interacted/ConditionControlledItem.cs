using Arcatech.Level;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Arcatech.Level
{
    public abstract class ConditionControlledItem : MonoBehaviour, IConditionControlled
    {
        public virtual void SetState(bool newstate)
        {
            OnSetState(newstate);
        }
        protected abstract void OnSetState(bool newstate);
    }



}