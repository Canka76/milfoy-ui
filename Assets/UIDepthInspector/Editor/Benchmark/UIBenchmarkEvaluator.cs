using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace UIDepthInspector.Editor.Benchmark
{
    /// <summary>
    /// Raw execution metrics recorded during an agent diagnostic trial.
    /// </summary>
    [Serializable]
    public class AgentTrialRecord
    {
        public string agentName;
        public int totalPromptTokens;
        public int totalCompletionTokens;
        public int turns;
        public float durationSeconds;
        public List<string> reportedIssuePaths = new List<string>();

        public int totalTokens => totalPromptTokens + totalCompletionTokens;

        public static string ToJson(AgentTrialRecord record, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(record, prettyPrint);
        }

        public static AgentTrialRecord FromJson(string json)
        {
            return JsonUtility.FromJson<AgentTrialRecord>(json);
        }
    }

    /// <summary>
    /// Evaluated classification and efficiency metrics for a single agent trial.
    /// </summary>
    [Serializable]
    public class AgentEvaluationMetrics
    {
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
        public AgentTrialRecord trialRecord;

        public int totalTokens => totalPromptTokens + totalCompletionTokens;

        public UIBenchmarkResult ToBenchmarkResult(string benchmarkId = null)
        {
            return new UIBenchmarkResult
            {
                benchmarkId = benchmarkId ?? trialRecord?.agentName ?? "",
                agentName = agentName,
                truePositives = truePositives,
                falsePositives = falsePositives,
                falseNegatives = falseNegatives,
                precision = precision,
                recall = recall,
                f1Score = f1Score,
                totalPromptTokens = totalPromptTokens,
                totalCompletionTokens = totalCompletionTokens,
                turns = turns,
                durationSeconds = durationSeconds
            };
        }
    }

    /// <summary>
    /// Comparative metrics between Milfoy-equipped agent and baseline agent.
    /// </summary>
    [Serializable]
    public class TrialComparisonMetrics
    {
        public string benchmarkId;
        public int groundTruthCount;
        public AgentEvaluationMetrics milfoyMetrics;
        public AgentEvaluationMetrics baselineMetrics;
        public float tokenReductionPercentage;
        public float timeReductionPercentage;
        public float turnReductionPercentage;

        public static string ToJson(TrialComparisonMetrics metrics, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(metrics, prettyPrint);
        }

        public static TrialComparisonMetrics FromJson(string json)
        {
            return JsonUtility.FromJson<TrialComparisonMetrics>(json);
        }
    }

    /// <summary>
    /// Evaluator calculating TP/FP/FN, precision, recall, F1, and comparative scoreboard reports.
    /// </summary>
    public static class UIBenchmarkEvaluator
    {
        /// <summary>
        /// Normalizes hierarchy path for matching (handles leading/trailing slashes, backslashes, and root container).
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            string p = path.Trim().Replace('\\', '/').Trim('/');
            const string rootPrefix = "[Benchmark_Generated_UI]/";
            if (p.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                p = p.Substring(rootPrefix.Length).Trim('/');
            }
            return p;
        }

        /// <summary>
        /// Checks whether two hierarchy paths match under normalization and root tolerance.
        /// </summary>
        public static bool PathMatches(string pathA, string pathB)
        {
            if (string.IsNullOrEmpty(pathA) || string.IsNullOrEmpty(pathB)) return false;
            string normA = NormalizePath(pathA);
            string normB = NormalizePath(pathB);
            return string.Equals(normA, normB, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Evaluates a single agent's reported issues against ground truth anomalies.
        /// </summary>
        public static AgentEvaluationMetrics EvaluateAgent(UIBenchmarkGroundTruth groundTruth, AgentTrialRecord trial)
        {
            var gtAnomalies = groundTruth != null && groundTruth.anomalies != null
                ? groundTruth.anomalies
                : new List<InjectedAnomalyEntry>();

            int tp = 0;
            int fp = 0;
            var matchedGtIndices = new HashSet<int>();

            if (trial != null && trial.reportedIssuePaths != null)
            {
                var seenReported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < trial.reportedIssuePaths.Count; i++)
                {
                    string raw = trial.reportedIssuePaths[i];
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    string norm = NormalizePath(raw);
                    if (!seenReported.Add(norm)) continue;

                    bool matched = false;
                    for (int j = 0; j < gtAnomalies.Count; j++)
                    {
                        if (matchedGtIndices.Contains(j)) continue;

                        var a = gtAnomalies[j];
                        if (PathMatches(norm, a.targetPath) || PathMatches(norm, a.affectedTarget))
                        {
                            matchedGtIndices.Add(j);
                            matched = true;
                            tp++;
                            break;
                        }
                    }

                    if (!matched)
                    {
                        fp++;
                    }
                }
            }

            int fn = gtAnomalies.Count - matchedGtIndices.Count;

            float precision;
            if (tp + fp > 0)
            {
                precision = (float)tp / (tp + fp);
            }
            else
            {
                precision = (fn == 0 && gtAnomalies.Count == 0) ? 1f : 0f;
            }

            float recall;
            if (tp + fn > 0)
            {
                recall = (float)tp / (tp + fn);
            }
            else
            {
                recall = (fp == 0) ? 1f : 0f;
            }

            float f1Score = 0f;
            if (precision + recall > 0.0001f)
            {
                f1Score = 2f * (precision * recall) / (precision + recall);
            }

            return new AgentEvaluationMetrics
            {
                agentName = trial != null ? trial.agentName : "UnknownAgent",
                truePositives = tp,
                falsePositives = fp,
                falseNegatives = fn,
                precision = precision,
                recall = recall,
                f1Score = f1Score,
                totalPromptTokens = trial != null ? trial.totalPromptTokens : 0,
                totalCompletionTokens = trial != null ? trial.totalCompletionTokens : 0,
                turns = trial != null ? trial.turns : 0,
                durationSeconds = trial != null ? trial.durationSeconds : 0f,
                trialRecord = trial
            };
        }

        /// <summary>
        /// Compares a Milfoy-equipped agent against a baseline agent using ground truth anomaly data.
        /// </summary>
        public static TrialComparisonMetrics Evaluate(UIBenchmarkGroundTruth groundTruth, AgentTrialRecord milfoyAgent, AgentTrialRecord baselineAgent)
        {
            var milfoyMetrics = EvaluateAgent(groundTruth, milfoyAgent);
            var baselineMetrics = EvaluateAgent(groundTruth, baselineAgent);

            int baselineTokens = baselineAgent != null ? baselineAgent.totalTokens : 0;
            int milfoyTokens = milfoyAgent != null ? milfoyAgent.totalTokens : 0;
            float tokenReduction = baselineTokens > 0
                ? ((float)(baselineTokens - milfoyTokens) / baselineTokens) * 100f
                : 0f;

            float baselineTime = baselineAgent != null ? baselineAgent.durationSeconds : 0f;
            float milfoyTime = milfoyAgent != null ? milfoyAgent.durationSeconds : 0f;
            float timeReduction = baselineTime > 0.0001f
                ? ((baselineTime - milfoyTime) / baselineTime) * 100f
                : 0f;

            int baselineTurns = baselineAgent != null ? baselineAgent.turns : 0;
            int milfoyTurns = milfoyAgent != null ? milfoyAgent.turns : 0;
            float turnReduction = baselineTurns > 0
                ? ((float)(baselineTurns - milfoyTurns) / baselineTurns) * 100f
                : 0f;

            return new TrialComparisonMetrics
            {
                benchmarkId = groundTruth != null ? groundTruth.benchmarkId : "UnknownBenchmark",
                groundTruthCount = groundTruth != null && groundTruth.anomalies != null ? groundTruth.anomalies.Count : 0,
                milfoyMetrics = milfoyMetrics,
                baselineMetrics = baselineMetrics,
                tokenReductionPercentage = tokenReduction,
                timeReductionPercentage = timeReduction,
                turnReductionPercentage = turnReduction
            };
        }

        /// <summary>
        /// Generates a GitHub-flavored Markdown comparative scoreboard table.
        /// </summary>
        public static string FormatMarkdownReport(TrialComparisonMetrics comparison)
        {
            if (comparison == null) return string.Empty;

            var milfoy = comparison.milfoyMetrics ?? new AgentEvaluationMetrics();
            var baseline = comparison.baselineMetrics ?? new AgentEvaluationMetrics();

            string milfoyName = !string.IsNullOrEmpty(milfoy.agentName) ? milfoy.agentName : "Milfoy-Equipped Agent";
            string baselineName = !string.IsNullOrEmpty(baseline.agentName) ? baseline.agentName : "Baseline Agent (No Milfoy)";

            var sb = new StringBuilder(1024);
            string title = !string.IsNullOrEmpty(comparison.benchmarkId)
                ? $"# Milfoy Dual-Agent Benchmark Report: {comparison.benchmarkId}"
                : "# Milfoy Dual-Agent Benchmark Report";

            sb.AppendLine(title);
            sb.AppendLine();
            sb.AppendLine($"| Metric | {baselineName} | {milfoyName} | Efficiency Gain |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");

            // Total Tokens
            sb.AppendLine($"| **Total Tokens** | {baseline.totalTokens:N0} tokens | {milfoy.totalTokens:N0} tokens | **{FormatEfficiencyReduction(comparison.tokenReductionPercentage)}** |");

            // Tool / Turn Count
            sb.AppendLine($"| **Tool / Turn Count** | {baseline.turns} turns | {milfoy.turns} turns | **{FormatEfficiencyReduction(comparison.turnReductionPercentage)}** |");

            // Time to Complete
            sb.AppendLine($"| **Time to Complete** | {baseline.durationSeconds.ToString("F1", CultureInfo.InvariantCulture)} seconds | {milfoy.durationSeconds.ToString("F1", CultureInfo.InvariantCulture)} seconds | **{FormatEfficiencyReduction(comparison.timeReductionPercentage)}** |");

            // Ground Truth Issues
            sb.AppendLine($"| **Ground Truth Issues** | {comparison.groundTruthCount} | {comparison.groundTruthCount} | - |");

            // True Positives
            string tpDelta = FormatCountDelta(milfoy.truePositives - baseline.truePositives);
            sb.AppendLine($"| **True Positives (TP)** | {baseline.truePositives} | {milfoy.truePositives} | {tpDelta} |");

            // False Positives
            string fpDelta = milfoy.falsePositives == 0 ? "0 FP" : FormatCountDelta(milfoy.falsePositives - baseline.falsePositives);
            sb.AppendLine($"| **False Positives (FP)** | {baseline.falsePositives} | {milfoy.falsePositives} | {fpDelta} |");

            // False Negatives
            string fnDelta = milfoy.falseNegatives == 0 ? "0 FN" : FormatCountDelta(milfoy.falseNegatives - baseline.falseNegatives);
            sb.AppendLine($"| **False Negatives (FN)** | {baseline.falseNegatives} | {milfoy.falseNegatives} | {fnDelta} |");

            // Precision
            string precDelta = FormatPercentDelta(milfoy.precision - baseline.precision);
            sb.AppendLine($"| **Precision** | {FormatPercent(baseline.precision)} | {FormatPercent(milfoy.precision)} | **{precDelta}** |");

            // Recall
            string recallDelta = FormatPercentDelta(milfoy.recall - baseline.recall);
            sb.AppendLine($"| **Recall** | {FormatPercent(baseline.recall)} | {FormatPercent(milfoy.recall)} | **{recallDelta}** |");

            // F1 Score
            string f1Delta = FormatFloatDelta(milfoy.f1Score - baseline.f1Score);
            sb.AppendLine($"| **F1 Score** | {baseline.f1Score.ToString("F3", CultureInfo.InvariantCulture)} | {milfoy.f1Score.ToString("F3", CultureInfo.InvariantCulture)} | **{f1Delta}** |");

            return sb.ToString();
        }

        /// <summary>
        /// Generates CSV comparative scoreboard data for export and CI artifacts.
        /// </summary>
        public static string FormatCsvReport(TrialComparisonMetrics comparison)
        {
            if (comparison == null) return string.Empty;

            var milfoy = comparison.milfoyMetrics ?? new AgentEvaluationMetrics();
            var baseline = comparison.baselineMetrics ?? new AgentEvaluationMetrics();

            string milfoyName = !string.IsNullOrEmpty(milfoy.agentName) ? milfoy.agentName : "Milfoy-Equipped Agent";
            string baselineName = !string.IsNullOrEmpty(baseline.agentName) ? baseline.agentName : "Baseline Agent (No Milfoy)";

            var sb = new StringBuilder(1024);
            sb.AppendLine($"Metric,{EscapeCsv(baselineName)},{EscapeCsv(milfoyName)},Efficiency Gain / Delta");
            sb.AppendLine($"Total Tokens,{baseline.totalTokens},{milfoy.totalTokens},{FormatEfficiencyReduction(comparison.tokenReductionPercentage)}");
            sb.AppendLine($"Prompt Tokens,{baseline.totalPromptTokens},{milfoy.totalPromptTokens},{FormatEfficiencyReduction(CalculateReduction(baseline.totalPromptTokens, milfoy.totalPromptTokens))}");
            sb.AppendLine($"Completion Tokens,{baseline.totalCompletionTokens},{milfoy.totalCompletionTokens},{FormatEfficiencyReduction(CalculateReduction(baseline.totalCompletionTokens, milfoy.totalCompletionTokens))}");
            sb.AppendLine($"Tool / Turn Count,{baseline.turns},{milfoy.turns},{FormatEfficiencyReduction(comparison.turnReductionPercentage)}");
            sb.AppendLine($"Time (seconds),{baseline.durationSeconds.ToString("F2", CultureInfo.InvariantCulture)},{milfoy.durationSeconds.ToString("F2", CultureInfo.InvariantCulture)},{FormatEfficiencyReduction(comparison.timeReductionPercentage)}");
            sb.AppendLine($"Ground Truth Issues,{comparison.groundTruthCount},{comparison.groundTruthCount},-");
            sb.AppendLine($"True Positives (TP),{baseline.truePositives},{milfoy.truePositives},{FormatCountDelta(milfoy.truePositives - baseline.truePositives)}");
            sb.AppendLine($"False Positives (FP),{baseline.falsePositives},{milfoy.falsePositives},{FormatCountDelta(milfoy.falsePositives - baseline.falsePositives)}");
            sb.AppendLine($"False Negatives (FN),{baseline.falseNegatives},{milfoy.falseNegatives},{FormatCountDelta(milfoy.falseNegatives - baseline.falseNegatives)}");
            sb.AppendLine($"Precision,{FormatPercent(baseline.precision)},{FormatPercent(milfoy.precision)},{FormatPercentDelta(milfoy.precision - baseline.precision)}");
            sb.AppendLine($"Recall,{FormatPercent(baseline.recall)},{FormatPercent(milfoy.recall)},{FormatPercentDelta(milfoy.recall - baseline.recall)}");
            sb.AppendLine($"F1 Score,{baseline.f1Score.ToString("F3", CultureInfo.InvariantCulture)},{milfoy.f1Score.ToString("F3", CultureInfo.InvariantCulture)},{FormatFloatDelta(milfoy.f1Score - baseline.f1Score)}");

            return sb.ToString();
        }

        private static string FormatPercent(float val)
        {
            return (val * 100f).ToString("F1", CultureInfo.InvariantCulture) + "%";
        }

        private static string FormatEfficiencyReduction(float reductionPercentage)
        {
            if (Mathf.Abs(reductionPercentage) < 0.05f) return "0.0%";
            return reductionPercentage > 0
                ? $"-{reductionPercentage.ToString("F1", CultureInfo.InvariantCulture)}%"
                : $"+{Mathf.Abs(reductionPercentage).ToString("F1", CultureInfo.InvariantCulture)}%";
        }

        private static string FormatPercentDelta(float delta)
        {
            if (Mathf.Abs(delta) < 0.0005f) return "0.0%";
            float pct = delta * 100f;
            return pct > 0
                ? $"+{pct.ToString("F1", CultureInfo.InvariantCulture)}%"
                : $"-{Mathf.Abs(pct).ToString("F1", CultureInfo.InvariantCulture)}%";
        }

        private static string FormatCountDelta(int delta)
        {
            if (delta == 0) return "0";
            return delta > 0 ? $"+{delta}" : $"{delta}";
        }

        private static string FormatFloatDelta(float delta)
        {
            if (Mathf.Abs(delta) < 0.0005f) return "0.000";
            return delta > 0
                ? $"+{delta.ToString("F3", CultureInfo.InvariantCulture)}"
                : $"-{Mathf.Abs(delta).ToString("F3", CultureInfo.InvariantCulture)}";
        }

        private static float CalculateReduction(float baseline, float current)
        {
            if (baseline <= 0.0001f) return 0f;
            return ((baseline - current) / baseline) * 100f;
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Contains(",") || text.Contains("\"") || text.Contains("\n") || text.Contains("\r"))
            {
                return $"\"{text.Replace("\"", "\"\"")}\"";
            }
            return text;
        }
    }
}
