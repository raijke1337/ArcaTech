using AYellowpaper.SerializedCollections;
using com.cyborgAssets.inspectorButtonPro;
using DG.Tweening;
using UnityEngine;

namespace Arcatech.Level
{
    public class ItemMovesWhenActivated : ConditionControlledItem
    {
        [SerializeField] SerializedDictionary<Rigidbody,Vector3> movingItemsToVector3;
        [SerializeField] SerializedDictionary<Rigidbody,Vector3> rotateItemsToVector3;
        Vector3[] moveFrom;
        Vector3[] rotateFrom;

        [SerializeField] bool loop = true;  

        [SerializeField] float movetime = 1f;
        [SerializeField] Ease ease = Ease.InOutQuad;

        bool activated = false; // activated ??

        private void OnValidate()
        {
        }

        protected override void OnSetState(bool newstate)
        {

            if (activated && !newstate) return;
            {
                // case - disabled something
                // NYI

            }
            if (newstate)
            {
                int index = 0;
                moveFrom = new Vector3[movingItemsToVector3.Count];
                rotateFrom = new Vector3[rotateItemsToVector3.Count];
                foreach (var item in movingItemsToVector3.Keys)
                {
                    moveFrom[index] = item.position;
                    if (loop)
                    {                        
                        item.DOMove(moveFrom[index] + movingItemsToVector3[item], movetime).SetLoops(-1, LoopType.Yoyo).SetEase(ease);
                    }
                    else
                    {
                        item.DOMove(moveFrom[index] + movingItemsToVector3[item], movetime).SetEase(ease);
                    }
                } 
                foreach (var item in rotateItemsToVector3.Keys)
                {
                    rotateFrom[index] = item.transform.eulerAngles;
                    if (loop)
                    {
                        item.DORotate(rotateFrom[index] + rotateItemsToVector3[item], movetime).SetLoops(-1, LoopType.Yoyo).SetEase(ease);
                    }
                    else
                    {
                        item.DORotate(rotateFrom[index] + rotateItemsToVector3[item], movetime).SetEase(ease);
                    }
                }
                
            }
        }
    }

}