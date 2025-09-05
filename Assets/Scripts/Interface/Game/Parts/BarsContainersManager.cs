using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using System.Collections.Generic;
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
            if (!init)
            {
                _barsDict ??= new();
                foreach (var stat in stats)
                {
                    _barsDict[stat.Key] = Instantiate(_barPrefab, this.transform).
                    LinkContainer(stat.Value).
                    SetColors(_statColors[stat.Key]).
                    SetEaseMethod(_barsEaseMethod).
                    SetFillTime(_barsEaseTime);
                }
                init = true;
            }
        }
    }
}