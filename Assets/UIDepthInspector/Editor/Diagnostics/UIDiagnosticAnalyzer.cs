using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIDepthInspector.Editor.Diagnostics
{
    using Core;

    public static class UIDiagnosticAnalyzer
    {
        public static void Analyze(List<UIElementEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            // Pass 1: Compute per-element diagnostic flags
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Flags = ComputeFlags(entry);
                entries[i] = entry;
            }

            // Pass 2: Spatial occlusion analysis between interactive elements and upper blockers
            DetectSpatialOcclusions(entries);
        }

        static DiagnosticFlags ComputeFlags(UIElementEntry entry)
        {
            var flags = DiagnosticFlags.None;

            if (!entry.IsActive)
            {
                flags |= DiagnosticFlags.Inactive;
                return flags;
            }

            // Zero-size check
            if (entry.WorldRect.width < 0.01f || entry.WorldRect.height < 0.01f)
                flags |= DiagnosticFlags.ZeroSize;

            // CanvasGroup overrides
            if (!entry.BlocksRaycasts)
            {
                flags |= DiagnosticFlags.GroupBlocked;
                flags |= DiagnosticFlags.PassiveVisual;
                return flags;
            }

            // Ghost blocker: raycastTarget ON but effectively invisible
            if (entry.RaycastTarget)
            {
                bool isGhost = false;

                // Alpha zero (direct or via CanvasGroup)
                if (entry.EffectiveAlpha <= 0f)
                    isGhost = true;

                // Missing sprite on an Image component
                if (!isGhost && entry.Transform != null &&
                    entry.Transform.TryGetComponent<Image>(out var img) && img.sprite == null)
                    isGhost = true;

                // CanvasGroup making it transparent while still blocking
                if (entry.EffectiveAlpha <= 0f && entry.BlocksRaycasts)
                    flags |= DiagnosticFlags.GroupTransparent;

                if (isGhost)
                    flags |= DiagnosticFlags.GhostBlocker;
                else
                    flags |= DiagnosticFlags.RaycastBlocker;

                // Check for nested raycast target under button
                if (entry.Transform != null && entry.Transform.parent != null)
                {
                    var curr = entry.Transform.parent;
                    while (curr != null)
                    {
                        if (curr.TryGetComponent<Button>(out _))
                        {
                            flags |= DiagnosticFlags.NestedLabelRaycast;
                            break;
                        }
                        curr = curr.parent;
                    }
                }
            }
            else
            {
                flags |= DiagnosticFlags.PassiveVisual;
            }

            // Mask components
            if (entry.Transform != null)
            {
                if (entry.Transform.TryGetComponent<Mask>(out _))
                    flags |= DiagnosticFlags.HasMask;
                if (entry.Transform.TryGetComponent<RectMask2D>(out _))
                    flags |= DiagnosticFlags.HasRectMask2D;
            }

            return flags;
        }

        static void DetectSpatialOcclusions(List<UIElementEntry> entries)
        {
            if (entries.Count < 2)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                var lower = entries[i];
                if (!IsInteractiveElement(lower))
                    continue;

                var lowerRect = lower.WorldRect;
                if (lowerRect.width < 0.01f || lowerRect.height < 0.01f)
                    continue;

                float lowerArea = lowerRect.width * lowerRect.height;
                if (lowerArea < 0.0001f)
                    continue;

                for (int j = i + 1; j < entries.Count; j++)
                {
                    var upper = entries[j];

                    if (!upper.IsActive || !upper.BlocksRaycasts || !upper.RaycastTarget || upper.EffectiveAlpha <= 0f)
                        continue;

                    // Skip if upper is child/descendant of lower interactive element (e.g. child icon or label of button)
                    if (upper.Transform != null && lower.Transform != null && upper.Transform.IsChildOf(lower.Transform))
                        continue;

                    var upperRect = upper.WorldRect;
                    if (upperRect.width < 0.01f || upperRect.height < 0.01f)
                        continue;

                    float xMin = Mathf.Max(lowerRect.xMin, upperRect.xMin);
                    float xMax = Mathf.Min(lowerRect.xMax, upperRect.xMax);
                    float yMin = Mathf.Max(lowerRect.yMin, upperRect.yMin);
                    float yMax = Mathf.Min(lowerRect.yMax, upperRect.yMax);

                    if (xMax > xMin && yMax > yMin)
                    {
                        float overlapWidth = xMax - xMin;
                        float overlapHeight = yMax - yMin;
                        Rect overlapRect = new Rect(xMin, yMin, overlapWidth, overlapHeight);
                        float overlapArea = overlapWidth * overlapHeight;
                        float overlapPercentage = Mathf.Clamp((overlapArea / lowerArea) * 100f, 0f, 100f);

                        string blockerName = !string.IsNullOrEmpty(upper.Name) ? upper.Name : (upper.Transform != null ? upper.Transform.name : "Unknown");
                        string targetName = !string.IsNullOrEmpty(lower.Name) ? lower.Name : (lower.Transform != null ? lower.Transform.name : "Unknown");

                        var pair = new OcclusionPair(blockerName, targetName, overlapRect, overlapPercentage);

                        upper.Flags |= DiagnosticFlags.OcclusionBlocker;

                        if (upper.OcclusionPairs == null) upper.OcclusionPairs = new List<OcclusionPair>();
                        upper.OcclusionPairs.Add(pair);

                        if (lower.OcclusionPairs == null) lower.OcclusionPairs = new List<OcclusionPair>();
                        lower.OcclusionPairs.Add(pair);
                        entries[j] = upper;
                        entries[i] = lower;
                    }
                }
            }
        }

        static bool IsInteractiveElement(in UIElementEntry entry)
        {
            if (!entry.IsActive)
                return false;

            if (entry.Transform != null)
            {
                if (entry.Transform.TryGetComponent<Selectable>(out _))
                    return true;
                if (entry.Transform.TryGetComponent<ScrollRect>(out _))
                    return true;

                var curr = entry.Transform.parent;
                while (curr != null)
                {
                    if (curr.TryGetComponent<Button>(out _))
                        return true;
                    curr = curr.parent;
                }
            }

            if (!string.IsNullOrEmpty(entry.Name))
            {
                if (entry.Name.IndexOf("button", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    entry.Name.IndexOf("btn", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
