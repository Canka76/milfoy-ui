using System;
using System.IO;
using UnityEngine;
using UnityEditor;
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
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
    }
}
