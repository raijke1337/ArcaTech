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
        Dictionary <BaseStatType, BarContainerUIScript> _barsDict;
        [SerializeField] BarContainerUIScript _barPrefab;
        [Space,SerializeField] SerializedDictionary<BaseStatType,ColorSet> _statColors;
        [SerializeField] Ease _barsEaseMethod;
        [SerializeField] float _barsEaseTime = 0.3f;

        public void RemoveBar(BaseStatType type)
        {
            Destroy(_barsDict[type].gameObject);
            _barsDict.Remove(type);
        }
        public void ClearAllBars()
        {
            if (_barsDict == null) return;

            foreach (var bar in _barsDict.Values)
            {
                Destroy(bar.gameObject);
            }
            _barsDict.Clear();
        }

        private void AddBar (BaseStatType barValue)
        {
            _barsDict[barValue] = Instantiate(_barPrefab, this.transform).
                SetColors(_statColors[barValue]).
                SetEaseMethod(_barsEaseMethod).
                SetFillTime(_barsEaseTime);
        }

        

        public void UpdateBarValue(BaseStatType barValue, StatValueContainer container)
            
        {
            if (container == null) return;
            if (_barsDict == null)
            {
                _barsDict = new Dictionary<BaseStatType, BarContainerUIScript>();
            }
            if (!_barsDict.TryGetValue(barValue, out _))
            {
                AddBar (barValue);
            }
            _barsDict[barValue].UpdateValue(container);
        }
    }
}