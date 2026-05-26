// Place in Editor folder
// File: ItemDatabaseWindow.cs

using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Usables;
using UnityEditor;
using UnityEngine;

public class ItemDatabaseWindow : EditorWindow
{
    // Data
    private List<ItemSO> allItems = new List<ItemSO>();
    private List<ItemSO> filteredItems = new List<ItemSO>();
    
    // UI State
    private Vector2 scrollPosition;
    private string searchFilter = "";
    private ItemTypeFilter typeFilter = ItemTypeFilter.All;
    private SortMode sortMode = SortMode.Name;
    private int cardSize = 150; // Card width in pixels
    
    // Selection
    private ItemSO selectedItem;
    
    // Styles (cached)
    private GUIStyle cardStyle;
    private GUIStyle cardSelectedStyle;
    private GUIStyle titleStyle;
    private GUIStyle typeTagStyle;
    private GUIStyle searchStyle;
    private bool stylesInitialized = false;
    
    // Constants
    private const int MIN_CARD_SIZE = 100;
    private const int MAX_CARD_SIZE = 250;
    private const int TOOLBAR_HEIGHT = 25;
    private const int CARD_PADDING = 5;
    
    // Enums
    private enum ItemTypeFilter
    {
        All,
        BasicItems,
        Equipment,
        Usables
    }
    
    private enum SortMode
    {
        Name,
        Type,
        MaxStack
    }
    
    [MenuItem("Window/Game/Item Database")]
    public static void ShowWindow()
    {
        ItemDatabaseWindow window = GetWindow<ItemDatabaseWindow>();
        window.titleContent = new GUIContent("Item Database", EditorGUIUtility.IconContent("d_PreMatCube").image);
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnEnable()
    {
        RefreshItemList();
        EditorApplication.projectChanged += OnProjectChanged;
    }
    
    private void OnDisable()
    {
        EditorApplication.projectChanged -= OnProjectChanged;
    }
    
    private void OnProjectChanged()
    {
        RefreshItemList();
        Repaint();
    }
    
    private void InitStyles()
    {
        if (stylesInitialized) return;
        
        cardStyle = new GUIStyle("box")
        {
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(4, 4, 4, 4),
            alignment = TextAnchor.UpperCenter
        };
        
        cardSelectedStyle = new GUIStyle(cardStyle);
        cardSelectedStyle.normal.background = MakeColorTexture(new Color(0.3f, 0.5f, 0.8f, 0.5f));
        
        titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
            wordWrap = true,
            clipping = TextClipping.Clip
        };
        
        typeTagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 9,
            normal = { textColor = Color.white }
        };
        
        searchStyle = new GUIStyle(EditorStyles.toolbarSearchField);
        
        stylesInitialized = true;
    }
    
    private Texture2D MakeColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
    
    private void RefreshItemList()
    {
        allItems.Clear();
        
        // Find all ItemSO assets in the project
        string[] guids = AssetDatabase.FindAssets("t:ItemSO");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
            if (item != null)
            {
                allItems.Add(item);
            }
        }
        
        ApplyFiltersAndSort();
    }
    
    private void ApplyFiltersAndSort()
    {
        // Filter by type
        IEnumerable<ItemSO> items = allItems;
        
        switch (typeFilter)
        {
            case ItemTypeFilter.BasicItems:
                items = items.Where(i => i.GetType() == typeof(ItemSO));
                break;
            case ItemTypeFilter.Equipment:
                items = items.Where(i => i is EquipSO && !(i is UsablesSO));
                break;
            case ItemTypeFilter.Usables:
                items = items.Where(i => i is UsablesSO);
                break;
        }
        
        // Filter by search
        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            string search = searchFilter.ToLower();
            items = items.Where(i => 
            {
                if (i.name.ToLower().Contains(search)) return true;
                if (i.Description != null && i.Description.Title != null && 
                    i.Description.Title.ToLower().Contains(search)) return true;
                return false;
            });
        }
        
        // Sort
        switch (sortMode)
        {
            case SortMode.Name:
                items = items.OrderBy(i => i.Description?.Title ?? i.name);
                break;
            case SortMode.Type:
                items = items.OrderBy(i => i.GetType().Name).ThenBy(i => i.name);
                break;
            case SortMode.MaxStack:
                items = items.OrderByDescending(i => i.MaxStack).ThenBy(i => i.name);
                break;
        }
        
        filteredItems = items.ToList();
    }
    
    private void OnGUI()
    {
        InitStyles();
        
        DrawToolbar();
        DrawSecondaryToolbar();
        DrawItemGrid();
        DrawStatusBar();
        
        // Handle keyboard shortcuts
        HandleKeyboardInput();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // Refresh button
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            RefreshItemList();
        }
        
        // Search field
        EditorGUI.BeginChangeCheck();
        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
        if (EditorGUI.EndChangeCheck())
        {
            ApplyFiltersAndSort();
        }
        
        // Clear search button
        if (GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSearchCancelButton")))
        {
            searchFilter = "";
            GUI.FocusControl(null);
            ApplyFiltersAndSort();
        }
        
        GUILayout.FlexibleSpace();
        
        // Create new item dropdown
        if (EditorGUILayout.DropdownButton(new GUIContent("Create New", EditorGUIUtility.IconContent("d_CreateAddNew").image), 
            FocusType.Passive, EditorStyles.toolbarDropDown, GUILayout.Width(100)))
        {
            ShowCreateMenu();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSecondaryToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // Type filter
        EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
        EditorGUI.BeginChangeCheck();
        typeFilter = (ItemTypeFilter)EditorGUILayout.EnumPopup(typeFilter, EditorStyles.toolbarPopup, GUILayout.Width(100));
        if (EditorGUI.EndChangeCheck())
        {
            ApplyFiltersAndSort();
        }
        
        GUILayout.Space(20);
        
        // Sort mode
        EditorGUILayout.LabelField("Sort:", GUILayout.Width(35));
        EditorGUI.BeginChangeCheck();
        sortMode = (SortMode)EditorGUILayout.EnumPopup(sortMode, EditorStyles.toolbarPopup, GUILayout.Width(80));
        if (EditorGUI.EndChangeCheck())
        {
            ApplyFiltersAndSort();
        }
        
        GUILayout.FlexibleSpace();
        
        // Card size slider
        EditorGUILayout.LabelField("Size:", GUILayout.Width(35));
        cardSize = (int)GUILayout.HorizontalSlider(cardSize, MIN_CARD_SIZE, MAX_CARD_SIZE, GUILayout.Width(100));
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawItemGrid()
    {
        if (filteredItems.Count == 0)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (allItems.Count == 0)
            {
                EditorGUILayout.LabelField("No ItemSO assets found in project.", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("No items match the current filters.", EditorStyles.centeredGreyMiniLabel);
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            return;
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Calculate grid layout
        float viewWidth = position.width - 20; // Account for scrollbar
        int columns = Mathf.Max(1, Mathf.FloorToInt(viewWidth / (cardSize + CARD_PADDING * 2)));
        int rows = Mathf.CeilToInt((float)filteredItems.Count / columns);
        
        int itemIndex = 0;
        
        for (int row = 0; row < rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            for (int col = 0; col < columns && itemIndex < filteredItems.Count; col++)
            {
                DrawItemCard(filteredItems[itemIndex], cardSize);
                itemIndex++;
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void SelectWithoutPing(Object obj)
    {
        // Store the current Project window folder selection
        Object[] previousSelection = Selection.objects;
        string previousFolder = GetCurrentProjectFolder();
    
        // Select the new object (this will show it in Inspector)
        Selection.activeObject = obj;
    
        // Note: Unfortunately Unity doesn't provide a clean way to prevent
        // the Project window from scrolling. The Selection.activeObject
        // by itself doesn't ping, only EditorGUIUtility.PingObject does.
        // So this should work as expected - Inspector shows the item,
        // Project window stays where it was.
    }

    private string GetCurrentProjectFolder()
    {
        // Try to get the currently selected folder in Project window
        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                if (System.IO.Directory.Exists(path))
                {
                    return path;
                }
                return System.IO.Path.GetDirectoryName(path);
            }
        }
        return "Assets";
    }

    private void DrawItemCard(ItemSO item, int size)
    {    bool isSelected = selectedItem == item;
        GUIStyle style = isSelected ? cardSelectedStyle : cardStyle;
    
        // Calculate card height based on size
        int cardHeight = size + 40; // Extra space for title and type tag
    
        Rect cardRect = GUILayoutUtility.GetRect(size, cardHeight, style, GUILayout.Width(size), GUILayout.Height(cardHeight));
    
        // Handle input
        Event e = Event.current;
        if (cardRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0) // Left click
                {
                    // Always update selection in the window
                    selectedItem = item;
                
                    if (e.clickCount == 2) // Double click - ping asset in Project window
                    {
                        EditorGUIUtility.PingObject(item);
                    }
                    // Single click - just select, don't ping
                    // This opens in Inspector without changing Project window focus
                
                    // Use Selection.activeObject to show in Inspector
                    // But we do it in a way that doesn't ping
                    SelectWithoutPing(item);
                
                    e.Use();
                    Repaint();
                }
                else if (e.button == 1) // Right click
                {
                    ShowItemContextMenu(item);
                    e.Use();
                }
            }
        }

        // Draw card background
        GUI.Box(cardRect, GUIContent.none, style);

        // Draw type color indicator
        Color typeColor = GetTypeColor(item);
        Rect colorBarRect = new Rect(cardRect.x, cardRect.y, cardRect.width, 4);
        EditorGUI.DrawRect(colorBarRect, typeColor);

        // Content area
        Rect contentRect = new Rect(cardRect.x + 6, cardRect.y + 8, cardRect.width - 12, cardRect.height - 12);

        float yOffset = contentRect.y;

        // Draw icon/picture
        int imageSize = size - 40;
        Rect imageRect = new Rect(contentRect.x + (contentRect.width - imageSize) / 2, yOffset, imageSize, imageSize);

        Sprite picture = item.Description?.Picture;
        if (picture != null)
        {
            Texture2D preview = AssetPreview.GetAssetPreview(picture);
            if (preview != null)
            {
                GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
            }
            else
            {
                // Draw sprite directly if preview not ready
                DrawSprite(imageRect, picture);
            }
        }
        else
        {
            // Draw placeholder
            EditorGUI.DrawRect(imageRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));
            GUI.Label(imageRect, EditorGUIUtility.IconContent("d_PreMatCube"),
                new GUIStyle { alignment = TextAnchor.MiddleCenter });
        }

        yOffset += imageSize + 4;

        // Draw title
        string title = item.Description?.Title;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = item.name;
        }

        // Truncate title if too long
        if (title.Length > 20)
        {
            title = title.Substring(0, 17) + "...";
        }

        Rect titleRect = new Rect(contentRect.x, yOffset, contentRect.width, 16);
        GUI.Label(titleRect, title, titleStyle);
        yOffset += 16;

        // Draw type tag
        string typeTag = GetTypeTag(item);
        Rect tagRect = new Rect(contentRect.x, yOffset, contentRect.width, 14);

        // Tag background
        Rect tagBgRect = new Rect(
            contentRect.x + (contentRect.width - 60) / 2,
            yOffset,
            60, 14
        );
        EditorGUI.DrawRect(tagBgRect, new Color(typeColor.r, typeColor.g, typeColor.b, 0.8f));
        GUI.Label(tagRect, typeTag, typeTagStyle);

        // Draw stack count badge if > 1
        if (item.MaxStack > 1)
        {
            Rect badgeRect = new Rect(cardRect.xMax - 24, cardRect.y + 6, 20, 16);
            EditorGUI.DrawRect(badgeRect, new Color(0.2f, 0.2f, 0.2f, 0.8f));
            GUI.Label(badgeRect, item.MaxStack.ToString(), new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }
    }

    private void DrawSprite(Rect rect, Sprite sprite)
    {
        if (sprite == null || sprite.texture == null) return;
        
        Texture2D tex = sprite.texture;
        Rect texCoords = new Rect(
            sprite.rect.x / tex.width,
            sprite.rect.y / tex.height,
            sprite.rect.width / tex.width,
            sprite.rect.height / tex.height
        );
        
        GUI.DrawTextureWithTexCoords(rect, tex, texCoords);
    }
    
    private Color GetTypeColor(ItemSO item)
    {
        if (item is UsablesSO) return new Color(0.4f, 0.8f, 0.4f);
        if (item is EquipSO) return new Color(0.4f, 0.6f, 0.9f);
        return new Color(0.9f, 0.7f, 0.3f);
    }
    
    private string GetTypeTag(ItemSO item)
    {
        if (item is UsablesSO) return "Usable";
        if (item is EquipSO) return "Equip";
        return "Item";
    }
    
    private void DrawStatusBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // Item count
        string countText = $"{filteredItems.Count} items";
        if (filteredItems.Count != allItems.Count)
        {
            countText += $" (of {allItems.Count} total)";
        }
        EditorGUILayout.LabelField(countText, EditorStyles.miniLabel);
        
        GUILayout.FlexibleSpace();
        
        // Selected item info
        if (selectedItem != null)
        {
            EditorGUILayout.LabelField($"Selected: {selectedItem.name}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void ShowCreateMenu()
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Basic Item"), false, () => CreateNewItem<ItemSO>("New Item"));
        menu.AddItem(new GUIContent("Equipment"), false, () => CreateNewItem<EquipSO>("New Equipment"));
        menu.AddItem(new GUIContent("Usable Item"), false, () => CreateNewItem<UsablesSO>("New Usable"));
        
        menu.ShowAsContext();
    }
    
    private void CreateNewItem<T>(string defaultName) where T : ItemSO
    {
        // Get save path
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Item",
            defaultName,
            "asset",
            "Choose a location for the new item"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        // Create the asset
        T newItem = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();
        
        // Refresh and select
        RefreshItemList();
        selectedItem = newItem;
        Selection.activeObject = newItem;
        EditorGUIUtility.PingObject(newItem);
    }
    
    private void ShowItemContextMenu(ItemSO item)
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Select in Project"), false, () =>
        {
            Selection.activeObject = item;
            EditorGUIUtility.PingObject(item);
        });
        
        menu.AddItem(new GUIContent("Open Inspector"), false, () =>
        {
            Selection.activeObject = item;
        });
        
        menu.AddSeparator("");
        
        menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateItem(item));
        
        menu.AddSeparator("");
        
        menu.AddItem(new GUIContent("Delete"), false, () =>
        {
            if (EditorUtility.DisplayDialog("Delete Item", 
                $"Are you sure you want to delete '{item.name}'?", 
                "Delete", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(item);
                AssetDatabase.DeleteAsset(path);
                RefreshItemList();
                
                if (selectedItem == item)
                {
                    selectedItem = null;
                }
            }
        });
        
        menu.ShowAsContext();
    }
    
    private void DuplicateItem(ItemSO item)
    {
        string originalPath = AssetDatabase.GetAssetPath(item);
        string directory = System.IO.Path.GetDirectoryName(originalPath);
        string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{item.name}_Copy.asset");
        
        AssetDatabase.CopyAsset(originalPath, newPath);
        AssetDatabase.SaveAssets();
        
        RefreshItemList();
        
        ItemSO newItem = AssetDatabase.LoadAssetAtPath<ItemSO>(newPath);
        selectedItem = newItem;
        Selection.activeObject = newItem;
        EditorGUIUtility.PingObject(newItem);
    }
    
    private void HandleKeyboardInput()
    {
        Event e = Event.current;
        
        if (e.type != EventType.KeyDown) return;
        
        switch (e.keyCode)
        {
            case KeyCode.F5:
                RefreshItemList();
                e.Use();
                break;
                
            case KeyCode.Delete:
                if (selectedItem != null)
                {
                    if (EditorUtility.DisplayDialog("Delete Item", 
                        $"Are you sure you want to delete '{selectedItem.name}'?", 
                        "Delete", "Cancel"))
                    {
                        string path = AssetDatabase.GetAssetPath(selectedItem);
                        AssetDatabase.DeleteAsset(path);
                        selectedItem = null;
                        RefreshItemList();
                    }
                    e.Use();
                }
                break;
                
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                if (selectedItem != null)
                {
                    Selection.activeObject = selectedItem;
                    EditorGUIUtility.PingObject(selectedItem);
                    e.Use();
                }
                break;
                
            case KeyCode.Escape:
                selectedItem = null;
                GUI.FocusControl(null);
                e.Use();
                Repaint();
                break;
        }
    }
}