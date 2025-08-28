using Arcatech.Stat;
using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.UI
{
    public class BarsContainersManager : MonoBehaviour
    {



        [SerializeField] StatBarContainerUIScript _barPrefab;
        [Space,SerializeField] SerializedDictionary<BaseStatType,ColorSet> _statColors;
        [SerializeField] Ease _barsEaseMethod;
        [SerializeField] float _barsEaseTime = 0.3f;

        Dictionary<BaseStatType, StatBarContainerUIScript> _barsDict;


        public void LinkStats(EntityStatsComponent stats)
        {
            _barsDict ??= new();
            foreach (var stat in stats.GetAllStats)
            {
                _barsDict[stat.Key] = Instantiate(_barPrefab, this.transform).
                LinkContainer(stat.Value).
                SetColors(_statColors[stat.Key]).
                SetEaseMethod(_barsEaseMethod).
                SetFillTime(_barsEaseTime);
            }
        }
   
    }
}