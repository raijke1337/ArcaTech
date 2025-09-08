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


        public void HandleStatsUpdate(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            // initial setup

            if (!init)
            {
                var enumKeys = Enum.GetNames(typeof(BaseStatType));

                _barsDict = new();
                foreach (var enumKey in enumKeys)
                {
                    BaseStatType type = Enum.Parse<BaseStatType>(enumKey);
                    var b = Instantiate(_barPrefab, transform);
                    _barsDict[type] = b;
                    b.gameObject.SetActive(false);
                }
                init = true;
            }

            foreach (var pair in stats)
            {
                if (pair.Value.Initialized)
                {
                    _barsDict[pair.Key].gameObject.SetActive(true);
                    SetupBar(pair.Key, pair.Value);                    
                }
                else
                {
                    _barsDict[pair.Key].gameObject.SetActive(false);
                }
            }
        }

        void SetupBar(BaseStatType stat, StatValueContainer cont)
        {
            _barsDict[stat].LinkContainer(cont).SetColors(_statColors[stat]).
                SetEaseMethod(_barsEaseMethod).SetFillTime(_barsEaseTime).SetBrightGlowAT(_barFlashTreschold);
        }
    }
}