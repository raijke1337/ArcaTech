using DG.Tweening;
using UnityEngine;
namespace Arcatech
{
    public abstract class SerializedDOTweener : ScriptableObject
    {
        public Tween GetTween(Transform target) => Build(target);
        protected abstract Tween Build(Transform t);
    }


}