using Arcatech.Items;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    /// <summary>
    ///  new class to VIEW the data
    /// </summary>
    [RequireComponent(typeof(EntityInventoryComponent))]
    public class EntityInventoryDrawerComponent : MonoBehaviour, IUnitInventoryView
    {
        [Self,SerializeField] EntityInventoryComponent inventoryComponent;
        [SerializeField] protected ItemEmpties itemEmpties;
        [SerializeField] protected DrawItemsStrategy defaultItemsDrawStrat;

        UnitInventoryModel inventoryModel;
        IDrawItemStrategy currentDrawStrategy;

        public event UnityAction ViewChangedInventory;

        public void RefreshView(UnitInventoryModel model)
        {
            inventoryModel = model;
            DrawItems(currentDrawStrategy);
        }

        private void Start()
        {
            currentDrawStrategy = defaultItemsDrawStrat;
            inventoryComponent?.SetModelView(this);
        }
        public void DrawItems(IDrawItemStrategy strategy)
        {
            if (strategy == null) return; // case for destructrible items
            currentDrawStrategy = strategy;
            foreach (var e in inventoryModel.Equipments.GetAllValues())
            {
                ItemPlaceType placeType = strategy.GetPlaces[e.Type];
                switch (placeType)
                {
                    case ItemPlaceType.Hidden:
                        e.ItemShown = false;
                        break;
                    default:
                        e.SetItemEmpty(itemEmpties.ItemPositions[strategy.GetPlaces[e.Type]]);
                        break;
                }
            }
        }

    }

}