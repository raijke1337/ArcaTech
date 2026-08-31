using System;
using Arcatech.SaveSystem;
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
        public virtual bool ReplayOnLoad => false;

        
        /// <summary>
        /// Вызывается при принудительной отмене взаимодействия.
        /// </summary>
        public virtual void OnCancelled() { }

       // public abstract void OnLoadLevelState(ProgressItemState stateToLoad);
    }
}