using DG.Tweening;
using UnityEngine;

namespace Arcatech.Triggers
{
    /// <summary>
    /// using dotween, add animation to items
    /// </summary>
    public class BouncyPickupDisplayComponent : MonoBehaviour
    {
        Tween movement;
        Tween rotation;
        private void Start()
        {
            movement = transform.DOLocalMoveY(transform.position.y + 1f, 1f).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.InOutSine);
            rotation = transform.DOLocalRotate(new Vector3(transform.eulerAngles.x, 359, transform.eulerAngles.z), 6, RotateMode.Fast).SetLoops(-1).SetEase(Ease.Linear);
        }
        private void OnDisable()
        {
            movement.Kill();
            rotation.Kill();
        }
    }


}
