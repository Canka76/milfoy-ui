using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UIDepthInspector.Editor.Benchmark;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;

namespace UIDepthInspector.Editor.Export
{
    /// <summary>
    /// Headless CLI entry points for automated AI agents and batchmode runners.
    /// Can be executed via:
    ///   Unity.exe -batchmode -quit -projectPath . -executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.DumpContext -dumpFormat md -dumpMode anomalies -dumpOutput "ui-context.md"
    /// </summary>
    public static class UIDepthInspectorCLI
    {
        public const string BeginDelimiter = "=== BEGIN UI AI CONTEXT ===";
        public const string EndDelimiter = "=== END UI AI CONTEXT ===";

        public static void DumpContext()
        {
            var args = Environment.GetCommandLineArgs();
            string format = GetArg(args, "-dumpFormat", "md").ToLowerInvariant();
            string modeStr = GetArg(args, "-dumpMode", "anomalies");
            string outputPath = GetArg(args, "-dumpOutput", "");
            string canvasFilter = GetArg(args, "-canvasName", "");

            ExportMode mode = ParseExportMode(modeStr);
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            Canvas targetCanvas = null;

            if (canvases != null && canvases.Length > 0)
            {
                if (!string.IsNullOrEmpty(canvasFilter))
                {
                    for (int i = 0; i < canvases.Length; i++)
                    {
                        if (string.Equals(canvases[i].name, canvasFilter, StringComparison.OrdinalIgnoreCase))
                        {
                            targetCanvas = canvases[i];
                            break;
                        }
                    }
                }

                if (targetCanvas == null)
                    targetCanvas = canvases[0];
            }

            if (targetCanvas == null)
            {
                string noCanvasMsg = "No active Canvas found in the current scene.";
                if (!string.IsNullOrEmpty(outputPath))
                    File.WriteAllText(outputPath, noCanvasMsg);
                else
                    Debug.LogWarning($"[UIDepthInspectorCLI] {noCanvasMsg}");
                return;
            }

            var entries = UIRenderTreeCollector.Collect(new Canvas[] { targetCanvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            string content;
            if (format == "json")
            {
                content = UIAIContextExporter.ExportToJson(entries, targetCanvas, mode, prettyPrint: true);
            }
            else
            {
                content = UIAIContextExporter.ExportToCompactMarkdown(entries, targetCanvas, mode);
            }

            if (!string.IsNullOrEmpty(outputPath))
            {
                try
                {
                    string dir = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(outputPath, content);
                    Debug.Log($"[UIDepthInspectorCLI] Exported UI AI context ({format}, {mode}) to '{outputPath}'.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UIDepthInspectorCLI] Failed writing to '{outputPath}': {ex.Message}");
                }
            }
            else
            {
                // Print directly to console stdout with clean delimiters for AI agents
                Debug.Log($"{BeginDelimiter}\n{content}\n{EndDelimiter}");
            }
        }

        public static ExportMode ParseExportMode(string modeStr)
        {
            if (string.IsNullOrEmpty(modeStr))
                return ExportMode.Anomalies;

            if (string.Equals(modeStr, "compact", StringComparison.OrdinalIgnoreCase))
                return ExportMode.Compact;

            if (string.Equals(modeStr, "full", StringComparison.OrdinalIgnoreCase))
                return ExportMode.Full;

            return ExportMode.Anomalies;
        }

        public static string GetArg(string[] args, string paramName, string defaultValue)
        {
            if (args == null || args.Length == 0) return defaultValue;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], paramName, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return defaultValue;
        }
        public static void GenerateBenchmark()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                string presetStr = GetArg(args, "-benchmarkPreset", "CasualHud");
                string seedStr = GetArg(args, "-benchmarkSeed", "42");
                string outDir = GetArg(args, "-benchmarkOutDir", "");

                if (!int.TryParse(seedStr, out int seed))
                {
                    seed = 42;
                }

                UIBenchmarkPresetType presetType = ParseBenchmarkPreset(presetStr);
                GenerateBenchmark(presetType, seed, outDir);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIDepthInspectorCLI] GenerateBenchmark failed: {ex.Message}\n{ex.StackTrace}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw;
            }
        }

        public static UIBenchmarkGroundTruth GenerateBenchmark(UIBenchmarkPresetType presetType, int seed, string outDir)
        {
            var config = UIBenchmarkPreset.GetConfig(presetType);

            if (string.IsNullOrEmpty(outDir))
            {
                outDir = Path.Combine("BenchmarkTrials", $"Run_{presetType}_{seed}");
            }

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            var rootGo = UISyntheticSceneGenerator.Generate(config, seed, out var groundTruth);

            // Export benchmark-ground-truth.json
            string gtJson = UIBenchmarkGroundTruth.ToJson(groundTruth, prettyPrint: true);
            File.WriteAllText(Path.Combine(outDir, "benchmark-ground-truth.json"), gtJson);

            // Export challenge-instructions.md
            string instructions = $@"# Milfoy Agent Diagnostic Challenge: {groundTruth.benchmarkId}

## Mission
Analyze the active Unity uGUI scene hierarchy and identify all UI defects and anomalies.

## Preset Configuration
- Preset: {groundTruth.preset}
- Seed: {groundTruth.seed}
- Total Generated Elements: {groundTruth.totalElements}
- Total Injected Anomalies: {groundTruth.totalAnomalies}

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
            File.WriteAllText(Path.Combine(outDir, "challenge-instructions.md"), instructions);

            // Export agent folders & template trial records
            string milfoyAgentDir = Path.Combine(outDir, "milfoy-agent");
            string baselineAgentDir = Path.Combine(outDir, "baseline-agent");
            Directory.CreateDirectory(milfoyAgentDir);
            Directory.CreateDirectory(baselineAgentDir);

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

            // Export active Milfoy UI context from the generated hierarchy
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            if (canvases != null && canvases.Length > 0)
            {
                Canvas primaryCanvas = null;
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i].isRootCanvas)
                    {
                        primaryCanvas = canvases[i];
                        break;
                    }
                }
                if (primaryCanvas == null) primaryCanvas = canvases[0];

                var entries = UIRenderTreeCollector.Collect(canvases);
                UIDiagnosticAnalyzer.Analyze(entries);

                string mdContext = UIAIContextExporter.ExportToCompactMarkdown(entries, primaryCanvas, ExportMode.Anomalies);
                string jsonContext = UIAIContextExporter.ExportToJson(entries, primaryCanvas, ExportMode.Anomalies, prettyPrint: true);

                File.WriteAllText(Path.Combine(outDir, "ui-context.md"), mdContext);
                File.WriteAllText(Path.Combine(outDir, "ui-context.json"), jsonContext);
                File.WriteAllText(Path.Combine(milfoyAgentDir, "ui-context.md"), mdContext);
                File.WriteAllText(Path.Combine(milfoyAgentDir, "ui-context.json"), jsonContext);
            }

            // Sync passive cache
            UIAIContextAutoExporter.ExportActiveContext();

            Debug.Log($"[UIDepthInspectorCLI] Successfully generated benchmark '{groundTruth.benchmarkId}' with {groundTruth.totalElements} elements and {groundTruth.totalAnomalies} anomalies to '{outDir}'.");
            return groundTruth;
        }

        public static void EvaluateBenchmark()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                string groundTruthPath = GetArg(args, "-groundTruthPath", "");
                string milfoyResultPath = GetArg(args, "-milfoyResultPath", "");
                string baselineResultPath = GetArg(args, "-baselineResultPath", "");
                string reportOutDir = GetArg(args, "-reportOutDir", "");

                if (string.IsNullOrEmpty(groundTruthPath) || !File.Exists(groundTruthPath))
                {
                    Debug.LogError($"[UIDepthInspectorCLI] Ground truth file not found or path missing: '{groundTruthPath}'");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }

                if (string.IsNullOrEmpty(milfoyResultPath) || !File.Exists(milfoyResultPath))
                {
                    Debug.LogError($"[UIDepthInspectorCLI] Milfoy result file not found or path missing: '{milfoyResultPath}'");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }

                if (string.IsNullOrEmpty(baselineResultPath) || !File.Exists(baselineResultPath))
                {
                    Debug.LogError($"[UIDepthInspectorCLI] Baseline result file not found or path missing: '{baselineResultPath}'");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }

                EvaluateBenchmark(groundTruthPath, milfoyResultPath, baselineResultPath, reportOutDir);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIDepthInspectorCLI] EvaluateBenchmark failed: {ex.Message}\n{ex.StackTrace}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw;
            }
        }

        public static TrialComparisonMetrics EvaluateBenchmark(string groundTruthPath, string milfoyResultPath, string baselineResultPath, string reportOutDir)
        {
            if (string.IsNullOrEmpty(groundTruthPath) || !File.Exists(groundTruthPath))
                throw new FileNotFoundException("Ground truth file not found", groundTruthPath);
            if (string.IsNullOrEmpty(milfoyResultPath) || !File.Exists(milfoyResultPath))
                throw new FileNotFoundException("Milfoy result file not found", milfoyResultPath);
            if (string.IsNullOrEmpty(baselineResultPath) || !File.Exists(baselineResultPath))
                throw new FileNotFoundException("Baseline result file not found", baselineResultPath);

            string gtJson = File.ReadAllText(groundTruthPath);
            var groundTruth = UIBenchmarkGroundTruth.FromJson(gtJson);

            string milfoyJson = File.ReadAllText(milfoyResultPath);
            var milfoyTrial = AgentTrialRecord.FromJson(milfoyJson);

            string baselineJson = File.ReadAllText(baselineResultPath);
            var baselineTrial = AgentTrialRecord.FromJson(baselineJson);

            var comparison = UIBenchmarkEvaluator.Evaluate(groundTruth, milfoyTrial, baselineTrial);
            string mdReport = UIBenchmarkEvaluator.FormatMarkdownReport(comparison);
            string csvReport = UIBenchmarkEvaluator.FormatCsvReport(comparison);

            if (string.IsNullOrEmpty(reportOutDir))
            {
                reportOutDir = Path.GetDirectoryName(milfoyResultPath);
                if (string.IsNullOrEmpty(reportOutDir))
                    reportOutDir = "BenchmarkTrials";
            }

            if (!Directory.Exists(reportOutDir))
            {
                Directory.CreateDirectory(reportOutDir);
            }

            string mdPath = Path.Combine(reportOutDir, "benchmark-report.md");
            string csvPath = Path.Combine(reportOutDir, "benchmark-report.csv");
            File.WriteAllText(mdPath, mdReport);
            File.WriteAllText(csvPath, csvReport);

            Debug.Log($"=== BEGIN BENCHMARK REPORT ===\n{mdReport}\n=== END BENCHMARK REPORT ===");
            Debug.Log($"[UIDepthInspectorCLI] Successfully evaluated benchmark. Reports written to '{mdPath}' and '{csvPath}'.");

            return comparison;
        }

        public static UIBenchmarkPresetType ParseBenchmarkPreset(string presetStr)
        {
            if (string.IsNullOrEmpty(presetStr))
                return UIBenchmarkPresetType.CasualHud;

            if (Enum.TryParse<UIBenchmarkPresetType>(presetStr, true, out var preset))
            {
                return preset;
            }

            return UIBenchmarkPresetType.CasualHud;
        }
    }
}
