#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using Arcatech.Actions;
using Arcatech.Texts;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Arcatech.Usables.UnityEditor
{
    public class EnemyAbilityWizard : EditorWindow
    {
        private enum WizardStep
        {
            Basic = 0,
            Description,
            Cost,
            Cooldown,
            Animation,
            UsableData,
            Transition,
            Build
        }

        private enum CooldownKind
        {
            Basic,
            Reload,
            Queue
        }

        [MenuItem("Arcatech/Tools/Enemy Ability Wizard")]
        public static void OpenWindow()
        {
            var w = GetWindow<EnemyAbilityWizard>("Enemy Ability Wizard");
            w.minSize = new Vector2(520, 800);
            w.Show();
        }

        private Vector2 _scroll;
        private WizardStep _step = WizardStep.Basic;

        [SerializeField] private string _baseName = "enemy_ability";
        [SerializeField] private string _folder = "Assets/Content/Abilities";
        [SerializeField] private bool _overwriteExisting = false;
        [SerializeField] private bool _autoSelectAfterCreate = true;

        private Description _descriptionAsset;
        private AppliedStatsDeltaEffect _costAsset;
        private SerializedUnitState _stateAsset;
        private SerializedGenericCooldownStrategy _cooldownAsset;
        private SerializedUsableStrategy _usableStrategyPreview;
        private SerializedUsableStrategy _usableStrategyAsset;
        private SerializedStateTransition _transitionAsset;
        private CooldownKind _cooldownKind = CooldownKind.Basic;
        private bool _createCost = true;

        private readonly Dictionary<Object, Editor> _previewEditors = new();

        private void OnDisable()
        {
            DisposeEditors();
        }

        private void DisposeEditors()
        {
            foreach (var ed in _previewEditors.Values)
                if (ed != null) DestroyImmediate(ed);

            _previewEditors.Clear();
        }

        /// <summary>
        /// After a successful build we must drop all references to created assets.
        /// Otherwise reusing the same window will try to save old assets under new paths.
        /// </summary>
        private void ResetAllPreviews()
        {
            _descriptionAsset = null;
            _costAsset = null;
            _stateAsset = null;
            _cooldownAsset = null;
            _transitionAsset = null;
            _usableStrategyPreview = null;
            _usableStrategyAsset = null;

            DisposeEditors();
            _step = WizardStep.Basic;
        }

        private void OnGUI()
        {
            using var scroll = new EditorGUILayout.ScrollViewScope(_scroll);
            _scroll = scroll.scrollPosition;

            DrawHeader();
            DrawStepTabs();
            EditorGUILayout.Space(12);

            switch (_step)
            {
                case WizardStep.Basic: DrawBasicStep(); break;
                case WizardStep.Description: DrawDescriptionStep(); break;
                case WizardStep.Cost: DrawCostStep(); break;
                case WizardStep.Cooldown: DrawCooldownStep(); break;
                case WizardStep.Animation: DrawAnimationStep(); break;
                case WizardStep.UsableData: DrawUsableDataStep(); break;
                case WizardStep.Transition: DrawTransitionStep(); break;
                case WizardStep.Build: DrawBuildStep(); break;
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(8);
            GUILayout.Label("Enemy Ability Wizard", EditorStyles.largeLabel);
            GUILayout.Label("Creates the full ScriptableObject chain for one enemy ability in a single click.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);
        }

        private void DrawStepTabs()
        {
            string[] tabs =
            {
                "1. Base",
                "2. Desc",
                "3. Cost",
                "4. CD",
                "5. Anim",
                "6. Data",
                "7. Trans",
                "8. Build"
            };

            int selected = GUILayout.Toolbar((int)_step, tabs, GUILayout.Height(28));
            _step = (WizardStep)selected;
        }

        // ========================= BASIC =========================

        private void DrawBasicStep()
        {
            EditorGUILayout.LabelField("Identity and output folder", EditorStyles.boldLabel);

            _baseName = EditorGUILayout.TextField("Ability base name", _baseName);

            EditorGUILayout.HelpBox(
                "Used as suffix: description_<name>, state_<name>, usable_<name>, etc.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            _folder = EditorGUILayout.TextField("Output folder", _folder);
            if (GUILayout.Button("Select...", GUILayout.Width(80)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select ability output folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                    _folder = MakeRelativePath(selected);
            }
            EditorGUILayout.EndHorizontal();

            _overwriteExisting = EditorGUILayout.Toggle("Overwrite existing assets", _overwriteExisting);
            _autoSelectAfterCreate = EditorGUILayout.Toggle("Select root asset after create", _autoSelectAfterCreate);

            EditorGUILayout.Space(10);
            DrawNextButton();
        }

        private static string MakeRelativePath(string absolutePath)
        {
            string assetsPath = Application.dataPath;
            if (absolutePath.StartsWith(assetsPath, StringComparison.Ordinal))
            {
                string relative = absolutePath.Substring(assetsPath.Length).TrimStart('\\', '/');
                return "Assets/" + relative;
            }
            return absolutePath;
        }

        // ========================= DESCRIPTION =========================

        private void DrawDescriptionStep()
        {
            EnsureDescription();
            EditorGUILayout.LabelField("Description asset", EditorStyles.boldLabel);
            DrawPreviewEditor(_descriptionAsset);

            EditorGUILayout.Space(10);
            DrawNavigation();
        }

        private void EnsureDescription()
        {
            if (_descriptionAsset != null) return;

            _descriptionAsset = CreateInstance<Description>();
            _descriptionAsset.name = $"description_{_baseName}";
            _descriptionAsset.Title = ObjectNames.NicifyVariableName(_baseName);
        }

        // ========================= COST =========================

        private void DrawCostStep()
        {
            _createCost = EditorGUILayout.Toggle("Create use-cost effect asset", _createCost);

            if (_createCost)
            {
                EnsureCost();
                EditorGUILayout.LabelField("Use cost (AppliedStatsDeltaEffect)", EditorStyles.boldLabel);
                DrawPreviewEditor(_costAsset);
            }
            else
            {
                _costAsset = null;
            }

            EditorGUILayout.Space(10);
            DrawNavigation();
        }

        private void EnsureCost()
        {
            if (_costAsset != null || !_createCost) return;

            _costAsset = CreateInstance<AppliedStatsDeltaEffect>();
            _costAsset.name = $"usableEffect_stats_{_baseName}_cost";
        }

        // ========================= COOLDOWN =========================

        private void DrawCooldownStep()
        {
            CooldownKind newKind = (CooldownKind)EditorGUILayout.EnumPopup("Cooldown strategy type", _cooldownKind);
            if (newKind != _cooldownKind)
            {
                _cooldownKind = newKind;
                ResetCooldownAsset();
            }

            EnsureCooldown();
            EditorGUILayout.LabelField("Cooldown strategy asset", EditorStyles.boldLabel);
            DrawPreviewEditor(_cooldownAsset);

            EditorGUILayout.Space(10);
            DrawNavigation();
        }

        private void EnsureCooldown()
        {
            if (_cooldownAsset != null)
            {
                bool matches;
                switch (_cooldownKind)
                {
                    case CooldownKind.Basic:
                        matches = _cooldownAsset.GetType() == typeof(SerializedGenericCooldownStrategy);
                        break;
                    case CooldownKind.Reload:
                        matches = _cooldownAsset is SerializedReloadStrategy;
                        break;
                    case CooldownKind.Queue:
                        matches = _cooldownAsset is SerializedQueueChargesStrategy;
                        break;
                    default:
                        matches = false;
                        break;
                }

                if (!matches) ResetCooldownAsset();
            }

            if (_cooldownAsset != null) return;

            switch (_cooldownKind)
            {
                case CooldownKind.Reload:
                    _cooldownAsset = CreateInstance<SerializedReloadStrategy>();
                    break;
                case CooldownKind.Queue:
                    _cooldownAsset = CreateInstance<SerializedQueueChargesStrategy>();
                    break;
                default:
                    _cooldownAsset = CreateInstance<SerializedGenericCooldownStrategy>();
                    break;
            }

            _cooldownAsset.name = $"charges_{_baseName}_{_cooldownKind.ToString().ToLowerInvariant()}";
        }

        private void ResetCooldownAsset()
        {
            DisposeEditors();

            if (_cooldownAsset != null)
            {
                DestroyImmediate(_cooldownAsset);
                _cooldownAsset = null;
            }
        }

        // ========================= ANIMATION STATE =========================

        private void DrawAnimationStep()
        {
            EnsureState();

            EditorGUILayout.LabelField("Animation state asset", EditorStyles.boldLabel);
            DrawPreviewEditor(_stateAsset);

            EditorGUILayout.Space(10);
            DrawNavigation();
        }

        private void EnsureState()
        {
            if (_stateAsset != null) return;

            _stateAsset = CreateInstance<SerializedUnitState>();
            _stateAsset.name = $"state_{_baseName}";
            _stateAsset.stateDisplayName = ObjectNames.NicifyVariableName(_baseName);
        }

        // ========================= USABLE DATA =========================

        private void DrawUsableDataStep()
        {
            EnsureUsableDataPreview();

            EditorGUILayout.LabelField("Usable Data Containers", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Each container describes a hit producer, effect applier and triggered effects. " +
                "An empty SerializedApplyUsableEffectsResult will be created automatically and assigned to usableData[0].effects[0].",
                MessageType.Info);

            SerializedObject so = new SerializedObject(_usableStrategyPreview);
            SerializedProperty prop = so.FindProperty("usableData");
            EditorGUILayout.PropertyField(prop, new GUIContent("Usable Data"), true);
            so.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            DrawNavigation();
        }

        private void EnsureUsableDataPreview()
        {
            if (_usableStrategyPreview != null) return;

            _usableStrategyPreview = CreateInstance<SerializedUsableStrategy>();
            _usableStrategyPreview.name = $"usable_preview_{_baseName}";

            // Pre-fill one data container so user sees where effects will land.
            SerializedObject so = new SerializedObject(_usableStrategyPreview);
            SerializedProperty prop = so.FindProperty("usableData");
            prop.arraySize = 1;
            so.ApplyModifiedProperties();
        }

        // ========================= STATE TRANSITION =========================

        private void DrawTransitionStep()
        {
            EnsureState();
            EnsureUsableDataPreview();
            EnsureTransition();

            EditorGUILayout.LabelField("State transition asset", EditorStyles.boldLabel);
            DrawPreviewEditor(_transitionAsset);

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                $"'Next State' is wired to: {(_stateAsset != null ? _stateAsset.name : "<none>")}",
                MessageType.Info);

            EditorGUILayout.Space(10);
            DrawNavigation();
        }

        private void EnsureTransition()
        {
            if (_transitionAsset != null) return;

            _transitionAsset = CreateInstance<SerializedStateTransition>();
            _transitionAsset.name = $"transitionTo_{_baseName}";
            _transitionAsset.Priority = 10;
            _transitionAsset.minTimeInSourceStateNormalized = 1f;
        }

        // ========================= BUILD =========================

        private void DrawBuildStep()
        {
            EditorGUILayout.LabelField("Ready to build", EditorStyles.boldLabel);

            string cdSuffix = _cooldownKind.ToString().ToLowerInvariant();

            EditorGUILayout.HelpBox(
                $"The wizard will create folder '{_baseName}' and generate:\n" +
                $"• description_{_baseName}.asset\n" +
                (_createCost ? $"• usableEffect_stats_{_baseName}_cost.asset\n" : "") +
                $"• charges_{_baseName}_{cdSuffix}.asset\n" +
                $"• state_{_baseName}.asset\n" +
                $"• usable_package_effects_{_baseName}.asset  ← empty, pre-assigned to usableData[0].effects[0]\n" +
                $"• transitionTo_{_baseName}.asset\n" +
                $"• usable_{_baseName}.asset  ← root asset, links everything\n\n" +
                $"After creation the wizard resets for the next ability; fill the empty ApplyUsableEffectsResult in the root asset.",
                MessageType.Info);

            bool valid = Validate(out string error);
            if (!valid)
                EditorGUILayout.HelpBox(error, MessageType.Error);

            EditorGUILayout.Space(10);

            GUI.enabled = valid;
            if (GUILayout.Button("CREATE ALL ABILITY ASSETS", GUILayout.Height(40)))
            {
                CreateAllAssets();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(15);
            if (GUILayout.Button("← Back")) _step--;
        }

        private bool Validate(out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(_baseName))
            {
                error = "Base name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_folder) || !_folder.StartsWith("Assets", StringComparison.Ordinal))
            {
                error = "Output folder must be inside the project Assets/ folder.";
                return false;
            }

            if (_stateAsset != null && string.IsNullOrWhiteSpace(_stateAsset.stateDisplayName))
            {
                error = "Animation state asset requires a display name.";
                return false;
            }

            return true;
        }

        // ========================= NAVIGATION =========================

        private void DrawNavigation()
        {
            EditorGUILayout.BeginHorizontal();
            if (_step > 0)
            {
                if (GUILayout.Button("← Back")) _step--;
            }

            GUILayout.FlexibleSpace();

            if (_step < WizardStep.Build)
            {
                if (GUILayout.Button("Next →")) _step++;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNextButton()
        {
            if (GUILayout.Button("Next →")) _step++;
        }

        // ========================= PREVIEW EDITORS =========================

        private void DrawPreviewEditor(Object asset)
        {
            if (asset == null) return;

            Editor editor = GetPreviewEditor(asset);
            if (editor == null) return;

            EditorGUI.BeginChangeCheck();
            editor.OnInspectorGUI();
            EditorGUI.EndChangeCheck();
        }

        private Editor GetPreviewEditor(Object asset)
        {
            if (asset == null) return null;

            if (_previewEditors.TryGetValue(asset, out Editor editor) && editor != null)
                return editor;

            editor = Editor.CreateEditor(asset);
            _previewEditors[asset] = editor;
            return editor;
        }

        // ========================= BUILD ASSETS =========================

        private void CreateAllAssets()
        {
            if (!Validate(out string error))
            {
                EditorUtility.DisplayDialog("Validation failed", error, "OK");
                return;
            }

            string targetFolder = $"{_folder}/{_baseName}";
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                AssetDatabase.Refresh();
            }

            var createdAssets = new List<Object>();

            // 1. Description
            var description = (Description)EnsureAssetSaved(_descriptionAsset, "description_", _baseName, targetFolder);
            createdAssets.Add(description);

            // 2. Cost
            AppliedStatsDeltaEffect cost = null;
            if (_createCost)
            {
                cost = (AppliedStatsDeltaEffect)EnsureAssetSaved(_costAsset, "usableEffect_stats_", _baseName + "_cost", targetFolder);
                createdAssets.Add(cost);
            }

            // 3. Cooldown
            string cdSuffix = _cooldownKind.ToString().ToLowerInvariant();
            var cooldown = (SerializedGenericCooldownStrategy)EnsureAssetSaved(_cooldownAsset, "charges_", _baseName + "_" + cdSuffix, targetFolder);
            createdAssets.Add(cooldown);

            // 4. Unit state
            var state = (SerializedUnitState)EnsureAssetSaved(_stateAsset, "state_", _baseName, targetFolder);
            createdAssets.Add(state);

            // 5. Effects action result — created empty, will be assigned to usableData[0].effects[0]
            var effects = CreateInstance<SerializedApplyUsableEffectsResult>();
            effects.name = $"usable_package_effects_{_baseName}";
            effects = (SerializedApplyUsableEffectsResult)EnsureAssetSaved(effects, "usable_package_effects_", _baseName, targetFolder);
            createdAssets.Add(effects);

            // 6. State transition
            var transition = (SerializedStateTransition)EnsureAssetSaved(_transitionAsset, "transitionTo_", _baseName, targetFolder);
            WireTransition(transition, state);
            createdAssets.Add(transition);

            // 7. Root usable strategy
            _usableStrategyAsset = CreateInstance<SerializedUsableStrategy>();
            _usableStrategyAsset.name = $"usable_{_baseName}";
            var usable = (SerializedUsableStrategy)EnsureAssetSaved(_usableStrategyAsset, "usable_", _baseName, targetFolder);
            WireUsableStrategy(usable, description, cooldown, cost, transition, effects);
            createdAssets.Add(usable);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // CRITICAL: drop references to created assets so the next run starts fresh.
            ResetAllPreviews();

            EditorUtility.DisplayDialog(
                "Ability created",
                $"Created {createdAssets.Count} assets in:\n{targetFolder}\n\n" +
                $"Open usable_{_baseName} and fill the empty SerializedApplyUsableEffectsResult assigned to usableData[0].effects[0].",
                "OK");

            if (_autoSelectAfterCreate)
            {
                Selection.activeObject = usable;
                EditorGUIUtility.PingObject(usable);
            }
        }

        private Object EnsureAssetSaved(Object asset, string prefix, string name, string folder)
        {
            if (asset == null) return null;

            string path = $"{folder}/{prefix}{name}.asset";

            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                if (_overwriteExisting)
                {
                    AssetDatabase.DeleteAsset(path);
                }
                else
                {
                    path = AssetDatabase.GenerateUniqueAssetPath(path);
                }
            }

            // Safety net: if somehow a stale saved asset is still referenced, don't reuse it.
            if (AssetDatabase.Contains(asset) && AssetDatabase.GetAssetPath(asset) != path)
            {
                Debug.LogError($"[EnemyAbilityWizard] The asset {asset.name} is already saved to {AssetDatabase.GetAssetPath(asset)}. A new preview instance is needed. This should not happen after ResetAllPreviews().");
                return null;
            }

            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private void WireTransition(SerializedStateTransition transition, SerializedUnitState nextState)
        {
            SerializedObject so = new SerializedObject(transition);

            so.FindProperty("nextState").objectReferenceValue = nextState;

            SerializedProperty onTransition = so.FindProperty("onTransition");
            onTransition.arraySize = 0;

            so.ApplyModifiedProperties();
        }

        private void WireUsableStrategy(
            SerializedUsableStrategy usable,
            Description description,
            SerializedGenericCooldownStrategy cooldown,
            AppliedStatsDeltaEffect cost,
            SerializedStateTransition transition,
            SerializedApplyUsableEffectsResult effects)
        {
            SerializedObject so = new SerializedObject(usable);

            so.FindProperty("description").objectReferenceValue = description;

            SerializedProperty settings = so.FindProperty("settings");
            settings.FindPropertyRelative("charge").objectReferenceValue = cooldown;
            settings.FindPropertyRelative("useCost").objectReferenceValue = cost;
            settings.FindPropertyRelative("drawItemsStrategy").objectReferenceValue = null;

            so.FindProperty("useStateTransition").objectReferenceValue = transition;

            // Copy usableData from the preview asset, then assign empty effects result.
            SerializedProperty usableData = so.FindProperty("usableData");
            if (_usableStrategyPreview != null)
            {
                SerializedObject previewSo = new SerializedObject(_usableStrategyPreview);
                SerializedProperty previewData = previewSo.FindProperty("usableData");
                CopyUsableDataArray(previewData, usableData);
            }
            else
            {
                usableData.arraySize = 0;
            }

            // Ensure at least one container and assign the empty effects result to it.
            if (usableData.arraySize == 0)
                usableData.arraySize = 1;

            SerializedProperty firstContainer = usableData.GetArrayElementAtIndex(0);
            SerializedProperty firstEffects = firstContainer.FindPropertyRelative("effects");
            firstEffects.arraySize = 1;
            firstEffects.GetArrayElementAtIndex(0).objectReferenceValue = effects;

            so.ApplyModifiedProperties();
        }

        private void CopyUsableDataArray(SerializedProperty source, SerializedProperty destination)
        {
            destination.arraySize = source.arraySize;

            for (int i = 0; i < source.arraySize; i++)
            {
                SerializedProperty src = source.GetArrayElementAtIndex(i);
                SerializedProperty dst = destination.GetArrayElementAtIndex(i);

                dst.FindPropertyRelative("hitProducer").objectReferenceValue = src.FindPropertyRelative("hitProducer").objectReferenceValue;
                dst.FindPropertyRelative("proceedOnSelfHit").boolValue = src.FindPropertyRelative("proceedOnSelfHit").boolValue;

                dst.FindPropertyRelative("effectApplier").objectReferenceValue = src.FindPropertyRelative("effectApplier").objectReferenceValue;
                dst.FindPropertyRelative("applicationEffect").objectReferenceValue = src.FindPropertyRelative("applicationEffect").objectReferenceValue;

                SerializedProperty srcEffects = src.FindPropertyRelative("effects");
                SerializedProperty dstEffects = dst.FindPropertyRelative("effects");
                dstEffects.arraySize = srcEffects.arraySize;
                for (int j = 0; j < srcEffects.arraySize; j++)
                {
                    dstEffects.GetArrayElementAtIndex(j).objectReferenceValue = srcEffects.GetArrayElementAtIndex(j).objectReferenceValue;
                }

                dst.FindPropertyRelative("onInvalidHit").objectReferenceValue = src.FindPropertyRelative("onInvalidHit").objectReferenceValue;
            }
        }
    }
}

#endif