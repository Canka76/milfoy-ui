using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIDepthInspector.Editor.Core
{
    public static class UICustomColorRegistry
    {
        const string SessionKey = "UIDepthInspector_CustomColors";

        [Serializable]
        public class ColorContainer
        {
            public List<ColorEntry> entries = new List<ColorEntry>();
        }

        [Serializable]
        public class ColorEntry
        {
            public int id;
            public float r, g, b, a;
        }

        static readonly Dictionary<int, Color> s_Colors = new Dictionary<int, Color>();
        static readonly object s_Lock = new object();
        static bool s_Loaded = false;

        static void EnsureLoaded()
        {
            if (s_Loaded) return;
            s_Colors.Clear();
            string json = SessionState.GetString(SessionKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var container = JsonUtility.FromJson<ColorContainer>(json);
                    if (container != null && container.entries != null)
                    {
                        foreach (var entry in container.entries)
                        {
                            s_Colors[entry.id] = new Color(entry.r, entry.g, entry.b, entry.a);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[UICustomColorRegistry] Failed to load custom colors: {ex.Message}");
                }
            }
            s_Loaded = true;
        }

        static void Save()
        {
            var container = new ColorContainer();
            foreach (var kvp in s_Colors)
            {
                container.entries.Add(new ColorEntry
                {
                    id = kvp.Key,
                    r = kvp.Value.r,
                    g = kvp.Value.g,
                    b = kvp.Value.b,
                    a = kvp.Value.a
                });
            }
            string json = JsonUtility.ToJson(container);
            SessionState.SetString(SessionKey, json);
        }

        public static bool TryGetColor(int instanceId, out Color color)
        {
            lock (s_Lock)
            {
                EnsureLoaded();
                return s_Colors.TryGetValue(instanceId, out color);
            }
        }

        public static void SetColor(int instanceId, Color color)
        {
            lock (s_Lock)
            {
                EnsureLoaded();
                s_Colors[instanceId] = color;
                Save();
            }
        }

        public static void RemoveColor(int instanceId)
        {
            lock (s_Lock)
            {
                EnsureLoaded();
                if (s_Colors.Remove(instanceId))
                {
                    Save();
                }
            }
        }

        public static void ClearAll()
        {
            lock (s_Lock)
            {
                EnsureLoaded();
                s_Colors.Clear();
                Save();
            }
        }

        /// <summary>
        /// Forces a reload from SessionState (e.g. for testing domain reloads).
        /// </summary>
        public static void Reload()
        {
            lock (s_Lock)
            {
                s_Loaded = false;
                EnsureLoaded();
            }
        }
    }
}
