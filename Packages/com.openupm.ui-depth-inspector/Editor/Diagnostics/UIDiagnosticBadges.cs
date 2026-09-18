using UnityEngine;
using UnityEngine.UIElements;

namespace UIDepthInspector.Editor.Diagnostics
{
    using Core;

    public static class UIDiagnosticBadges
    {
        public static Color GetBadgeColor(DiagnosticFlags flags)
        {
            if ((flags & DiagnosticFlags.Inactive) != 0)
                return new Color(0.502f, 0.502f, 0.502f, 1f); // #808080
            if ((flags & DiagnosticFlags.GhostBlocker) != 0)
                return new Color(1f, 0.690f, 0.188f, 1f); // #FFB030
            if ((flags & DiagnosticFlags.RaycastBlocker) != 0)
                return new Color(0.878f, 0.376f, 0.376f, 1f); // #E06060
            return new Color(0.376f, 0.627f, 0.878f, 1f); // #60A0E0 (Passive)
        }

        public static VisualElement CreateDot(DiagnosticFlags flags)
        {
            var dot = new VisualElement();
            dot.AddToClassList("stack-dot");
            UpdateDot(dot, flags);
            return dot;
        }

        public static void UpdateDot(VisualElement dot, DiagnosticFlags flags)
        {
            dot.EnableInClassList("stack-dot--inactive", (flags & DiagnosticFlags.Inactive) != 0);
            dot.EnableInClassList("stack-dot--ghost", (flags & DiagnosticFlags.Inactive) == 0 && (flags & DiagnosticFlags.GhostBlocker) != 0);
            dot.EnableInClassList("stack-dot--raycast", (flags & DiagnosticFlags.Inactive) == 0 && (flags & DiagnosticFlags.GhostBlocker) == 0 && (flags & DiagnosticFlags.RaycastBlocker) != 0);
            dot.EnableInClassList("stack-dot--passive", (flags & DiagnosticFlags.Inactive) == 0 && (flags & DiagnosticFlags.GhostBlocker) == 0 && (flags & DiagnosticFlags.RaycastBlocker) == 0);
        }

        public static Label CreateWarningBadge()
        {
            return new Label("⚠ Invisible Hitbox") { pickingMode = PickingMode.Ignore };
        }
    }
}
