using System;
using System.Collections.Generic;
using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEngine;
namespace Arcatech.UI
{

    public class BarsContainersManager : MonoBehaviour, IStatUpdatesViewer
    {
        [SerializeField] StatBarContainerUIScript _barPrefab;
        [Space,SerializeField] SerializedDictionary<ResourceStatType,ColorSet> _statColors;
        [SerializeField] Ease _barsEaseMethod;
        [SerializeField] float _barsEaseTime = 0.3f;
        [SerializeField, Range(0, 1), Tooltip("Delta change for visual effects")] float _barFlashTreschold = 0.2f;

        Dictionary<ResourceStatType, StatBarContainerUIScript> _barsDict;

        bool init = false;

        private ResourceStatType[] _typesCached;

        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType,
            BaseGameEntityComponent source)
        {
            if (!init) InitBars();
            var bar = _barsDict[stat];
            if (!bar.gameObject.activeSelf) bar.gameObject.SetActive(true);
            switch (changeType)
            {
                case EntityStatsComponent.ExpendType.None:
                    break;
                case EntityStatsComponent.ExpendType.UsableCost:
                    bar.UpdateValue(statCurrent, statMax, statDelta);
                    break;
                case EntityStatsComponent.ExpendType.ActionResult:
                    bar.UpdateValue(statCurrent, statMax, statDelta);
                    break;
                case EntityStatsComponent.ExpendType.Equipment:
                    bar.UpdateValue(statCurrent, statMax, statDelta);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null);
            }
        }

        public void SetShieldValue(ResourceStatType shieldStat, float currentValue)
        {
            _barsDict[shieldStat].DrawShield(currentValue);
        }

        private void InitBars()
        {
            if (!init)
            {
                _barsDict = new Dictionary<ResourceStatType, StatBarContainerUIScript>();
                foreach (var enumKey in (ResourceStatType[])Enum.GetValues(typeof(ResourceStatType)))
                {
                    _barsDict[enumKey] = Instantiate(_barPrefab, transform);
                    SetupBar(enumKey);
                    _barsDict[enumKey].gameObject.SetActive(false);
                }
            }
            init = true;
        }        
        void SetupBar(ResourceStatType stat)
        {
            _barsDict[stat].SetColors(_statColors[stat]).
                SetEaseMethod(_barsEaseMethod).SetFillTime(_barsEaseTime).SetBrightGlowAT(_barFlashTreschold);
        }
        public StatBarContainerUIScript GetResourceBar(ResourceStatType stat)
        {
            if (!init)
            {
                InitBars();
            }

            return _barsDict[stat];
        }
        public bool TryGetResourceBar(
            ResourceStatType stat,
            out StatBarContainerUIScript bar)
        {
            if (!init)
            {
                InitBars();
            }

            return _barsDict.TryGetValue(stat, out bar);
        }
    }
}