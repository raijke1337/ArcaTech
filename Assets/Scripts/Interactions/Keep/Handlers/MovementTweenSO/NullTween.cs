using DG.Tweening;
using UnityEngine;

namespace Arcatech
{
    [CreateAssetMenu(fileName = "Null Tween", menuName = "Tweening/Null")]
    public class NullTween : SerializedDOTweener
    {
        protected override Tween Build(Transform t) => null;
    }
}