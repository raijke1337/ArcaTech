using System;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    public abstract class InteractionEffect : MonoBehaviour
    {
        public abstract void Play(InteractionContext ctx);
        /// <summary>
        /// Если true, pipeline приостановится после Play 
        /// до тех пор, пока IsBlockingComplete не станет true.
        /// </summary>
        public virtual bool IsBlocking => false;
        public virtual bool IsBlockingComplete => true;

        /// <summary>
        /// Вызывается при принудительной отмене взаимодействия.
        /// </summary>
        public virtual void OnCancelled() { }
    }
}