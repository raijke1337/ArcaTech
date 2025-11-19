using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Stats
{
    public class KillKillablesSO : StatChangeResponseStrat
    {
        public override IOnStatChange Deserialize(EntityStatsComponent comp)
        {
            return new KillKillables(comp);
        }
    }

    public class KillKillables : IOnStatChange
    {
        List <IKillableComponent> killables;
        public KillKillables(EntityStatsComponent comp)
        {
            killables = new (comp.gameObject.GetComponentsInChildren<IKillableComponent>());
        }
        public void OnStatChanged(ResourceStatType type, float current, float max, float delta, object contributionSource)
        {
            if (type == ResourceStatType.Health && current <= 0f)
            {
                foreach (var f in killables)
                {
                    f.Killed = true;
                }
            }
        }
    }
    
}
