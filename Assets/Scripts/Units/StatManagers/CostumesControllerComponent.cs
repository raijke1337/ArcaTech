using Arcatech.EventBus;
using CartoonFX;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units
{
    public class CostumesControllerComponent : MonoBehaviour
    {
        public static CostumesControllerComponent instance;
        private void Awake()
        {
            instance = this;
        }

        [SerializeField] private List<GameObject> _parts;
        [SerializeField] CFXR_Effect[] BreakEffects;
        public void OnBreak()
        {
            if (_parts != null)
            {
                if (_parts.Count > 0)
                {
                    var ind = Random.Range(0, _parts.Count - 1);
                    var s = _parts[ind];


                    if (BreakEffects != null && BreakEffects.Length > 0)
                    {
                        EventBus<VFXEvent>.Raise(new VFXEvent(BreakEffects[Random.Range(0,BreakEffects.Length-1)], s.transform));
                    }
                    s.SetActive(false);
                    _parts.RemoveAt(ind);
                }
            }
        }

    }

}