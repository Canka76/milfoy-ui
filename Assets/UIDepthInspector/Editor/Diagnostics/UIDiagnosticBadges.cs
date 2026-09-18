using UnityEngine.UIElements;

namespace UIDepthInspector.Editor.Diagnostics
{
    using Core;

    public static class UIDiagnosticBadges
    {
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
