using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units.Stats;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
namespace Arcatech.Stat
{
    /// <summary>
    /// new component to handle the current stats and their changes on any game entity
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour
    {
        [SerializeField] protected BaseStatsConfig startingStats;
        [SerializeField] protected float statsUpdateFrequency = 0.1f;

        private Dictionary<BaseStatType, StatValueContainer> _stats;

        private void Start()
        {
            _stats = startingStats.BuildBaseStats;
        }
        private void Update()
        {
            foreach (var stat in _stats)
            {
                stat.Value.UpdateInDelta(Time.deltaTime);
            }
        }
        public void ApplyStatsEffect(StatsEffect eff)
        {

        }
        public void ApplyStatMod(StatsMod mod)
        {

        }

        // void ApplyStatsEffectOLD(StatsEffect eff, IEquippable shield, out float current)
        //{
        //    current = 0;
        //    if (UnitDead) return;

        //    if (_stats.CanApplyEffect(eff, shield))
        //    {
        //        current = _stats.GetStatValues[eff.StatType].GetCurrent;
        //    }
        //    OnTimedStatsUpdate();
        //}
    }
}