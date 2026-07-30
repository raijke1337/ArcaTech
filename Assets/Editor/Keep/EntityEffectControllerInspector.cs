#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using Arcatech.Usables.Effects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityEffectController))]
public class EntityEffectControllerInspector : Editor
{
    // ── Reflection field handles (кэшируются один раз) ──────────────────────
    private static readonly FieldInfo _fActive = typeof(EntityEffectController)
        .GetField("_active", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo _fParticles = typeof(EntityEffectController)
        .GetField("_activeParticles", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo _fKilled = typeof(EntityEffectController)
        .GetField("_killed", BindingFlags.Instance | BindingFlags.NonPublic);

    // ActiveEffectInstance private fields
    private static readonly FieldInfo _fElapsed = typeof(ActiveEffectInstance)
        .GetField("_elapsed", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo _fInfinite = typeof(ActiveEffectInstance)
        .GetField("_infinite", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo _fTicksFired = typeof(ActiveEffectInstance)
        .GetField("_ticksFired", BindingFlags.Instance | BindingFlags.NonPublic);

    // ── Foldout state per EffectKey (строковый ключ → открыт?) ──────────────
    private readonly Dictionary<string, bool> _foldouts = new();

    // ── Стили (создаются лениво) ─────────────────────────────────────────────
    private GUIStyle _boxStyle;
    private GUIStyle _headerStyle;
    private GUIStyle _killedStyle;
    private GUIStyle _pausedStyle;
    private GUIStyle _tagStyle;

    // ── Цвета ────────────────────────────────────────────────────────────────
    private static readonly Color ColFinished  = new(1f,  0.35f, 0.35f, 1f);   // красный
    private static readonly Color ColInfinite  = new(0.4f, 0.8f, 1f,  1f);    // голубой
    private static readonly Color ColNormal    = new(0.7f, 1f,  0.7f, 1f);    // зелёный
    private static readonly Color ColParticle  = new(1f,  0.85f, 0.3f, 1f);   // жёлтый
    private static readonly Color ColHeader    = new(0.18f, 0.18f, 0.22f, 1f);

    // ── Автообновление в Play Mode ───────────────────────────────────────────
    public override bool RequiresConstantRepaint() => Application.isPlaying;

    // ────────────────────────────────────────────────────────────────────────
    public override void OnInspectorGUI()
    {
        InitStyles();

        var ctrl = (EntityEffectController)target;

        DrawStatusBar(ctrl);
        EditorGUILayout.Space(4);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Данные эффектов доступны только в Play Mode.", MessageType.Info);
            return;
        }

        var active = _fActive.GetValue(ctrl)
            as Dictionary<EffectKey, List<ActiveEffectInstance>>;
        var particles = _fParticles.GetValue(ctrl)
            as Dictionary<EffectKey, ParticleSystem>;

        if (active == null || active.Count == 0)
        {
            EditorGUILayout.HelpBox("Нет активных эффектов.", MessageType.None);
            return;
        }

        DrawSummary(active, particles);
        EditorGUILayout.Space(4);
        DrawEffectGroups(active, particles);
    }

    // ── Статус-бар (Paused / Killed) ─────────────────────────────────────────
    private void DrawStatusBar(EntityEffectController ctrl)
    {
        bool killed  = (bool)(_fKilled.GetValue(ctrl) ?? false);
        bool paused  = ctrl.Paused;

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("EntityEffectController", _headerStyle, GUILayout.ExpandWidth(true));

            if (killed)
                GUILayout.Label("  KILLED  ", _killedStyle);
            else if (paused)
                GUILayout.Label("  PAUSED  ", _pausedStyle);
            else
                GUILayout.Label("  ACTIVE  ", _tagStyle);
        }
    }

    // ── Сводка (счётчики) ────────────────────────────────────────────────────
    private void DrawSummary(
        Dictionary<EffectKey, List<ActiveEffectInstance>> active,
        Dictionary<EffectKey, ParticleSystem> particles)
    {
        int totalInstances = 0;
        foreach (var kv in active) totalInstances += kv.Value.Count;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(
                $"Групп эффектов: {active.Count}   |   Инстансов: {totalInstances}   |   Частиц: {particles.Count}",
                EditorStyles.miniLabel);
        }
    }

    // ── Группы эффектов ──────────────────────────────────────────────────────
    private void DrawEffectGroups(
        Dictionary<EffectKey, List<ActiveEffectInstance>> active,
        Dictionary<EffectKey, ParticleSystem> particles)
    {
        foreach (var kv in active)
        {
            EffectKey key       = kv.Key;
            var       instances = kv.Value;
            string    keyStr    = key.ToString();

            _foldouts.TryGetValue(keyStr, out bool open);

            using (new EditorGUILayout.VerticalScope(_boxStyle))
            {
                // ── Заголовок группы ─────────────────────────────────────
                using (new EditorGUILayout.HorizontalScope())
                {
                    open = EditorGUILayout.Foldout(open, GUIContent.none, true);

                    // Иконка частицы
                    particles.TryGetValue(key, out var ps);
                    if (ps != null)
                    {
                        var prevColor = GUI.color;
                        GUI.color = ColParticle;
                        GUILayout.Label("●", GUILayout.Width(14));
                        GUI.color = prevColor;
                    }
                    else
                    {
                        GUILayout.Label("○", GUILayout.Width(14));
                    }

                    EditorGUILayout.LabelField(
                        $"eff# {key.EffectId}",
                        EditorStyles.boldLabel,
                        GUILayout.ExpandWidth(true));

                    GUILayout.Label(
                        $"src# {TruncateId(key.SourceId)}",
                        EditorStyles.miniLabel,
                        GUILayout.Width(120));

                    DrawStackBadge(instances.Count);
                }

                _foldouts[keyStr] = open;

                // ── Содержимое группы ────────────────────────────────────
                if (open)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < instances.Count; i++)
                        DrawInstance(instances[i], i);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space(2);
        }
    }

    // ── Один инстанс эффекта ─────────────────────────────────────────────────
    private void DrawInstance(ActiveEffectInstance inst, int idx)
    {
        float elapsed    = (float)(_fElapsed.GetValue(inst)    ?? 0f);
        bool  infinite   = (bool)(_fInfinite.GetValue(inst)    ?? false);
        int   ticksFired = (int)(_fTicksFired.GetValue(inst)   ?? 0);

        // Цвет строки по состоянию
        Color rowColor = inst.IsFinished ? ColFinished
                       : infinite        ? ColInfinite
                                         : ColNormal;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            // ── Заголовок инстанса ────────────────────────────────────────
            using (new EditorGUILayout.HorizontalScope())
            {
                var prevColor = GUI.contentColor;
                GUI.contentColor = rowColor;
                EditorGUILayout.LabelField(
                    $"[{idx}]  {(inst.IsFinished ? "✗ FINISHED" : infinite ? "∞ INFINITE" : "▶ ACTIVE")}",
                    EditorStyles.boldLabel);
                GUI.contentColor = prevColor;

                GUILayout.FlexibleSpace();

                if (inst.Stacks > 1)
                    DrawStackBadge(inst.Stacks);
            }

            // ── Прогресс-бар + elapsed ────────────────────────────────────
            if (!infinite && !inst.IsFinished)
            {
                // PeriodicityRunner.TotalDuration недоступна напрямую — 
                // используем elapsed как индикатор «прошло времени»
                var rectBar = GUILayoutUtility.GetRect(0, 6, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rectBar, new Color(0.2f, 0.2f, 0.2f));

                // Нормализованный прогресс: clamp в [0,1] на случай долгих эффектов
                float norm = Mathf.Clamp01(elapsed / Mathf.Max(elapsed + 0.5f, 1f));
                var fill = new Rect(rectBar.x, rectBar.y, rectBar.width * norm, rectBar.height);
                EditorGUI.DrawRect(fill, ColNormal);
            }

            // ── Поля данных ───────────────────────────────────────────────
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField("Elapsed (s)", elapsed);
                EditorGUILayout.IntField("Ticks Fired",  ticksFired);
                EditorGUILayout.Toggle("Infinite",       infinite);
                EditorGUILayout.ObjectField(
                    "Source",
                    inst.Source as UnityEngine.Object,
                    typeof(UnityEngine.Object),
                    true);
                EditorGUILayout.ObjectField(
                    "Particle Prefab",
                    inst.GetDisplayEffect,
                    typeof(ParticleSystem),
                    false);
            }
        }
    }

    // ── Бейдж со стаками ─────────────────────────────────────────────────────
    private void DrawStackBadge(int count)
    {
        if (count <= 1) return;
        var prev = GUI.color;
        GUI.color = new Color(1f, 0.6f, 0.1f);
        GUILayout.Label($"×{count}", EditorStyles.boldLabel, GUILayout.Width(28));
        GUI.color = prev;
    }

    // ── Обрезка длинного GUID для компактного отображения ────────────────────
    private static string TruncateId(string id) =>
        id.Length > 12 ? id.Substring(0, 8) + "…" : id;

    // ── Ленивая инициализация стилей ─────────────────────────────────────────
    private void InitStyles()
    {
        if (_boxStyle != null) return;

        _boxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(6, 6, 4, 4)
        };

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 13,
            alignment = TextAnchor.MiddleLeft
        };

        _killedStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal    = { textColor = ColFinished },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _pausedStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal    = { textColor = new Color(1f, 0.8f, 0.2f) },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _tagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal    = { textColor = ColNormal },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
    }
}
#endif