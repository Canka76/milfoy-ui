using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;

namespace UIDepthInspector.Editor.Export
{
    public enum ExportMode
    {
        Anomalies,
        Compact,
        Full
    }

    /// <summary>
    /// Generates token-optimized diagnostic summaries (Compact Markdown & JSON)
    /// designed specifically for AI models (Claude, ChatGPT, Gemini) and headless inspection.
    /// </summary>
    public static class UIAIContextExporter
    {
        [Serializable]
        public class ExportCanvasInfo
        {
            public string canvasName;
            public string renderMode;
            public int sortingOrder;
            public int totalElements;
            public string exportMode;
            public List<ExportElementInfo> elements = new List<ExportElementInfo>();
            public List<ExportAnomalyInfo> anomalies = new List<ExportAnomalyInfo>();
        }

        [Serializable]
        public class ExportElementInfo
        {
            public int drawIndex;
            public string name;
            public string hierarchyPath;
            public string type;
            public float x;
            public float y;
            public float width;
            public float height;
            public float effectiveAlpha;
            public bool raycastTarget;
            public bool blocksRaycasts;
            public string flags;
        }

        [Serializable]
        public class ExportFixAction
        {
            public string targetPath;
            public string component;
            public string property;
            public bool value;
        }

        [Serializable]
        public class ExportAnomalyInfo
        {
            public string severity;
            public string flag;
            public string elementName;
            public int drawIndex;
            public string description;
            public string suggestedFix;
            public ExportFixAction fixAction;
        }

        public static string ExportToCompactMarkdown(IReadOnlyList<UIElementEntry> entries, Canvas rootCanvas, ExportMode mode = ExportMode.Compact)
        {
            if (entries == null || entries.Count == 0)
                return "# UI Diagnostic Report\n*No UI elements detected in hierarchy.*";

            var sb = new StringBuilder(1024);
            string canvasName = rootCanvas != null ? rootCanvas.name : "Canvas";
            string renderMode = rootCanvas != null ? rootCanvas.renderMode.ToString() : "Unknown";

            sb.AppendLine("# UI Depth Inspector - AI Diagnostic Report");
            sb.AppendFormat("**Canvas**: {0} ({1}) | **Total Elements**: {2} | **Draw Range**: [0..{3}]\n\n",
                canvasName, renderMode, entries.Count, Mathf.Max(0, entries.Count - 1));

            Transform rootTransform = rootCanvas != null ? rootCanvas.transform : null;

            if (mode == ExportMode.Anomalies)
            {
                sb.AppendLine("## Hierarchy & Draw Order (Anomalies Only)");
                bool hasAny = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (!HasAnomaly(entry))
                        continue;

                    hasAny = true;
                    int depth = GetHierarchyDepth(entry.Transform, rootTransform);
                    string typeName = GetElementTypeName(entry.Transform);
                    string indent = GetHierarchyIndent(depth);
                    string flagsStr = FormatFlags(entry.Flags);

                    sb.AppendFormat("{0}[#{1:D2}] {2} ({3})",
                        indent,
                        entry.GlobalDrawIndex,
                        entry.Name,
                        typeName);

                    if (!string.IsNullOrEmpty(flagsStr))
                    {
                        sb.Append(" ").Append(flagsStr);
                    }

                    if (entry.RaycastTarget && entry.EffectiveAlpha < 1f)
                    {
                        sb.AppendFormat(" (Alpha: {0:0.##})", entry.EffectiveAlpha);
                    }

                    sb.AppendLine();
                }

                if (!hasAny)
                {
                    sb.AppendLine("*No anomalous elements detected.*");
                }
            }
            else if (mode == ExportMode.Compact)
            {
                sb.AppendLine("## Hierarchy & Draw Order");
                int healthyRun = 0;
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    bool isAnomaly = HasAnomaly(entry);

                    if (!isAnomaly)
                    {
                        healthyRun++;
                        continue;
                    }

                    if (healthyRun > 0)
                    {
                        sb.AppendFormat("  [+ {0} healthy element{1}]\n", healthyRun, healthyRun > 1 ? "s" : "");
                        healthyRun = 0;
                    }

                    int depth = GetHierarchyDepth(entry.Transform, rootTransform);
                    string typeName = GetElementTypeName(entry.Transform);
                    string indent = GetHierarchyIndent(depth);
                    string flagsStr = FormatFlags(entry.Flags);

                    sb.AppendFormat("{0}[#{1:D2}] {2} ({3})",
                        indent,
                        entry.GlobalDrawIndex,
                        entry.Name,
                        typeName);

                    if (!string.IsNullOrEmpty(flagsStr))
                    {
                        sb.Append(" ").Append(flagsStr);
                    }

                    if (entry.RaycastTarget && entry.EffectiveAlpha < 1f)
                    {
                        sb.AppendFormat(" (Alpha: {0:0.##})", entry.EffectiveAlpha);
                    }

                    sb.AppendLine();
                }

                if (healthyRun > 0)
                {
                    sb.AppendFormat("  [+ {0} healthy element{1}]\n", healthyRun, healthyRun > 1 ? "s" : "");
                }
            }
            else // ExportMode.Full
            {
                sb.AppendLine("## Hierarchy & Draw Order");
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    int depth = GetHierarchyDepth(entry.Transform, rootTransform);
                    string typeName = GetElementTypeName(entry.Transform);
                    string indent = GetHierarchyIndent(depth);
                    string flagsStr = FormatFlags(entry.Flags);

                    sb.AppendFormat("{0}[#{1:D2}] {2} ({3})",
                        indent,
                        entry.GlobalDrawIndex,
                        entry.Name,
                        typeName);

                    if (!string.IsNullOrEmpty(flagsStr))
                    {
                        sb.Append(" ").Append(flagsStr);
                    }

                    if (entry.RaycastTarget && entry.EffectiveAlpha < 1f)
                    {
                        sb.AppendFormat(" (Alpha: {0:0.##})", entry.EffectiveAlpha);
                    }

                    sb.AppendLine();
                }
            }

            var anomalies = new List<(string Flag, string Name, int DrawIndex, string Reason, string Fix)>();
            for (int i = 0; i < entries.Count; i++)
            {
                CollectAnomalies(entries[i], anomalies);
            }

            sb.AppendLine("\n## Critical Diagnostic Anomalies & Fixes");
            if (anomalies.Count == 0)
            {
                sb.AppendLine("No critical raycast blocking or hierarchy anomalies detected. UI layout is healthy.");
            }
            else
            {
                for (int j = 0; j < anomalies.Count; j++)
                {
                    var a = anomalies[j];
                    sb.AppendFormat("{0}. **[{1}]** `{2}` (DrawIndex: {3})\n", j + 1, a.Flag, a.Name, a.DrawIndex);
                    sb.AppendFormat("   - **Reason**: {0}\n", a.Reason);
                    sb.AppendFormat("   - **Fix**: {0}\n", a.Fix);
                }
            }

            return sb.ToString();
        }

        public static string ExportToJson(IReadOnlyList<UIElementEntry> entries, Canvas rootCanvas, bool prettyPrint)
        {
            return ExportToJson(entries, rootCanvas, ExportMode.Full, prettyPrint);
        }

        public static string ExportToJson(IReadOnlyList<UIElementEntry> entries, Canvas rootCanvas, ExportMode mode = ExportMode.Full, bool prettyPrint = true)
        {
            var data = new ExportCanvasInfo
            {
                canvasName = rootCanvas != null ? rootCanvas.name : "Canvas",
                renderMode = rootCanvas != null ? rootCanvas.renderMode.ToString() : "Unknown",
                sortingOrder = rootCanvas != null ? rootCanvas.sortingOrder : 0,
                totalElements = entries != null ? entries.Count : 0,
                exportMode = mode.ToString()
            };

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    bool isAnomaly = HasAnomaly(entry);

                    if (mode == ExportMode.Full || mode == ExportMode.Compact || (mode == ExportMode.Anomalies && isAnomaly))
                    {
                        data.elements.Add(new ExportElementInfo
                        {
                            drawIndex = entry.GlobalDrawIndex,
                            name = entry.Name,
                            hierarchyPath = GetFullPath(entry.Transform),
                            type = GetElementTypeName(entry.Transform),
                            x = entry.WorldRect.x,
                            y = entry.WorldRect.y,
                            width = entry.WorldRect.width,
                            height = entry.WorldRect.height,
                            effectiveAlpha = entry.EffectiveAlpha,
                            raycastTarget = entry.RaycastTarget,
                            blocksRaycasts = entry.BlocksRaycasts,
                            flags = entry.Flags.ToString()
                        });
                    }

                    string targetPath = entry.Transform != null ? GetFullPath(entry.Transform) : entry.Name;
                    if (string.IsNullOrEmpty(targetPath)) targetPath = entry.Name;

                    if ((entry.Flags & DiagnosticFlags.GhostBlocker) != 0)
                    {
                        data.anomalies.Add(new ExportAnomalyInfo
                        {
                            severity = "Warning",
                            flag = "GHOST_BLOCKER",
                            elementName = entry.Name,
                            drawIndex = entry.GlobalDrawIndex,
                            description = string.Format("Graphic has raycastTarget enabled but effective alpha is {0:0.##} or sprite is null.", entry.EffectiveAlpha),
                            suggestedFix = string.Format("Disable Raycast Target on '{0}'.", entry.Name),
                            fixAction = new ExportFixAction
                            {
                                targetPath = targetPath,
                                component = GetGraphicComponentName(entry.Transform),
                                property = "raycastTarget",
                                value = false
                            }
                        });
                    }

                    if ((entry.Flags & DiagnosticFlags.ZeroSize) != 0)
                    {
                        data.anomalies.Add(new ExportAnomalyInfo
                        {
                            severity = "Warning",
                            flag = "ZERO_SIZE",
                            elementName = entry.Name,
                            drawIndex = entry.GlobalDrawIndex,
                            description = "Element has zero width/height with active layout.",
                            suggestedFix = string.Format("Adjust RectTransform dimensions or disable raycastTarget on '{0}'.", entry.Name),
                            fixAction = new ExportFixAction
                            {
                                targetPath = targetPath,
                                component = "RectTransform",
                                property = "raycastTarget",
                                value = false
                            }
                        });
                    }

                    if ((entry.Flags & DiagnosticFlags.GroupTransparent) != 0)
                    {
                        data.anomalies.Add(new ExportAnomalyInfo
                        {
                            severity = "Warning",
                            flag = "GROUP_TRANSPARENT_BLOCKER",
                            elementName = entry.Name,
                            drawIndex = entry.GlobalDrawIndex,
                            description = "CanvasGroup alpha is 0 while blocksRaycasts is true.",
                            suggestedFix = string.Format("Disable blocksRaycasts on CanvasGroup for '{0}'.", entry.Name),
                            fixAction = new ExportFixAction
                            {
                                targetPath = targetPath,
                                component = "CanvasGroup",
                                property = "blocksRaycasts",
                                value = false
                            }
                        });
                    }

                    if ((entry.Flags & DiagnosticFlags.NestedLabelRaycast) != 0)
                    {
                        data.anomalies.Add(new ExportAnomalyInfo
                        {
                            severity = "Warning",
                            flag = "NESTED_LABEL_RAYCAST",
                            elementName = entry.Name,
                            drawIndex = entry.GlobalDrawIndex,
                            description = string.Format("Child Graphic '{0}' under Button has raycastTarget=true.", entry.Name),
                            suggestedFix = string.Format("Disable Raycast Target on '{0}'.", entry.Name),
                            fixAction = new ExportFixAction
                            {
                                targetPath = targetPath,
                                component = GetGraphicComponentName(entry.Transform),
                                property = "raycastTarget",
                                value = false
                            }
                        });
                    }

                    if (entry.OcclusionPairs != null && entry.OcclusionPairs.Count > 0)
                    {
                        for (int p = 0; p < entry.OcclusionPairs.Count; p++)
                        {
                            var pair = entry.OcclusionPairs[p];
                            if (pair.BlockerName == entry.Name || (entry.Flags & DiagnosticFlags.OcclusionBlocker) != 0)
                            {
                                data.anomalies.Add(new ExportAnomalyInfo
                                {
                                    severity = "Critical",
                                    flag = "SPATIAL_BLOCK",
                                    elementName = entry.Name,
                                    drawIndex = entry.GlobalDrawIndex,
                                    description = string.Format("[SPATIAL BLOCK] '{0}' intercepts {1:0.#}% of Button '{2}' at Rect({3:0.#}, {4:0.#}, {5:0.#}, {6:0.#}) -> Fix: Disable Raycast Target or lower Canvas sorting order.",
                                        pair.BlockerName, pair.OverlapPercentage, pair.TargetName, pair.OverlapRect.x, pair.OverlapRect.y, pair.OverlapRect.width, pair.OverlapRect.height),
                                    suggestedFix = "Disable Raycast Target or lower Canvas sorting order.",
                                    fixAction = new ExportFixAction
                                    {
                                        targetPath = targetPath,
                                        component = GetGraphicComponentName(entry.Transform),
                                        property = "raycastTarget",
                                        value = false
                                    }
                                });
                            }
                        }
                    }
                }
            }

            return JsonUtility.ToJson(data, prettyPrint);
        }

        public static bool HasAnomaly(in UIElementEntry entry)
        {
            var anomalyFlags = DiagnosticFlags.GhostBlocker |
                               DiagnosticFlags.ZeroSize |
                               DiagnosticFlags.GroupTransparent |
                               DiagnosticFlags.OcclusionBlocker |
                               DiagnosticFlags.NestedLabelRaycast;

            if ((entry.Flags & anomalyFlags) != 0)
                return true;

            if (entry.OcclusionPairs != null && entry.OcclusionPairs.Count > 0)
            {
                for (int i = 0; i < entry.OcclusionPairs.Count; i++)
                {
                    if (entry.OcclusionPairs[i].BlockerName == entry.Name)
                        return true;
                }
            }


            return false;
        }

        public static int GetHierarchyDepth(Transform t, Transform root)
        {
            if (t == null) return 0;
            int depth = 0;
            var curr = t.parent;
            while (curr != null && curr != root)
            {
                depth++;
                curr = curr.parent;
            }
            return depth;
        }

        public static string GetElementTypeName(Transform t)
        {
            if (t == null) return "Transform";
            if (t.TryGetComponent<UnityEngine.UI.Button>(out _)) return "Button";
            if (t.TryGetComponent<UnityEngine.UI.Image>(out _)) return "Image";
            if (t.TryGetComponent<UnityEngine.UI.RawImage>(out _)) return "RawImage";
            if (t.TryGetComponent<UnityEngine.UI.Text>(out _)) return "Text";
            if (t.TryGetComponent<Canvas>(out _)) return "Canvas";
            if (t.TryGetComponent<UnityEngine.UI.Graphic>(out var graphic)) return graphic.GetType().Name;
            return "RectTransform";
        }

        public static string GetGraphicComponentName(Transform t)
        {
            if (t == null) return "Image";
            if (t.TryGetComponent<UnityEngine.UI.Image>(out _)) return "Image";
            if (t.TryGetComponent<UnityEngine.UI.RawImage>(out _)) return "RawImage";
            if (t.TryGetComponent<UnityEngine.UI.Text>(out _)) return "Text";
            if (t.TryGetComponent<UnityEngine.UI.Graphic>(out var graphic)) return graphic.GetType().Name;
            return "Image";
        }

        public static string GetHierarchyIndent(int depth)
        {
            if (depth <= 0) return "- ";
            var sb = new StringBuilder();
            for (int i = 0; i < depth - 1; i++)
            {
                sb.Append("  ");
            }
            sb.Append("  └── ");
            return sb.ToString();
        }

        public static string FormatFlags(DiagnosticFlags flags)
        {
            if (flags == DiagnosticFlags.None) return "";

            var parts = new List<string>(4);
            if ((flags & DiagnosticFlags.GhostBlocker) != 0) parts.Add("[GHOST_BLOCKER]");
            else if ((flags & DiagnosticFlags.RaycastBlocker) != 0) parts.Add("[RAYCAST]");

            if ((flags & DiagnosticFlags.OcclusionBlocker) != 0) parts.Add("[OCCLUSION_BLOCKER]");
            if ((flags & DiagnosticFlags.NestedLabelRaycast) != 0) parts.Add("[NESTED_LABEL_RAYCAST]");
            if ((flags & DiagnosticFlags.HasMask) != 0 || (flags & DiagnosticFlags.HasRectMask2D) != 0) parts.Add("[MASK]");
            if ((flags & DiagnosticFlags.ZeroSize) != 0) parts.Add("[ZERO_SIZE]");
            if ((flags & DiagnosticFlags.GroupBlocked) != 0) parts.Add("[GROUP_BLOCKED]");
            if ((flags & DiagnosticFlags.GroupTransparent) != 0) parts.Add("[GROUP_TRANSPARENT]");
            if ((flags & DiagnosticFlags.Inactive) != 0) parts.Add("[INACTIVE]");

            return string.Join(" ", parts);
        }

        public static void CollectAnomalies(UIElementEntry entry, List<(string Flag, string Name, int DrawIndex, string Reason, string Fix)> anomalies)
        {
            if (anomalies == null) return;

            if ((entry.Flags & DiagnosticFlags.GhostBlocker) != 0)
            {
                anomalies.Add((
                    "GHOST_BLOCKER",
                    entry.Name,
                    entry.GlobalDrawIndex,
                    string.Format("Element has raycastTarget=true but effective alpha is {0:0.##} or missing sprite. It invisibly blocks clicks to underlying buttons.", entry.EffectiveAlpha),
                    string.Format("Uncheck 'Raycast Target' on Graphic component '{0}' or disable GameObject.", entry.Name)
                ));
            }

            if ((entry.Flags & DiagnosticFlags.ZeroSize) != 0)
            {
                anomalies.Add((
                    "ZERO_SIZE",
                    entry.Name,
                    entry.GlobalDrawIndex,
                    "RectTransform size is 0x0. If raycastTarget is enabled, touches cannot register or layout is collapsed.",
                    string.Format("Set valid width/height in RectTransform on '{0}' or disable raycastTarget.", entry.Name)
                ));
            }

            if ((entry.Flags & DiagnosticFlags.GroupTransparent) != 0)
            {
                anomalies.Add((
                    "GROUP_TRANSPARENT_BLOCKER",
                    entry.Name,
                    entry.GlobalDrawIndex,
                    "Parent CanvasGroup has Alpha=0 but Blocks Raycasts is enabled. This invisibly blocks touches.",
                    string.Format("Uncheck 'Blocks Raycasts' on CanvasGroup on/above '{0}'.", entry.Name)
                ));
            }

            if ((entry.Flags & DiagnosticFlags.NestedLabelRaycast) != 0)
            {
                anomalies.Add((
                    "NESTED_LABEL_RAYCAST",
                    entry.Name,
                    entry.GlobalDrawIndex,
                    string.Format("Child Graphic '{0}' under Button has raycastTarget=true, adding unnecessary raycast query overhead.", entry.Name),
                    string.Format("Uncheck 'Raycast Target' on '{0}'.", entry.Name)
                ));
            }

            if (entry.OcclusionPairs != null && entry.OcclusionPairs.Count > 0)
            {
                for (int i = 0; i < entry.OcclusionPairs.Count; i++)
                {
                    var pair = entry.OcclusionPairs[i];
                    if (pair.BlockerName == entry.Name || (entry.Flags & DiagnosticFlags.OcclusionBlocker) != 0)
                    {
                        anomalies.Add((
                            "SPATIAL_BLOCK",
                            entry.Name,
                            entry.GlobalDrawIndex,
                            string.Format("[SPATIAL BLOCK] '{0}' intercepts {1:0.#}% of Button '{2}' at Rect({3:0.#}, {4:0.#}, {5:0.#}, {6:0.#}) -> Fix: Disable Raycast Target or lower Canvas sorting order.",
                                pair.BlockerName, pair.OverlapPercentage, pair.TargetName, pair.OverlapRect.x, pair.OverlapRect.y, pair.OverlapRect.width, pair.OverlapRect.height),
                            "Disable Raycast Target or lower Canvas sorting order."
                        ));
                    }
                }
            }
        }

        public static string GetFullPath(Transform t)
        {
            if (t == null) return "";
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
