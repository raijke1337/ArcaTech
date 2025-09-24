using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;


namespace Arcatech.UI
{

    public class BarsContainersManager : MonoBehaviour, IStatUpdatesHandler
    {
        [SerializeField] StatBarContainerUIScript _barPrefab;
        [Space,SerializeField] SerializedDictionary<BaseStatType,ColorSet> _statColors;
        [SerializeField] Ease _barsEaseMethod;
        [SerializeField] float _barsEaseTime = 0.3f;
        [SerializeField, Range(0, 1), Tooltip("Delta change for visual effects")] float _barFlashTreschold = 0.2f;

        Dictionary<BaseStatType, StatBarContainerUIScript> _barsDict;

        bool init = false;

        private BaseStatType[] _typesCached;
        
        public void HandleStatsUpdate(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            // initial setup

            if (!init)
            {
                _barsDict = new();
                _typesCached = Enum.GetValues(typeof(BaseStatType)) as BaseStatType[];
                
                foreach (var enumKey in _typesCached)
                {
                    var b = Instantiate(_barPrefab, transform);
                    _barsDict[enumKey] = b;
                    b.gameObject.SetActive(false);
                }
                init = true;
            }

            foreach (var stat in _typesCached)
            {
                if (!stats.ContainsKey(stat))
                {
                    _barsDict[stat].gameObject.SetActive(false);
                }
                else
                {
                    if (!_barsDict[stat].Setup && stats[stat].Initialized)
                    {
                        SetupBar(stat, stats[stat]);  
                    }
                    _barsDict[stat].gameObject.SetActive(true);
                }
            }
        }

        void SetupBar(BaseStatType stat, StatValueContainer cont)
        {
            //Debug.Log($"Setting up bar for {stat} ");
            _barsDict[stat].LinkContainer(ref cont).SetColors(_statColors[stat]).
                SetEaseMethod(_barsEaseMethod).SetFillTime(_barsEaseTime).SetBrightGlowAT(_barFlashTreschold);
        }
    }
}