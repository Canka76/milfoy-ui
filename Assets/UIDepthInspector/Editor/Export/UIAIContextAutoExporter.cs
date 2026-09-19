using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;

namespace UIDepthInspector.Editor.Export
{
    /// <summary>
    /// Background watcher that automatically exports token-optimized UI diagnostic context
    /// whenever scenes are saved in the Editor.
    /// Eliminates the need for external AI agents to launch heavy Unity batchmode instances.
    /// </summary>
    [InitializeOnLoad]
    public static class UIAIContextAutoExporter
    {
        public const string OutputDirectory = ".milfoy";
        public const string ActiveContextMarkdown = ".milfoy/ui-context.md";
        public const string ActiveContextJson = ".milfoy/ui-context.json";

        static UIAIContextAutoExporter()
        {
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        static void OnSceneSaved(Scene scene)
        {
            ExportActiveContext();
        }

        /// <summary>
        /// Collects the active Canvas hierarchy, analyzes spatial occlusions & ghost blockers,
        /// and dumps compact Markdown and JSON action patches to .milfoy/.
        /// </summary>
        public static void ExportActiveContext()
        {
            try
            {
                var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (canvases == null || canvases.Length == 0)
                    return;

                // Pick the primary root canvas
                Canvas rootCanvas = null;
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i].isRootCanvas)
                    {
                        rootCanvas = canvases[i];
                        break;
                    }
                }
                if (rootCanvas == null) rootCanvas = canvases[0];

                var entries = UIRenderTreeCollector.Collect(canvases);
                UIDiagnosticAnalyzer.Analyze(entries);

                string mdContent = UIAIContextExporter.ExportToCompactMarkdown(entries, rootCanvas, ExportMode.Anomalies);
                string jsonContent = UIAIContextExporter.ExportToJson(entries, rootCanvas, ExportMode.Anomalies, prettyPrint: true);

                if (!Directory.Exists(OutputDirectory))
                {
                    Directory.CreateDirectory(OutputDirectory);
                }

                File.WriteAllText(ActiveContextMarkdown, mdContent);
                File.WriteAllText(ActiveContextJson, jsonContent);
            }
            catch (Exception ex)
            {
                // Never block or crash the editor on background export
                Debug.LogWarning($"[Milfoy AutoExporter] Background export skipped: {ex.Message}");
            }
        }
    }
}
