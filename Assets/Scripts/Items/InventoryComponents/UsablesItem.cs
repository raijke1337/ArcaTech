using System.Collections.Generic;
using Arcatech.Items;


namespace Arcatech.Usables
{
    public class UsablesItem : Equipment, IUsablesSource
    {
        Dictionary<UnitActionType,IUsable> usables = new Dictionary<UnitActionType, IUsable>();
        
        public IDictionary<UnitActionType, IUsable> GetUsables => usables;
        public UsablesItem(UsablesSO cfg, BaseGameEntityComponent ow) : base(cfg, ow)
        {
            foreach (var st in cfg.usedActions)
            {
                usables.Add(st.Key, st.Value.Deserialize(ow,DisplayItem));
            }
        }

        /// <summary>Предмет надет: применения подключаются к триггерам и событиям.</summary>
        public override void OnEquip()
        {
            base.OnEquip();
            foreach (var usable in usables.Values) (usable as IAttachable)?.Attach();
        }

        /// <summary>
        /// Предмет снят или выброшен: применения отключаются и освобождают ресурсы (пул снарядов,
        /// приёмники в общей зоне ближнего боя). Чистить нужно именно здесь, а не при обновлении вида
        /// инвентаря - оно затрагивает и предметы, которые остаются экипированными.
        /// </summary>
        public override void OnRemove()
        {
            foreach (var usable in usables.Values) (usable as IAttachable)?.Detach();
            base.OnRemove();
        }

    }
}