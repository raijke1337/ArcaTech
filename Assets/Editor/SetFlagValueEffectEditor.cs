// Place in Editor folder
// File: SetFlagValueEffectEditor.cs

using Arcatech.Interactions;
using Arcatech.SaveSystem;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Показывает текущее состояние флага цели, который ставит SetFlagValueEffect.
/// Состояние флага живёт только в памяти (ObjectiveFlagSO), поэтому видно в Play Mode.
/// </summary>
[CustomEditor(typeof(SetFlagValueEffect))]
[CanEditMultipleObjects]
public class SetFlagValueEffectEditor : Editor
{
    private static readonly Color SetColor = new Color(0.37f, 0.9f, 0.66f);
    private static readonly Color UnsetColor = new Color(0.55f, 0.58f, 0.65f);

    private SerializedProperty flagProp;
    private SerializedProperty onStateProp;

    private void OnEnable()
    {
        flagProp = serializedObject.FindProperty("flag");
        onStateProp = serializedObject.FindProperty("onState");
    }

    // В Play Mode флаг может поменяться в любой момент - перерисовываем постоянно.
    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(flagProp, new GUIContent("Flag"));
        EditorGUILayout.PropertyField(onStateProp, new GUIContent("Set On State",
            "Флаг ставится, когда взаимодействие доходит до этого состояния"));

        serializedObject.ApplyModifiedProperties();

        if (targets.Length > 1) return;

        EditorGUILayout.Space(6);
        DrawFlagState(flagProp.objectReferenceValue as ObjectiveFlagSO);
    }

    private static void DrawFlagState(ObjectiveFlagSO flag)
    {
        if (flag == null)
        {
            EditorGUILayout.HelpBox("Флаг не назначен - эффект ничего не сделает.", MessageType.Error);
            return;
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Состояние флага хранится только в памяти и видно в Play Mode. " +
                "При загрузке уровня флаг сбрасывается и восстанавливается из чекпоинта.",
                MessageType.None);
            return;
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            // Текущее состояние
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect dot = GUILayoutUtility.GetRect(12, 12, GUILayout.Width(12), GUILayout.Height(18));
                dot.y += 3; dot.height = 12;
                EditorGUI.DrawRect(dot, flag.IsSet ? SetColor : UnsetColor);

                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = flag.IsSet ? SetColor : UnsetColor }
                };
                EditorGUILayout.LabelField(flag.IsSet ? "Установлен" : "Не установлен", style);
            }

            // Что записано в последнем сохранённом чекпоинте
            var save = SaveManager.TryGetInstance();
            if (save != null && save.TryGetCheckpoint(out var checkpoint))
            {
                bool inCheckpoint = checkpoint.setFlags != null && checkpoint.setFlags.Contains(flag.name);
                EditorGUILayout.LabelField("В последнем чекпоинте",
                    inCheckpoint ? "установлен" : "не установлен", EditorStyles.miniLabel);
            }

            // Отладка: поменять флаг вручную
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(flag.IsSet))
                {
                    if (GUILayout.Button("Установить")) flag.Set();
                }
                using (new EditorGUI.DisabledScope(!flag.IsSet))
                {
                    if (GUILayout.Button("Сбросить")) flag.Reset();
                }
            }
        }
    }
}
