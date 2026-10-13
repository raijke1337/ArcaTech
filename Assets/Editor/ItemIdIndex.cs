// Place in Editor folder
// File: ItemIdIndex.cs

using System.Collections.Generic;
using Arcatech.Items;
using UnityEditor;

/// <summary>
/// Индекс ID всех ItemSO проекта для редакторских инструментов.
/// Сохранения ссылаются на предметы по ID, поэтому два ассета с одним ID
/// (копия через Ctrl+D или Duplicate) приводят к путанице в инвентаре.
/// Пересчитывается лениво после любого изменения проекта.
/// </summary>
[InitializeOnLoad]
public static class ItemIdIndex
{
    private static Dictionary<string, List<ItemSO>> _byId;

    static ItemIdIndex()
    {
        EditorApplication.projectChanged += Invalidate;
    }

    public static void Invalidate() => _byId = null;

    private static void Build()
    {
        _byId = new Dictionary<string, List<ItemSO>>();
        foreach (var guid in AssetDatabase.FindAssets("t:ItemSO"))
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || string.IsNullOrEmpty(item.ID)) continue;

            if (!_byId.TryGetValue(item.ID, out var list))
            {
                list = new List<ItemSO>();
                _byId[item.ID] = list;
            }
            if (!list.Contains(item)) list.Add(item);
        }
    }

    /// <summary>Другие ассеты с тем же ID, что у item. Пусто - ID уникален.</summary>
    public static List<ItemSO> GetDuplicates(ItemSO item)
    {
        var result = new List<ItemSO>();
        if (item == null || string.IsNullOrEmpty(item.ID)) return result;
        if (_byId == null) Build();

        if (_byId.TryGetValue(item.ID, out var list))
            foreach (var other in list)
                if (other != item) result.Add(other);
        return result;
    }

    public static bool HasDuplicates(ItemSO item) => GetDuplicates(item).Count > 0;

    public static int CountItemsWithDuplicateIds()
    {
        if (_byId == null) Build();
        int count = 0;
        foreach (var list in _byId.Values)
            if (list.Count > 1) count += list.Count;
        return count;
    }

    /// <summary>Выдаёт ассету новый ID. Старые сохранения, ссылающиеся на прежний ID, его больше не найдут.</summary>
    public static void RegenerateId(ItemSO item)
    {
        if (item == null) return;
        Undo.RecordObject(item, "Regenerate Item ID");
        item.ID = System.Guid.NewGuid().ToString();
        EditorUtility.SetDirty(item);
        AssetDatabase.SaveAssetIfDirty(item);
        Invalidate();
    }

    /// <summary>Подпись категории для карточек и инспектора.</summary>
    public static string CategoryLabel(ItemSO item)
    {
        if (item is Arcatech.Usables.UsablesSO) return "Usable";
        if (item is EquipSO) return "Equip";
        return item.Category switch
        {
            ItemCategory.Part => "Part",
            ItemCategory.CostumeBlueprint => "Costume BP",
            ItemCategory.GalleryBlueprint => "Gallery BP",
            _ => "Level only"
        };
    }
}
