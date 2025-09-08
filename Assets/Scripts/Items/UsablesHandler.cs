using Arcatech.Skills;
using Arcatech.Stat;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items
{
    [Serializable]
    public class UsablesHandler : IUnitActionsHandler, IUnitInventoryView
    {
        #region serialize
        [SerializeField, ReadOnlyText] string info;
        #endregion

        IDrawItemStrategy currentDrawItemStrategy;
        public event UnityAction<IDrawItemStrategy> DrawStrategyUpdateEvent = delegate { };
        public event UnityAction ViewChangedInventory;

        Dictionary<UnitActionType, IUsable> _usables;
        public List<IUsable> GetUsables => _usables.Values.ToList();

        public UsablesHandler ()
        {
            _usables = new();
            info = "Init";
        }

        public void RefreshView(UnitInventoryModel model)
        {
            Debug.Log("Refresh usables in handler");

            var newEquips = model.ListEquipped;
            List<IUsable> newList = new();

            foreach (var equipment in newEquips)
            {
                newList.AddRange(equipment.GetUsables);
            }
            foreach (var sk in newList)
            {
                if (!_usables.TryGetValue(sk.UseActionType, out IUsable usable) || usable != sk)
                {
                    // no key or different skill loaded
                    _usables[sk.UseActionType] = sk;
                }
            }
        }

        public void Update(float delta)
        {
            foreach (var u in _usables.Values)
            {
                u.DoUpdate(delta);
            }
        }

        public bool TryHandleAction(UnitActionType type, EntityStatsComponent stats, out BaseUnitAction action)
        {
            action = null;
            if (_usables.TryGetValue(type, out var usable))
            {
                if (usable == null)
                {
                    info = $"No usable assigned for {type}";
                    return false;
                }

                bool ok = usable.TryUseItem(stats, out action);

                info = $"Use {usable} result {ok} ";

                if (usable is IAffectsItemDisplay disp)
                {
                    if (disp.DrawStrategy != currentDrawItemStrategy)
                    {
                        currentDrawItemStrategy = disp.DrawStrategy;
                        DrawStrategyUpdateEvent?.Invoke(disp.DrawStrategy);
                    }
                }
                return ok;
            }
            return false;
        }


    }


}