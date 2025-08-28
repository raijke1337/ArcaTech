using Arcatech.Skills;
using Arcatech.Stat;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items
{
    [Serializable]
    public class UsablesHandler : IUnitActionsHandler
    {
        #region serialize
        [SerializeField, ReadOnlyText] string info;
        #endregion


        public event UnityAction<IDrawItemStrategy> DrawStrategyUpdateEvent = delegate { };

        ObservableDictionary<EquipmentType, Equipment> _dict;
        Dictionary<UnitActionType, IUsable> _usables;
        public Dictionary<UnitActionType, IUsable> GetUsables => _usables;

        public UsablesHandler(ObservableDictionary<EquipmentType,Equipment> equipments)
        {
            _usables = new();
            _dict = equipments;
            Refresh();

            _dict.AnyValueChanged += _dict_AnyValueChanged;
            info = "Init";
        }

        private void _dict_AnyValueChanged(IEnumerable<Equipment> obj)
        {
            Refresh();
        }


        void Refresh()
        {
            foreach (var eq in _dict.GetAllValues())
            {
                var u = eq.GetUsables;
                foreach (var uu in u)
                {
                    _usables[uu.UseActionType] = uu;
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
                bool ok = usable.TryUseItem(stats, out action);

                info = $"Use {usable} result {ok} ";

                if (usable is IAffectsItemDisplay disp)
                {
                    DrawStrategyUpdateEvent?.Invoke(disp.DrawStrategy);
                }
                return ok;
            }
            return false;
        }
    }


}