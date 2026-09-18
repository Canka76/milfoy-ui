using System.Collections.Generic;
using UnityEditor;

namespace UIDepthInspector.Editor.Core
{
    public class UIRenderTreeCache
    {
        List<UIElementEntry> _entries = new();
        int _generation;
        bool _dirty = true;

        public IReadOnlyList<UIElementEntry> Entries
        {
            get
            {
                if (_dirty)
                    Rebuild();
                return _entries;
            }
        }

        public int Generation => _generation;

        public UIRenderTreeCache()
        {
            EditorApplication.hierarchyChanged += Invalidate;
            Undo.undoRedoPerformed += Invalidate;
        }

        public void Dispose()
        {
            EditorApplication.hierarchyChanged -= Invalidate;
            Undo.undoRedoPerformed -= Invalidate;
        }

        public void Invalidate()
        {
            _dirty = true;
        }

        void Rebuild()
        {
            _entries = UIRenderTreeCollector.Collect();
            _generation++;
            _dirty = false;
        }
    }
}
