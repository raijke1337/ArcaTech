using System;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Items
{
    /// <summary>Состояние панели коллекционного предмета в Мастерской.</summary>
    public enum GalleryEntryState
    {
        /// <summary>Чертёж не найден: серая панель, знак вопроса.</summary>
        Hidden,
        /// <summary>Чертёж найден: серая иконка, название, кнопка «Разблокировать».</summary>
        Found,
        /// <summary>Разблокировано: цветная иконка, кнопка «Исследовать» / «Экипировать».</summary>
        Unlocked
    }

    /// <summary>Запись о ловушке или костюме в Мастерской. Прототип: заполняется в инспекторе контроллера.</summary>
    [Serializable]
    public class GalleryEntry
    {
        [Tooltip("Чертёж (ItemSO). Для ловушек - категория GalleryBlueprint, для костюмов - CostumeBlueprint. " +
                 "У костюма поле можно оставить пустым: костюм считается открытым с начала игры.")]
        public ItemSO blueprint;

        [Tooltip("Цена разблокировки в деталях")]
        [Min(0)] public int unlockCost = 100;

        [Tooltip("Название и картинка панели. Пусто - берутся у костюма, затем у чертежа.")]
        public Description description;

        [Tooltip("Только для костюмов: что надевается кнопкой «Экипировать»")]
        public EquipSO costume;

        public string BlueprintId => blueprint != null ? blueprint.ID : string.Empty;

        public Description ResolveDescription()
        {
            if (description != null) return description;
            if (costume != null && costume.Description != null) return costume.Description;
            return blueprint != null ? blueprint.Description : null;
        }

        public string ResolveTitle()
        {
            var d = ResolveDescription();
            if (d != null && !string.IsNullOrEmpty(d.Title)) return d.Title;
            if (costume != null) return costume.name;
            return blueprint != null ? blueprint.name : "?";
        }
    }
}
