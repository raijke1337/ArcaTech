using DG.Tweening;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ItemMovesWithTween : MonoBehaviour
    {
        
        [SerializeField] SerializedDOTweener tween;
        Tween cached;

        private void OnEnable()
        {
            cached = tween.GetTween(transform).Play();
        }
    }
}