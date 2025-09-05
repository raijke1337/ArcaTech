using DG.Tweening;
using UnityEngine;

namespace Arcatech.Triggers
{
    /// <summary>
    /// using dotween, add animation to items
    /// </summary>
    public class TweenedEntityComponent : MonoBehaviour
    {
        [SerializeField] SerializedDOTweener tween;
        Tween t;
        private void Start()
        {
            t = tween.GetTween(transform);
        }
        private void OnDestroy()
        {
            t.Kill();
        }
    }


}
