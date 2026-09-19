using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UIDepthInspector.Editor.Export;

namespace UIDepthInspector.Editor.Benchmark
{
    public class UIBenchmarkWindow : EditorWindow
    {
        [MenuItem("Tools/Milfoy/Benchmark Lab", false, 100)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<UIBenchmarkWindow>("Benchmark Lab");
            wnd.minSize = new Vector2(480, 520);
            wnd.Show();
        }

        [SerializeField] UIBenchmarkPresetType _selectedPreset = UIBenchmarkPresetType.CasualHud;
        [SerializeField] int _seed = 42;
        [SerializeField] int _targetElementCount = 35;
        [SerializeField] int _canvasCount = 1;
        [SerializeField] bool _injectGhostBlockers = true;
        [SerializeField] bool _injectSpatialOverlaps = true;
        [SerializeField] bool _injectMissingSprites = false;
        [SerializeField] bool _injectNestedRaycasts = true;
        [SerializeField] bool _injectCanvasGroupTraps = false;

        [SerializeField] string _lastGeneratedJson = "";
        [SerializeField] string _statusMessage = "Ready. Configure parameters and generate a synthetic benchmark.";
        [SerializeField] MessageType _statusType = MessageType.Info;

        Vector2 _scrollPos;
        bool _showAnomaliesFoldout = true;
        bool _showDetailsFoldout = false;
        UIBenchmarkGroundTruth _cachedGroundTruth;

        void OnEnable()
        {
            ApplyPresetConfig(_selectedPreset);
        }

        void ApplyPresetConfig(UIBenchmarkPresetType preset)
        {
            _selectedPreset = preset;
            if (preset == UIBenchmarkPresetType.Custom) return;

            var config = UIBenchmarkPreset.GetConfig(preset);
            _targetElementCount = config.targetElementCount;
            _canvasCount = config.canvasCount;
            _injectGhostBlockers = config.injectGhostBlockers;
            _injectSpatialOverlaps = config.injectSpatialOverlaps;
            _injectMissingSprites = config.injectMissingSprites;
            _injectNestedRaycasts = config.injectNestedLabelRaycasts;
            _injectCanvasGroupTraps = config.injectCanvasGroupTraps;
        }

        void MarkCustom()
        {
            _selectedPreset = UIBenchmarkPresetType.Custom;
        }

        void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawHeader();
            EditorGUILayout.Space(6);

            DrawPresetSection();
            EditorGUILayout.Space(8);

            DrawConfigurationSection();
            EditorGUILayout.Space(8);

            DrawActionsSection();
            EditorGUILayout.Space(8);

            DrawStatusSection();

            EditorGUILayout.EndScrollView();
        }

        void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft
                };
                EditorGUILayout.LabelField("Milfoy UI Benchmark Lab", titleStyle);
                EditorGUILayout.LabelField("Procedural uGUI synthetic scene generator & dual-agent evaluation testbed.", EditorStyles.miniLabel);
            }
        }

        void DrawPresetSection()
        {
            EditorGUILayout.LabelField("Benchmark Preset", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newPreset = (UIBenchmarkPresetType)EditorGUILayout.EnumPopup("Preset", _selectedPreset);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyPresetConfig(newPreset);
            }
        }

        void DrawConfigurationSection()
        {
            EditorGUILayout.LabelField("Simulation Parameters", EditorStyles.boldLabel);

            // Seed & Randomize
            using (new EditorGUILayout.HorizontalScope())
            {
                _seed = EditorGUILayout.IntField(new GUIContent("RNG Seed", "Deterministic random generator seed."), _seed);
                if (GUILayout.Button("Randomize Seed", GUILayout.Width(120)))
                {
                    _seed = UnityEngine.Random.Range(1, 999999);
                    GUI.FocusControl(null);
                }
            }

            // Element count
            EditorGUI.BeginChangeCheck();
            _targetElementCount = EditorGUILayout.IntSlider(
                new GUIContent("Target Elements", "Target number of UI elements to procedurally generate (15 to 120)."),
                _targetElementCount, 15, 120);
            if (EditorGUI.EndChangeCheck())
            {
                MarkCustom();
            }

            // Anomaly checkboxes
            _showAnomaliesFoldout = EditorGUILayout.Foldout(_showAnomaliesFoldout, "Injected Anomaly Categories", true);
            if (_showAnomaliesFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();

                _injectGhostBlockers = EditorGUILayout.Toggle(
                    new GUIContent("Ghost Blockers", "Transparent images (alpha=0) that intercept raycasts."),
                    _injectGhostBlockers);

                _injectSpatialOverlaps = EditorGUILayout.Toggle(
                    new GUIContent("Spatial Overlaps", "Higher-order sibling cards or headers occluding interactive buttons underneath."),
                    _injectSpatialOverlaps);

                _injectMissingSprites = EditorGUILayout.Toggle(
                    new GUIContent("Missing Sprites", "Active Graphic components with missing (null) sprite references but raycasts enabled."),
                    _injectMissingSprites);

                _injectNestedRaycasts = EditorGUILayout.Toggle(
                    new GUIContent("Nested Raycasts", "Nested child Text/Labels inside buttons with redundant raycastTarget=true."),
                    _injectNestedRaycasts);

                _injectCanvasGroupTraps = EditorGUILayout.Toggle(
                    new GUIContent("CanvasGroup Traps", "CanvasGroups with blocksRaycasts=false or alpha=0 disabling interaction."),
                    _injectCanvasGroupTraps);

                if (EditorGUI.EndChangeCheck())
                {
                    MarkCustom();
                }
                EditorGUI.indentLevel--;
            }
        }

        void DrawActionsSection()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            // Generate Button
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.35f, 0.75f, 1f, 1f);
            if (GUILayout.Button(new GUIContent("Generate Benchmark UI", "Generates deterministic uGUI synthetic hierarchy and writes ground truth JSON."), GUILayout.Height(30)))
            {
                GenerateBenchmark();
            }
            GUI.backgroundColor = oldColor;

            EditorGUILayout.Space(2);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Clear Benchmark UI", "Tears down any generated benchmark hierarchy in the scene.")))
                {
                    ClearBenchmark();
                }

                if (GUILayout.Button(new GUIContent("Inspect in 3D Viewport", "Opens UIDepthInspector and selects the generated benchmark Canvas.")))
                {
                    InspectInViewport();
                }
            }

            EditorGUILayout.Space(2);

            if (GUILayout.Button(new GUIContent("Export Agent Challenge Pack", "Packages trial folders for Milfoy-equipped and baseline agents into BenchmarkTrials/.")))
            {
                ExportChallengePack();
            }
        }

        void DrawStatusSection()
        {
            EditorGUILayout.LabelField("Status & Ground Truth", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(_statusMessage, _statusType);

            if (_cachedGroundTruth != null && _cachedGroundTruth.anomalies != null && _cachedGroundTruth.anomalies.Count > 0)
            {
                _showDetailsFoldout = EditorGUILayout.Foldout(_showDetailsFoldout, $"Active Anomalies ({_cachedGroundTruth.anomalies.Count})", true);
                if (_showDetailsFoldout)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < _cachedGroundTruth.anomalies.Count; i++)
                    {
                        var a = _cachedGroundTruth.anomalies[i];
                        EditorGUILayout.LabelField($"[{a.type}] {a.targetPath}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        void GenerateBenchmark()
        {
            try
            {
                var config = new UIBenchmarkPresetConfig
                {
                    presetType = _selectedPreset,
                    name = _selectedPreset.ToString(),
                    targetElementCount = _targetElementCount,
                    canvasCount = _canvasCount,
                    injectGhostBlockers = _injectGhostBlockers,
                    injectSpatialOverlaps = _injectSpatialOverlaps,
                    injectMissingSprites = _injectMissingSprites,
                    injectNestedLabelRaycasts = _injectNestedRaycasts,
                    injectCanvasGroupTraps = _injectCanvasGroupTraps
                };

                var rootGo = UISyntheticSceneGenerator.Generate(config, _seed, out _cachedGroundTruth);

                // Export benchmark-ground-truth.json
                string json = UIBenchmarkGroundTruth.ToJson(_cachedGroundTruth, prettyPrint: true);
                _lastGeneratedJson = json;
                File.WriteAllText("benchmark-ground-truth.json", json);

                if (!Directory.Exists(UIAIContextAutoExporter.OutputDirectory))
                {
                    Directory.CreateDirectory(UIAIContextAutoExporter.OutputDirectory);
                }
                File.WriteAllText(Path.Combine(UIAIContextAutoExporter.OutputDirectory, "benchmark-ground-truth.json"), json);

                // Trigger passive auto-export
                UIAIContextAutoExporter.ExportActiveContext();

                // Select primary canvas
                var canvas = rootGo.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    Selection.activeGameObject = canvas.gameObject;
                }
                else
                {
                    Selection.activeGameObject = rootGo;
                }

                _statusMessage = $"Generated benchmark scene '{_cachedGroundTruth.benchmarkId}' with {_cachedGroundTruth.totalElements} elements and {_cachedGroundTruth.totalAnomalies} anomalies.\nSaved 'benchmark-ground-truth.json' and updated '.milfoy/ui-context.md'.";
                _statusType = MessageType.Info;
                Debug.Log($"[UIBenchmarkWindow] {_statusMessage}");
            }
            catch (Exception ex)
            {
                _statusMessage = $"Generation failed: {ex.Message}";
                _statusType = MessageType.Error;
                Debug.LogError($"[UIBenchmarkWindow] {ex}");
            }
        }

        void ClearBenchmark()
        {
            try
            {
                UISyntheticSceneGenerator.ClearBenchmarkUI();
                _cachedGroundTruth = null;
                _statusMessage = "Cleared benchmark UI containers from active scene.";
                _statusType = MessageType.Info;
                Debug.Log("[UIBenchmarkWindow] Cleared benchmark UI hierarchy.");
            }
            catch (Exception ex)
            {
                _statusMessage = $"Failed to clear benchmark UI: {ex.Message}";
                _statusType = MessageType.Error;
                Debug.LogError($"[UIBenchmarkWindow] {ex}");
            }
        }

        void InspectInViewport()
        {
            try
            {
                var rootGo = GameObject.Find(UISyntheticSceneGenerator.RootContainerName);
                if (rootGo != null)
                {
                    var canvas = rootGo.GetComponentInChildren<Canvas>();
                    Selection.activeGameObject = canvas != null ? canvas.gameObject : rootGo;
                }

                UIDepthInspectorWindow.ShowWindow();
                _statusMessage = "Opened Milfoy 3D Viewport and focused generated benchmark Canvas.";
                _statusType = MessageType.Info;
            }
            catch (Exception ex)
            {
                _statusMessage = $"Failed to inspect in viewport: {ex.Message}";
                _statusType = MessageType.Error;
                Debug.LogError($"[UIBenchmarkWindow] {ex}");
            }
        }

        void ExportChallengePack()
        {
            try
            {
                if (_cachedGroundTruth == null)
                {
                    if (File.Exists("benchmark-ground-truth.json"))
                    {
                        string savedJson = File.ReadAllText("benchmark-ground-truth.json");
                        _cachedGroundTruth = UIBenchmarkGroundTruth.FromJson(savedJson);
                    }
                    else
                    {
                        GenerateBenchmark();
                    }
                }

                if (_cachedGroundTruth == null)
                {
                    _statusMessage = "Cannot export challenge pack: No active benchmark ground truth found.";
                    _statusType = MessageType.Warning;
                    return;
                }

                string runId = $"Run_{_selectedPreset}_{_seed}";
                string baseDir = Path.Combine("BenchmarkTrials", runId);
                string milfoyAgentDir = Path.Combine(baseDir, "milfoy-agent");
                string baselineAgentDir = Path.Combine(baseDir, "baseline-agent");

                Directory.CreateDirectory(baseDir);
                Directory.CreateDirectory(milfoyAgentDir);
                Directory.CreateDirectory(baselineAgentDir);

                // Ground truth
                string gtJson = UIBenchmarkGroundTruth.ToJson(_cachedGroundTruth, prettyPrint: true);
                File.WriteAllText(Path.Combine(baseDir, "benchmark-ground-truth.json"), gtJson);

                // Challenge instructions
                string instructions = $@"# Milfoy Agent Diagnostic Challenge: {_cachedGroundTruth.benchmarkId}

## Mission
Analyze the active Unity uGUI scene hierarchy and identify all UI defects and anomalies.

## Injected Anomaly Categories
- Ghost Blockers: Transparent graphics intercepting raycasts
- Spatial Overlaps: Interactive elements occluded by siblings/parents
- Missing Sprites: Active graphics with null sprites and raycastTarget enabled
- Nested Raycasts: Redundant raycastTarget=true on nested labels
- CanvasGroup Traps: CanvasGroups with blocksRaycasts=false or alpha=0 disabling interaction

## Output Requirements
Output an `AgentTrialRecord` JSON file containing your reported issue paths:
```json
{{
  ""agentName"": ""YourAgentName"",
  ""totalPromptTokens"": 1000,
  ""totalCompletionTokens"": 300,
  ""turns"": 2,
  ""durationSeconds"": 5.0,
  ""reportedIssuePaths"": [
    ""Canvas/Path/To/DefectiveElement""
  ]
}}
```
";
                File.WriteAllText(Path.Combine(baseDir, "challenge-instructions.md"), instructions);

                // Template trial records
                var emptyMilfoyRecord = new AgentTrialRecord
                {
                    agentName = "MilfoyEquippedAgent",
                    totalPromptTokens = 0,
                    totalCompletionTokens = 0,
                    turns = 0,
                    durationSeconds = 0f,
                    reportedIssuePaths = new List<string>()
                };
                File.WriteAllText(Path.Combine(milfoyAgentDir, "trial-record-template.json"), AgentTrialRecord.ToJson(emptyMilfoyRecord, prettyPrint: true));

                var emptyBaselineRecord = new AgentTrialRecord
                {
                    agentName = "BaselineAgent",
                    totalPromptTokens = 0,
                    totalCompletionTokens = 0,
                    turns = 0,
                    durationSeconds = 0f,
                    reportedIssuePaths = new List<string>()
                };
                File.WriteAllText(Path.Combine(baselineAgentDir, "trial-record-template.json"), AgentTrialRecord.ToJson(emptyBaselineRecord, prettyPrint: true));

                // Copy active milfoy context into milfoy-agent folder if present
                string milfoyMdPath = Path.Combine(UIAIContextAutoExporter.OutputDirectory, "ui-context.md");
                if (File.Exists(milfoyMdPath))
                {
                    File.Copy(milfoyMdPath, Path.Combine(milfoyAgentDir, "ui-context.md"), true);
                }
                string milfoyJsonPath = Path.Combine(UIAIContextAutoExporter.OutputDirectory, "ui-context.json");
                if (File.Exists(milfoyJsonPath))
                {
                    File.Copy(milfoyJsonPath, Path.Combine(milfoyAgentDir, "ui-context.json"), true);
                }

                _statusMessage = $"Exported Agent Challenge Pack to '{baseDir}'.\n- benchmark-ground-truth.json\n- milfoy-agent/\n- baseline-agent/";
                _statusType = MessageType.Info;
                Debug.Log($"[UIBenchmarkWindow] Exported challenge pack to '{baseDir}'.");
            }
            catch (Exception ex)
            {
                _statusMessage = $"Challenge pack export failed: {ex.Message}";
                _statusType = MessageType.Error;
                Debug.LogError($"[UIBenchmarkWindow] {ex}");
            }
        }
    }
}
