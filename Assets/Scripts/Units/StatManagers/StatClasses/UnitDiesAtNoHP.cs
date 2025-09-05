using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "unitDiesIStrat",menuName = "Units/Stat change/Unit dies at 0 hp ")]
    public class UnitDiesAtNoHP : StatUpdateHandlerSerialized
    {
        [SerializeField] SerializedUnitAction actionOnDeath;

        public override void HandleUpdate(IDictionary<BaseStatType, StatValueContainer> stats, BaseGameEntityComponent baseEntity, ActiveGameUnitComponent activeEntity)
        {
            var c = stats[BaseStatType.Health];
            if (c != null)
            {
                if (c.GetCurrent <= 0)
                {
                    if (actionOnDeath != null)
                    {
                        var a = actionOnDeath.ProduceAction(activeEntity, activeEntity.transform);
                        activeEntity.ForceUnitAction(a);
                    }
                    baseEntity.KillEntity();
                }
            }
        }
    }
}
