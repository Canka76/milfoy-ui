using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIDepthInspector.Editor.Viewport
{
    using Core;

    public enum ViewPreset
    {
        Front,
        Isometric,
        Side,
    }

    public class UIPreview3DViewport : IDisposable
    {
        PreviewRenderUtility _previewUtility;
        float _explosionFactor;
        float _slabThickness = 0.12f;
        int _highlightIndex = -1;
        readonly List<GameObject> _previewObjects = new();
        readonly List<Collider> _colliders = new();
        List<UIElementEntry> _currentEntries;
        Rect _normalizationRect;

        public void Initialize()
        {
            _previewUtility = new PreviewRenderUtility();
            _previewUtility.camera.fieldOfView = 30f;
            _previewUtility.camera.nearClipPlane = 0.01f;
            _previewUtility.camera.farClipPlane = 500f;
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f); // Studio dark #1f1f1f

            var shader = Shader.Find("Hidden/UIDepthInspector/QuadDiagnostic");

            _matRaycast = new Material(shader) { color = new Color(0.878f, 0.376f, 0.376f, 0.7f) };
            _matRaycast.SetFloat("_DiagnosticMode", 0f);

            _matPassive = new Material(shader) { color = new Color(0.376f, 0.627f, 0.878f, 0.5f) };
            _matPassive.SetFloat("_DiagnosticMode", 0f);

            _matInactive = new Material(shader) { color = new Color(0.5f, 0.5f, 0.5f, 0.3f) };
            _matInactive.SetFloat("_DiagnosticMode", 0f);

            _matGhost = new Material(shader) { color = new Color(1f, 0.75f, 0.2f, 0.85f) };
            _matGhost.SetFloat("_DiagnosticMode", 1f);
        }

        public void Dispose()
        {
            ClearPreviewObjects();

            if (_matRaycast != null) UnityEngine.Object.DestroyImmediate(_matRaycast);
            if (_matPassive != null) UnityEngine.Object.DestroyImmediate(_matPassive);
            if (_matInactive != null) UnityEngine.Object.DestroyImmediate(_matInactive);
            if (_matGhost != null) UnityEngine.Object.DestroyImmediate(_matGhost);

            _previewUtility?.Cleanup();
            _previewUtility = null;
        }

        public void RebuildFromEntries(List<UIElementEntry> entries)
        {
            _currentEntries = entries;
            ClearPreviewObjects();

            if (entries == null || entries.Count == 0) return;

            ComputeNormalizationRect(entries);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var go = CreateQuad(entry, i, entries.Count);
                _previewObjects.Add(go);
            }
        }

        public void SetExplosionFactor(float factor)
        {
            _explosionFactor = factor;
            RepositionQuads();
        }

        public void SetSlabThickness(float thickness)
        {
            _slabThickness = Mathf.Max(0.01f, thickness);
            RepositionQuads();
        }
        public void SetViewPreset(ViewPreset preset)
        {
            _pivotOffset = Vector3.zero;

            switch (preset)
            {
                case ViewPreset.Front:
                    _orbitAngles = Vector2.zero;
                    _zoomDistance = 10f;
                    _orthographic = true;
                    break;
                case ViewPreset.Isometric:
                    _orbitAngles = new Vector2(30f, 45f);
                    _zoomDistance = 12f;
                    _orthographic = false;
                    break;
                case ViewPreset.Side:
                    _orbitAngles = new Vector2(0f, 90f);
                    _zoomDistance = 12f;
                    _orthographic = false;
                    break;
            }
        }

        public void HighlightEntry(int globalDrawIndex)
        {
            _highlightIndex = globalDrawIndex;
        }

        public void FrameEntry(int globalDrawIndex)
        {
            if (globalDrawIndex < 0 || globalDrawIndex >= _previewObjects.Count) return;
            var go = _previewObjects[globalDrawIndex];
            if (go == null) return;

            _pivotOffset = go.transform.localPosition;
            _zoomDistance = 8f;
        }

        public int GetPickedEntryIndex(Vector2 mousePos, Rect viewportRect)
        {
            if (_previewUtility == null || _currentEntries == null || _currentEntries.Count == 0)
                return -1;

            // Convert mouse position to viewport-local coordinates
            var localMouse = mousePos - viewportRect.position;
            localMouse.y = viewportRect.height - localMouse.y; // Flip Y for camera

            var cam = _previewUtility.camera;
            var ray = cam.ScreenPointToRay(new Vector3(localMouse.x, localMouse.y, 0));

            float closestDist = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < _colliders.Count; i++)
            {
                if (_colliders[i] != null && _colliders[i].Raycast(ray, out var hit, 500f))
                {
                    if (hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        closestIndex = i;
                    }
                }
            }

            return closestIndex;
        }

        public void HandleInput(Event evt, Rect viewportRect)
        {
            if (evt == null) return;

            var mouseInRect = viewportRect.Contains(evt.mousePosition);
            if (!mouseInRect) return;

            switch (evt.type)
            {
                case EventType.MouseDrag when evt.button == 1: // Right-click orbit
                    _orbitAngles.x += evt.delta.y * 0.3f;
                    _orbitAngles.y += evt.delta.x * 0.3f;
                    evt.Use();
                    break;

                case EventType.MouseDrag when evt.button == 2: // Middle-click pan
                case EventType.MouseDrag when evt.button == 0 && evt.alt: // Alt+Left pan
                    float panSpeed = _zoomDistance * 0.002f;
                    var cam = _previewUtility.camera;
                    _pivotOffset -= cam.transform.right * evt.delta.x * panSpeed;
                    _pivotOffset += cam.transform.up * evt.delta.y * panSpeed;
                    evt.Use();
                    break;

                case EventType.ScrollWheel: // Zoom
                    _zoomDistance *= 1f + evt.delta.y * 0.05f;
                    _zoomDistance = Mathf.Clamp(_zoomDistance, 1f, 100f);
                    evt.Use();
                    break;
            }
        }

        public void OnGUI(Rect rect)
        {
            if (_previewUtility == null) return;

            // PreviewRenderUtility.BeginPreview allocates a RenderTexture and MUST only run during Repaint events
            // with positive, non-zero dimensions.
            if (Event.current.type != EventType.Repaint) return;

            int width = Mathf.Max(1, (int)rect.width);
            int height = Mathf.Max(1, (int)rect.height);
            if (width < 2 || height < 2) return;

            // Use normalized origin (0, 0, width, height) for offscreen preview render target
            var previewRect = new Rect(0, 0, width, height);

            _previewUtility.BeginPreview(previewRect, GUIStyle.none);

            UpdateCamera(previewRect);
            _previewUtility.camera.Render();

            // Draw high-contrast hitbox border rings around active blockers
            if (_currentEntries != null)
            {
                for (int i = 0; i < _previewObjects.Count && i < _currentEntries.Count; i++)
                {
                    var entry = _currentEntries[i];
                    var go = _previewObjects[i];
                    if (go == null) continue;

                    var mf = go.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) continue;

                    bool isGhost = (entry.Flags & DiagnosticFlags.GhostBlocker) != 0;
                    bool isRaycast = (entry.Flags & DiagnosticFlags.RaycastBlocker) != 0;
                    bool hasMask = (entry.Flags & (DiagnosticFlags.HasMask | DiagnosticFlags.HasRectMask2D)) != 0;

                    Handles.matrix = go.transform.localToWorldMatrix;

                    if (hasMask)
                    {
                        Handles.color = new Color(0.25f, 0.88f, 0.25f, 0.85f); // Bright green for masks
                        Handles.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size * 1.02f);
                    }
                    else if (isGhost)
                    {
                        Handles.color = new Color(1f, 0.78f, 0.2f, 0.95f); // High-contrast amber border
                        Handles.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size * 1.01f);
                    }
                    else if (isRaycast)
                    {
                        Handles.color = new Color(1f, 0.42f, 0.42f, 0.6f); // Soft coral border for active touch targets
                        Handles.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size * 1.005f);
                    }
                }
                Handles.matrix = Matrix4x4.identity;
            }

            // Draw selection highlight with glowing yellow volume cage
            if (_highlightIndex >= 0 && _highlightIndex < _previewObjects.Count)
            {
                var go = _previewObjects[_highlightIndex];
                if (go != null)
                {
                    var mf = go.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                        DrawWireframeCage(go.transform, mf.sharedMesh.bounds);
                }
            }
            var result = _previewUtility.EndPreview();
            if (result != null)
            {
                GUI.DrawTexture(rect, result, ScaleMode.StretchToFill, false);
            }
        }

        void UpdateCamera(Rect viewportRect)
        {
            var cam = _previewUtility.camera;
            cam.orthographic = _orthographic;

            var rotation = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0);
            var position = _pivotOffset + rotation * new Vector3(0, 0, -_zoomDistance);

            cam.transform.position = position;
            cam.transform.rotation = rotation;

            if (_orthographic)
                cam.orthographicSize = _zoomDistance * 0.5f;

            cam.aspect = viewportRect.width / viewportRect.height;
        }

        void ComputeNormalizationRect(List<UIElementEntry> entries)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var e in entries)
            {
                if (e.WorldRect.width < 0.01f && e.WorldRect.height < 0.01f) continue;
                minX = Mathf.Min(minX, e.WorldRect.xMin);
                minY = Mathf.Min(minY, e.WorldRect.yMin);
                maxX = Mathf.Max(maxX, e.WorldRect.xMax);
                maxY = Mathf.Max(maxY, e.WorldRect.yMax);
            }

            float w = maxX - minX;
            float h = maxY - minY;
            float size = Mathf.Max(w, h, 1f);
            _normalizationRect = new Rect(minX, minY, size, size);
        }

        GameObject CreateQuad(UIElementEntry entry, int index, int totalCount)
        {
            // Create 3D Cube primitive so the UI card has physical volume and visible side walls
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.hideFlags = HideFlags.HideAndDontSave;

            _previewUtility.AddSingleGO(go);

            // Assign material
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = GetMaterialForFlags(entry.Flags);

            // Set position and scale in XYZ
            PositionQuad(go.transform, entry, index, totalCount);

            // Setup collider for accurate front and side-wall 3D picking
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                _colliders.Add(col);
            }

            return go;
        }

        void PositionQuad(Transform t, UIElementEntry entry, int index, int totalCount)
        {
            float normX = (entry.WorldRect.center.x - _normalizationRect.x) / _normalizationRect.width * 8f - 4f;
            float normY = (entry.WorldRect.center.y - _normalizationRect.y) / _normalizationRect.height * 8f - 4f;
            float z = totalCount > 0 ? index * (_explosionFactor / Mathf.Max(totalCount, 1)) : 0f;

            t.localPosition = new Vector3(normX, normY, z);

            float scaleX = Mathf.Max(entry.WorldRect.width / _normalizationRect.width * 8f, 0.05f);
            float scaleY = Mathf.Max(entry.WorldRect.height / _normalizationRect.height * 8f, 0.05f);
            float scaleZ = Mathf.Max(_slabThickness, 0.01f);

            t.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
        void RepositionQuads()
        {
            if (_currentEntries == null) return;
            for (int i = 0; i < _previewObjects.Count && i < _currentEntries.Count; i++)
            {
                if (_previewObjects[i] != null)
                    PositionQuad(_previewObjects[i].transform, _currentEntries[i], i, _currentEntries.Count);
            }
        }

        Material GetMaterialForFlags(DiagnosticFlags flags)
        {
            if ((flags & DiagnosticFlags.Inactive) != 0) return _matInactive;
            if ((flags & DiagnosticFlags.GhostBlocker) != 0) return _matGhost;
            if ((flags & DiagnosticFlags.RaycastBlocker) != 0) return _matRaycast;
            return _matPassive;
        }

        void DrawWireframeCage(Transform t, Bounds localBounds)
        {
            Handles.color = Color.yellow;
            var matrix = t.localToWorldMatrix;
            Handles.matrix = matrix;
            Handles.DrawWireCube(localBounds.center, localBounds.size * 1.02f);
            Handles.matrix = Matrix4x4.identity;
        }

        void ClearPreviewObjects()
        {
            foreach (var go in _previewObjects)
            {
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            }
            _previewObjects.Clear();
            _colliders.Clear();
        }
    }
}
