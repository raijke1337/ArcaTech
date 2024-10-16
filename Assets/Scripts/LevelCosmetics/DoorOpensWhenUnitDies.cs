using Arcatech.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Level
{
    public class DoorOpensWhenUnitDies : MonoBehaviour
    {
        public BaseEntity Unit;
        private Vector3 start;
        [SerializeField] private Vector3 end;

        private void Start()
        {
            if (Unit == null) return;
            //Unit.BaseUnitDiedEvent += Unit_BaseUnitDiedEvent;
        }

        private void Unit_BaseUnitDiedEvent(BaseEntity arg)
        {
            start = transform.position;
            StartCoroutine(MoveDown());
        }

        private IEnumerator MoveDown()
        {
            float progress = 0;
            while (progress <= 1)
            {
                progress += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, progress);
                yield return null;
            }
            yield return null;
        }
    }
}