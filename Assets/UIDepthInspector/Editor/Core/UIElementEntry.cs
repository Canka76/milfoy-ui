using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIDepthInspector.Editor.Core
{
    [Flags]
    public enum DiagnosticFlags
    {
        None               = 0,
        RaycastBlocker     = 1 << 0,
        GhostBlocker       = 1 << 1,
        PassiveVisual      = 1 << 2,
        Inactive           = 1 << 3,
        GroupBlocked       = 1 << 4,
        GroupTransparent   = 1 << 5,
        HasMask            = 1 << 6,
        HasRectMask2D      = 1 << 7,
        ZeroSize           = 1 << 8,
        OcclusionBlocker   = 1 << 9,
        NestedLabelRaycast = 1 << 10,
    }

    [Serializable]
    public struct OcclusionPair
    {
        public string BlockerName;
        public string TargetName;
        public Rect OverlapRect;
        public float OverlapPercentage;

        public OcclusionPair(string blockerName, string targetName, Rect overlapRect, float overlapPercentage)
        {
            BlockerName = blockerName;
            TargetName = targetName;
            OverlapRect = overlapRect;
            OverlapPercentage = overlapPercentage;
        }
    }

    public struct UIElementEntry
    {
        public int GlobalDrawIndex;
        public int InstanceId;
        public string Name;
        public bool IsActive;
        public bool RaycastTarget;
        public float EffectiveAlpha;
        public bool BlocksRaycasts;
        public Rect WorldRect;
        public RenderMode CanvasRenderMode;
        public int RootCanvasId;
        public string RootCanvasName;
        public DiagnosticFlags Flags;
        public Transform Transform;
        public Color? CustomColor;
        public List<OcclusionPair> OcclusionPairs;
    }
}
