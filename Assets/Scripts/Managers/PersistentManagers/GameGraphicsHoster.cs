using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable,Obsolete]
public class GameGraphicsHoster : MonoBehaviour
{
    #region SingletonLogic

    public static GameGraphicsHoster Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    #endregion

    [SerializeField] private SerializedDictionary<CursorType, Sprite> _cursors;
    [SerializeField] private SerializedDictionary<FontType, TMP_FontAsset> _fonts;
    [SerializeField] private List<Sprite>  _match2Pictures;

#if UNITY_EDITOR
    private void OnEnable()
    {
        Assert.IsNotNull(_cursors);
        Assert.IsTrue(_cursors.Count > 0);
        Assert.IsNotNull(_fonts);
        Assert.IsTrue(_fonts.Count > 0);        
        Assert.IsNotNull(_match2Pictures);
        Assert.IsTrue(_fonts.Count > 0);
    }
#endif
    private void Start()
    {
        SetCursor(CursorType.Menu);
        Cursor.visible = true;
    }
    public void SetCursor(CursorType type)
    {
       // Cursor.SetCursor(_cursors[type].texture, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
    }
    public TMP_FontAsset GetFont(FontType t) => _fonts[t];
    public List<Sprite> GetMatch2Sprites => _match2Pictures;



}

