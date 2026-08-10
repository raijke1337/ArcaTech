using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Arcatech.SaveSystem;
using UnityEditor.SceneManagement;

public class EntityIDResetWindow : EditorWindow
{
    [MenuItem("Arcatech/Tools/Reset All EntityIDs in Scene")]
    public static void ShowWindow()
    {
        GetWindow<EntityIDResetWindow>("Reset EntityIDs");
    }

    private void OnGUI()
    {
        GUILayout.Label("Управление EntityID на сцене", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Этот инструмент найдет все компоненты EntityID на текущей открытой сцене, " +
            "сбросит их uniqueId в пустую строку, после чего сработает ваш OnValidate() и сгенерирует новые уникальные GUID.", 
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Сбросить EntityID на текущей сцене", GUILayout.Height(35)))
        {
            ResetEntityIDsInCurrentScene();
        }
    }

    private static void ResetEntityIDsInCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        
        if (!currentScene.IsValid() || !currentScene.isLoaded)
        {
            EditorUtility.DisplayDialog("Ошибка", "Активная сцена не валидна или не загружена!", "OK");
            return;
        }

        // Находим все EntityID на сцене, включая неактивные объекты
        EntityID[] entityIDs = GameObject.FindObjectsOfType<EntityID>(true);
        int resetCount = 0;

        // Открываем Undo блок, чтобы можно было отменить действие (Ctrl+Z)
        Undo.RegisterCompleteObjectUndo(entityIDs, "Reset Entity IDs");

        foreach (var entityID in entityIDs)
        {

            // Используем SerializedObject для доступа к приватному полю [SerializeField]
            SerializedObject serializedObject = new SerializedObject(entityID);
            SerializedProperty uniqueIdProp = serializedObject.FindProperty("uniqueId");

            if (uniqueIdProp != null)
            {
                uniqueIdProp.stringValue = string.Empty;
                serializedObject.ApplyModifiedProperties();

                // Помечаем компонент как измененный, чтобы OnValidate сработал или изменения сохранились
                EditorUtility.SetDirty(entityID);
                resetCount++;
            }
        }

        // Помечаем сцену как измененную, чтобы Unity попросила сохранить проект при закрытии
        EditorSceneManager.MarkSceneDirty(currentScene);

        EditorUtility.DisplayDialog(
            "Готово!", 
            $"Успешно сброшено EntityID: {resetCount} шт.\nНовые GUID сгенерированы автоматически.", 
            "OK");
        
        Debug.Log($"[EntityIDReset] Сброшено и перегенерировано EntityID на сцене '{currentScene.name}': {resetCount}");
    }
}