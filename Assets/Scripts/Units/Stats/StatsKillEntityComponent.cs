using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{
    [RequireComponent(typeof(BaseGameEntityComponent),typeof(EntityStatsComponent))]
    public class StatsKillEntityComponent : ValidatedMonoBehaviour, IStatUpdatesViewer, IKillerComponent
    {
        [Header("Will kill all killable components at 0 hp")] 
        [SerializeField, Self] private BaseGameEntityComponent entity;
        [SerializeField, Self] private EntityStatsComponent stats;
        
        private IKillableComponent[] comps;

        
        
        private void Awake()
        {
            comps = GetComponentsInChildren<IKillableComponent>();
            stats.RegisterStatsViewer(this);
        }

        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType,
            BaseGameEntityComponent source)
        {
            if (stat == ResourceStatType.Health && statCurrent <= 0)
            {
                foreach (IKillableComponent comp in comps)
                {
                    comp.SetKilled(this,true);
                }
            }
        }

        public void SetShieldValue(ResourceStatType shieldStat, float currentValue)
        { }

        public string KilledBy => "StatsKillEntity Comp: 0hp";
    }
}