using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIDepthInspector.Editor.Benchmark
{
    [Serializable]
    public class InjectedAnomalyEntry
    {
        public string anomalyId;
        public string type;
        public string targetPath;
        public string component;
        public string property;
        public string injectedValue;
        public string expectedValue;
        public string affectedTarget;
    }

    [Serializable]
    public class UIBenchmarkGroundTruth
    {
        public string benchmarkId;
        public int seed;
        public string preset;
        public string generatedAt;
        public int totalElements;
        public int totalAnomalies;
        public List<InjectedAnomalyEntry> anomalies = new List<InjectedAnomalyEntry>();

        public static string ToJson(UIBenchmarkGroundTruth gt, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(gt, prettyPrint);
        }

        public static UIBenchmarkGroundTruth FromJson(string json)
        {
            return JsonUtility.FromJson<UIBenchmarkGroundTruth>(json);
        }
    }

    [Serializable]
    public class UIBenchmarkResult
    {
        public string benchmarkId;
        public string agentName;
        public int truePositives;
        public int falsePositives;
        public int falseNegatives;
        public float precision;
        public float recall;
        public float f1Score;
        public int totalPromptTokens;
        public int totalCompletionTokens;
        public int turns;
        public float durationSeconds;

        public static string ToJson(UIBenchmarkResult result, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(result, prettyPrint);
        }

        public static UIBenchmarkResult FromJson(string json)
        {
            return JsonUtility.FromJson<UIBenchmarkResult>(json);
        }
    }
}
