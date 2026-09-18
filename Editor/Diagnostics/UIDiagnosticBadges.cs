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

            if ((flags & DiagnosticFlags.Inactive) != 0)
                dot.AddToClassList("stack-dot--inactive");
            else if ((flags & DiagnosticFlags.GhostBlocker) != 0)
                dot.AddToClassList("stack-dot--ghost");
            else if ((flags & DiagnosticFlags.RaycastBlocker) != 0)
                dot.AddToClassList("stack-dot--raycast");
            else
                dot.AddToClassList("stack-dot--passive");

            return dot;
        }

        public static Label CreateWarningBadge()
        {
            return new Label("⚠ Invisible Hitbox") { pickingMode = PickingMode.Ignore };
        }
    }
}
