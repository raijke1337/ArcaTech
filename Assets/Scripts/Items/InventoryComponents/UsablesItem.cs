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

        public void OnUnequip()
        {
            foreach (var st in usables.Values)
            {
                st.CleanUp();
            }
        }
    }
}