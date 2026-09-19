using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UIDepthInspector.Editor.Benchmark;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;
using UIDepthInspector.Editor.Export;
namespace UIDepthInspector.Editor.Tests
{
    [TestFixture]
    public class UIBenchmarkTests
    {
        [Test]
        public void GroundTruth_Serialization_RoundTripsAccurately()
        {
            var gt = new UIBenchmarkGroundTruth
            {
                benchmarkId = "BENCH_TEST_001",
                seed = 42,
                preset = "CasualHud",
                generatedAt = "2026-09-19T12:00:00Z",
                totalElements = 25,
                totalAnomalies = 2
            };

            gt.anomalies.Add(new InjectedAnomalyEntry
            {
                anomalyId = "ANO_001",
                type = "ANOMALY_GHOST_BLOCKER",
                targetPath = "Canvas/Panel/GhostBackdrop",
                component = "Image",
                property = "raycastTarget",
                injectedValue = "true",
                expectedValue = "false",
                affectedTarget = "Canvas/Panel/Button_Play"
            });

            string json = UIBenchmarkGroundTruth.ToJson(gt, prettyPrint: true);
            if (string.IsNullOrEmpty(json))
                throw new Exception("Expected non-empty JSON string");

            var loaded = UIBenchmarkGroundTruth.FromJson(json);
            if (loaded == null)
                throw new Exception("Expected deserialized UIBenchmarkGroundTruth to not be null");
            if (loaded.benchmarkId != "BENCH_TEST_001")
                throw new Exception($"Expected benchmarkId 'BENCH_TEST_001', got '{loaded.benchmarkId}'");
            if (loaded.seed != 42)
                throw new Exception($"Expected seed 42, got {loaded.seed}");
            if (loaded.preset != "CasualHud")
                throw new Exception($"Expected preset 'CasualHud', got '{loaded.preset}'");
            if (loaded.totalElements != 25)
                throw new Exception($"Expected totalElements 25, got {loaded.totalElements}");
            if (loaded.totalAnomalies != 2)
                throw new Exception($"Expected totalAnomalies 2, got {loaded.totalAnomalies}");
            if (loaded.anomalies == null || loaded.anomalies.Count != 1)
                throw new Exception($"Expected 1 anomaly, got {loaded.anomalies?.Count}");

            var a = loaded.anomalies[0];
            if (a.anomalyId != "ANO_001")
                throw new Exception($"Expected anomalyId 'ANO_001', got '{a.anomalyId}'");
            if (a.type != "ANOMALY_GHOST_BLOCKER")
                throw new Exception($"Expected type 'ANOMALY_GHOST_BLOCKER', got '{a.type}'");
            if (a.targetPath != "Canvas/Panel/GhostBackdrop")
                throw new Exception($"Expected targetPath 'Canvas/Panel/GhostBackdrop', got '{a.targetPath}'");
            if (a.component != "Image")
                throw new Exception($"Expected component 'Image', got '{a.component}'");
            if (a.property != "raycastTarget")
                throw new Exception($"Expected property 'raycastTarget', got '{a.property}'");
            if (a.injectedValue != "true")
                throw new Exception($"Expected injectedValue 'true', got '{a.injectedValue}'");
            if (a.expectedValue != "false")
                throw new Exception($"Expected expectedValue 'false', got '{a.expectedValue}'");
            if (a.affectedTarget != "Canvas/Panel/Button_Play")
                throw new Exception($"Expected affectedTarget 'Canvas/Panel/Button_Play', got '{a.affectedTarget}'");

            Assert.AreEqual("BENCH_TEST_001", loaded.benchmarkId);
            Assert.AreEqual(42, loaded.seed);
            Assert.AreEqual(1, loaded.anomalies.Count);
            Assert.AreEqual("ANOMALY_GHOST_BLOCKER", loaded.anomalies[0].type);
            Assert.AreEqual("Canvas/Panel/Button_Play", loaded.anomalies[0].affectedTarget);
        }

        [Test]
        public void BenchmarkResult_Serialization_RoundTripsAccurately()
        {
            var res = new UIBenchmarkResult
            {
                benchmarkId = "BENCH_TEST_001",
                agentName = "MilfoyAgent",
                truePositives = 5,
                falsePositives = 0,
                falseNegatives = 1,
                precision = 1.0f,
                recall = 0.833f,
                f1Score = 0.909f,
                totalPromptTokens = 1200,
                totalCompletionTokens = 400,
                turns = 3,
                durationSeconds = 6.2f
            };

            string json = UIBenchmarkResult.ToJson(res, prettyPrint: true);
            if (string.IsNullOrEmpty(json))
                throw new Exception("Expected non-empty JSON string");

            var loaded = UIBenchmarkResult.FromJson(json);
            if (loaded == null)
                throw new Exception("Expected deserialized UIBenchmarkResult to not be null");
            if (loaded.benchmarkId != "BENCH_TEST_001")
                throw new Exception($"Expected benchmarkId 'BENCH_TEST_001', got '{loaded.benchmarkId}'");
            if (loaded.agentName != "MilfoyAgent")
                throw new Exception($"Expected agentName 'MilfoyAgent', got '{loaded.agentName}'");
            if (loaded.truePositives != 5)
                throw new Exception($"Expected truePositives 5, got {loaded.truePositives}");
            if (loaded.falsePositives != 0)
                throw new Exception($"Expected falsePositives 0, got {loaded.falsePositives}");
            if (loaded.falseNegatives != 1)
                throw new Exception($"Expected falseNegatives 1, got {loaded.falseNegatives}");

            Assert.AreEqual("BENCH_TEST_001", loaded.benchmarkId);
            Assert.AreEqual("MilfoyAgent", loaded.agentName);
            Assert.AreEqual(5, loaded.truePositives);
            Assert.AreEqual(1.0f, loaded.precision, 0.001f);
        }

        [Test]
        public void Generator_CleanPreset_ProducesHierarchyWithZeroAnomalies()
        {
            var config = UIBenchmarkPreset.GetConfig(UIBenchmarkPresetType.CleanReference);
            var root = UISyntheticSceneGenerator.Generate(config, seed: 100, out var groundTruth);
            try
            {
                if (root == null)
                    throw new Exception("Expected generated root GameObject to not be null");
                if (groundTruth.totalAnomalies != 0)
                    throw new Exception($"Expected 0 anomalies for CleanReference, got {groundTruth.totalAnomalies}");
                if (groundTruth.anomalies.Count != 0)
                    throw new Exception($"Expected 0 anomalies in list, got {groundTruth.anomalies.Count}");
                if (groundTruth.totalElements <= 10)
                    throw new Exception($"Expected > 10 total elements, got {groundTruth.totalElements}");

                Assert.IsNotNull(root);
                Assert.AreEqual(0, groundTruth.totalAnomalies);
                Assert.AreEqual(0, groundTruth.anomalies.Count);
                Assert.Greater(groundTruth.totalElements, 10);
            }
            finally
            {
                UISyntheticSceneGenerator.ClearBenchmarkUI();
                if (GameObject.Find(UISyntheticSceneGenerator.RootContainerName) != null)
                    throw new Exception("Expected benchmark UI to be completely cleared");
                Assert.IsNull(GameObject.Find(UISyntheticSceneGenerator.RootContainerName));
            }
        }

        [Test]
        public void Generator_SameSeed_ProducesIdenticalHierarchyAndCount()
        {
            var config = UIBenchmarkPreset.GetConfig(UIBenchmarkPresetType.CasualHud);
            int seed = 200;

            var root1 = UISyntheticSceneGenerator.Generate(config, seed, out var gt1);
            int elementCount1 = gt1.totalElements;
            int anomalyCount1 = gt1.totalAnomalies;
            var paths1 = new List<string>();
            foreach (var rt in root1.GetComponentsInChildren<RectTransform>(true))
            {
                paths1.Add(UISyntheticSceneGenerator.GetHierarchyPath(rt, root1.transform));
            }
            UISyntheticSceneGenerator.ClearBenchmarkUI();

            var root2 = UISyntheticSceneGenerator.Generate(config, seed, out var gt2);
            int elementCount2 = gt2.totalElements;
            int anomalyCount2 = gt2.totalAnomalies;
            var paths2 = new List<string>();
            foreach (var rt in root2.GetComponentsInChildren<RectTransform>(true))
            {
                paths2.Add(UISyntheticSceneGenerator.GetHierarchyPath(rt, root2.transform));
            }
            UISyntheticSceneGenerator.ClearBenchmarkUI();

            if (elementCount1 != elementCount2)
                throw new Exception($"Deterministic mismatch: element counts {elementCount1} != {elementCount2}");
            if (anomalyCount1 != anomalyCount2)
                throw new Exception($"Deterministic mismatch: anomaly counts {anomalyCount1} != {anomalyCount2}");
            if (paths1.Count != paths2.Count)
                throw new Exception($"Deterministic mismatch: path counts {paths1.Count} != {paths2.Count}");

            for (int i = 0; i < paths1.Count; i++)
            {
                if (paths1[i] != paths2[i])
                    throw new Exception($"Deterministic mismatch at index {i}: '{paths1[i]}' != '{paths2[i]}'");
            }

            Assert.AreEqual(elementCount1, elementCount2);
            Assert.AreEqual(anomalyCount1, anomalyCount2);
            CollectionAssert.AreEqual(paths1, paths2);
        }

        [Test]
        public void Presets_AllTypes_ReturnValidConfigurations()
        {
            var types = (UIBenchmarkPresetType[])Enum.GetValues(typeof(UIBenchmarkPresetType));
            foreach (var type in types)
            {
                var cfg = UIBenchmarkPreset.GetConfig(type);
                if (cfg == null)
                    throw new Exception($"Expected non-null config for preset {type}");
                if (string.IsNullOrEmpty(cfg.name))
                    throw new Exception($"Expected non-empty name for preset {type}");
                if (cfg.targetElementCount <= 0)
                    throw new Exception($"Expected targetElementCount > 0 for preset {type}");
                if (cfg.canvasCount <= 0)
                    throw new Exception($"Expected canvasCount > 0 for preset {type}");

                Assert.IsNotNull(cfg);
                Assert.IsNotEmpty(cfg.name);
                Assert.Greater(cfg.targetElementCount, 0);
                Assert.Greater(cfg.canvasCount, 0);
            }
        }

        [Test]
        public void Generator_ChaoticPreset_InjectsAnomalies()
        {
            var config = UIBenchmarkPreset.GetConfig(UIBenchmarkPresetType.ChaoticStress);
            var root = UISyntheticSceneGenerator.Generate(config, seed: 400, out var groundTruth);
            try
            {
                if (root == null)
                    throw new Exception("Expected generated root to not be null");
                if (groundTruth.totalAnomalies == 0)
                    throw new Exception("Expected ChaoticStress preset to inject anomalies");
                if (groundTruth.anomalies.Count == 0)
                    throw new Exception("Expected groundTruth.anomalies to contain entries");

                var types = new HashSet<string>();
                foreach (var a in groundTruth.anomalies)
                    types.Add(a.type);

                if (!types.Contains("ANOMALY_GHOST_BLOCKER"))
                    throw new Exception("Expected ANOMALY_GHOST_BLOCKER in chaotic anomalies");
                if (!types.Contains("ANOMALY_SPATIAL_OVERLAP"))
                    throw new Exception("Expected ANOMALY_SPATIAL_OVERLAP in chaotic anomalies");
                if (!types.Contains("ANOMALY_MISSING_SPRITE"))
                    throw new Exception("Expected ANOMALY_MISSING_SPRITE in chaotic anomalies");
                if (!types.Contains("ANOMALY_NESTED_LABEL_RAYCAST"))
                    throw new Exception("Expected ANOMALY_NESTED_LABEL_RAYCAST in chaotic anomalies");
                if (!types.Contains("ANOMALY_CANVASGROUP_TRAP"))
                    throw new Exception("Expected ANOMALY_CANVASGROUP_TRAP in chaotic anomalies");

                Assert.GreaterOrEqual(groundTruth.totalAnomalies, 5);
                Assert.IsTrue(types.Contains("ANOMALY_GHOST_BLOCKER"));
                Assert.IsTrue(types.Contains("ANOMALY_SPATIAL_OVERLAP"));
                Assert.IsTrue(types.Contains("ANOMALY_MISSING_SPRITE"));
                Assert.IsTrue(types.Contains("ANOMALY_NESTED_LABEL_RAYCAST"));
                Assert.IsTrue(types.Contains("ANOMALY_CANVASGROUP_TRAP"));
            }
            finally
            {
                UISyntheticSceneGenerator.ClearBenchmarkUI();
                Assert.IsNull(GameObject.Find(UISyntheticSceneGenerator.RootContainerName));
            }
        }

        [Test]
        public void Milfoy_Detects_All_Injected_Benchmark_Anomalies()
        {
            var config = UIBenchmarkPreset.GetConfig(UIBenchmarkPresetType.ChaoticStress);
            var root = UISyntheticSceneGenerator.Generate(config, seed: 400, out var groundTruth);
            try
            {
                if (root == null)
                    throw new Exception("Expected generated root to not be null");
                if (groundTruth == null || groundTruth.anomalies.Count < 5)
                    throw new Exception($"Expected at least 5 anomalies, got {groundTruth?.anomalies.Count}");

                var entries = UIRenderTreeCollector.Collect();
                UIDiagnosticAnalyzer.Analyze(entries);

                if (entries == null || entries.Count == 0)
                    throw new Exception("Expected collected UI entries to be non-empty");

                var entryByPath = new Dictionary<string, UIElementEntry>(entries.Count);
                for (int i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (e.Transform != null)
                    {
                        string path = UISyntheticSceneGenerator.GetHierarchyPath(e.Transform, root.transform);
                        entryByPath[path] = e;
                    }
                }

                foreach (var anomaly in groundTruth.anomalies)
                {
                    UIElementEntry matchedEntry;
                    bool found = entryByPath.TryGetValue(anomaly.targetPath, out matchedEntry);
                    if (!found && !string.IsNullOrEmpty(anomaly.affectedTarget))
                    {
                        found = entryByPath.TryGetValue(anomaly.affectedTarget, out matchedEntry);
                    }

                    if (!found)
                        throw new Exception($"Injected anomaly {anomaly.anomalyId} ({anomaly.type}) at '{anomaly.targetPath}' not found in collected UI entries");

                    switch (anomaly.type)
                    {
                        case "ANOMALY_GHOST_BLOCKER":
                            if ((matchedEntry.Flags & DiagnosticFlags.GhostBlocker) == 0)
                                 throw new Exception($"Anomaly {anomaly.anomalyId} ({anomaly.targetPath}) expected DiagnosticFlags.GhostBlocker, got {matchedEntry.Flags}");
                            break;

                        case "ANOMALY_SPATIAL_OVERLAP":
                            if ((matchedEntry.Flags & DiagnosticFlags.OcclusionBlocker) == 0)
                                 throw new Exception($"Anomaly {anomaly.anomalyId} ({anomaly.targetPath}) expected DiagnosticFlags.OcclusionBlocker, got {matchedEntry.Flags}");
                            break;

                        case "ANOMALY_MISSING_SPRITE":
                            if ((matchedEntry.Flags & (DiagnosticFlags.GhostBlocker | DiagnosticFlags.ZeroSize)) == 0)
                                 throw new Exception($"Anomaly {anomaly.anomalyId} ({anomaly.targetPath}) expected DiagnosticFlags.GhostBlocker or ZeroSize, got {matchedEntry.Flags}");
                            break;

                        case "ANOMALY_NESTED_LABEL_RAYCAST":
                            if ((matchedEntry.Flags & DiagnosticFlags.NestedLabelRaycast) == 0)
                                 throw new Exception($"Anomaly {anomaly.anomalyId} ({anomaly.targetPath}) expected DiagnosticFlags.NestedLabelRaycast, got {matchedEntry.Flags}");
                            break;

                        case "ANOMALY_CANVASGROUP_TRAP":
                            if ((matchedEntry.Flags & (DiagnosticFlags.GroupBlocked | DiagnosticFlags.GroupTransparent)) == 0)
                                 throw new Exception($"Anomaly {anomaly.anomalyId} ({anomaly.targetPath}) expected GroupBlocked or GroupTransparent, got {matchedEntry.Flags}");
                            break;

                        default:
                            throw new Exception($"Unknown anomaly type {anomaly.type}");
                    }
                }

                Assert.GreaterOrEqual(groundTruth.anomalies.Count, 5);
            }
            finally
            {
                UIRenderTreeCollector.Collect(System.Array.Empty<Canvas>());
                UISyntheticSceneGenerator.ClearBenchmarkUI();
                Assert.IsNull(GameObject.Find(UISyntheticSceneGenerator.RootContainerName));
            }
        }

        [Test]
        public void Evaluator_CalculatesPrecisionRecallAndF1Accurately()
        {
            var gt = new UIBenchmarkGroundTruth { benchmarkId = "B1" };
            gt.anomalies.Add(new InjectedAnomalyEntry { anomalyId = "A1", targetPath = "Canvas/P1", type = "ANOMALY_GHOST_BLOCKER" });
            gt.anomalies.Add(new InjectedAnomalyEntry { anomalyId = "A2", targetPath = "Canvas/P2", type = "ANOMALY_NESTED_LABEL_RAYCAST" });

            var milfoyAgent = new AgentTrialRecord
            {
                agentName = "MilfoyAgent",
                totalPromptTokens = 800,
                totalCompletionTokens = 200,
                turns = 2,
                durationSeconds = 4.5f,
                reportedIssuePaths = new List<string> { "Canvas/P1", "Canvas/P2" }
            };

            var baselineAgent = new AgentTrialRecord
            {
                agentName = "BaselineAgent",
                totalPromptTokens = 12000,
                totalCompletionTokens = 3500,
                turns = 14,
                durationSeconds = 85.0f,
                reportedIssuePaths = new List<string> { "Canvas/P1", "Canvas/HallucinatedElement" }
            };

            var comparison = UIBenchmarkEvaluator.Evaluate(gt, milfoyAgent, baselineAgent);

            if (Math.Abs(comparison.milfoyMetrics.precision - 1.0f) > 0.01f)
                throw new Exception($"Expected milfoy precision 1.0, got {comparison.milfoyMetrics.precision}");
            if (Math.Abs(comparison.milfoyMetrics.recall - 1.0f) > 0.01f)
                throw new Exception($"Expected milfoy recall 1.0, got {comparison.milfoyMetrics.recall}");
            if (Math.Abs(comparison.milfoyMetrics.f1Score - 1.0f) > 0.01f)
                throw new Exception($"Expected milfoy f1Score 1.0, got {comparison.milfoyMetrics.f1Score}");
            if (comparison.milfoyMetrics.truePositives != 2)
                throw new Exception($"Expected milfoy TP 2, got {comparison.milfoyMetrics.truePositives}");
            if (comparison.milfoyMetrics.falsePositives != 0)
                throw new Exception($"Expected milfoy FP 0, got {comparison.milfoyMetrics.falsePositives}");
            if (comparison.milfoyMetrics.falseNegatives != 0)
                throw new Exception($"Expected milfoy FN 0, got {comparison.milfoyMetrics.falseNegatives}");

            if (Math.Abs(comparison.baselineMetrics.precision - 0.5f) > 0.01f)
                throw new Exception($"Expected baseline precision 0.5, got {comparison.baselineMetrics.precision}");
            if (Math.Abs(comparison.baselineMetrics.recall - 0.5f) > 0.01f)
                throw new Exception($"Expected baseline recall 0.5, got {comparison.baselineMetrics.recall}");
            if (Math.Abs(comparison.baselineMetrics.f1Score - 0.5f) > 0.01f)
                throw new Exception($"Expected baseline f1Score 0.5, got {comparison.baselineMetrics.f1Score}");
            if (comparison.baselineMetrics.truePositives != 1)
                throw new Exception($"Expected baseline TP 1, got {comparison.baselineMetrics.truePositives}");
            if (comparison.baselineMetrics.falsePositives != 1)
                throw new Exception($"Expected baseline FP 1, got {comparison.baselineMetrics.falsePositives}");
            if (comparison.baselineMetrics.falseNegatives != 1)
                throw new Exception($"Expected baseline FN 1, got {comparison.baselineMetrics.falseNegatives}");

            if (comparison.tokenReductionPercentage <= 90f)
                throw new Exception($"Expected token reduction > 90%, got {comparison.tokenReductionPercentage}");
            if (comparison.timeReductionPercentage <= 90f)
                throw new Exception($"Expected time reduction > 90%, got {comparison.timeReductionPercentage}");
            if (comparison.turnReductionPercentage <= 80f)
                throw new Exception($"Expected turn reduction > 80%, got {comparison.turnReductionPercentage}");

            Assert.AreEqual(1.0f, comparison.milfoyMetrics.precision, 0.01f);
            Assert.AreEqual(1.0f, comparison.milfoyMetrics.recall, 0.01f);
            Assert.AreEqual(1.0f, comparison.milfoyMetrics.f1Score, 0.01f);
            Assert.AreEqual(0.5f, comparison.baselineMetrics.precision, 0.01f);
            Assert.AreEqual(0.5f, comparison.baselineMetrics.recall, 0.01f);
            Assert.AreEqual(0.5f, comparison.baselineMetrics.f1Score, 0.01f);
            Assert.Greater(comparison.tokenReductionPercentage, 90f);
            Assert.Greater(comparison.timeReductionPercentage, 90f);
            Assert.Greater(comparison.turnReductionPercentage, 80f);
        }

        [Test]
        public void Evaluator_FormatsMarkdownAndCsvReportsCorrectly()
        {
            var gt = new UIBenchmarkGroundTruth { benchmarkId = "BENCH_TEST_042" };
            gt.anomalies.Add(new InjectedAnomalyEntry { anomalyId = "A1", targetPath = "Canvas/P1", type = "ANOMALY_GHOST_BLOCKER" });
            gt.anomalies.Add(new InjectedAnomalyEntry { anomalyId = "A2", targetPath = "Canvas/P2", type = "ANOMALY_NESTED_LABEL_RAYCAST" });

            var milfoy = new AgentTrialRecord
            {
                agentName = "MilfoyAgent",
                totalPromptTokens = 800,
                totalCompletionTokens = 200,
                turns = 2,
                durationSeconds = 4.5f,
                reportedIssuePaths = new List<string> { "Canvas/P1", "Canvas/P2" }
            };

            var baseline = new AgentTrialRecord
            {
                agentName = "BaselineAgent",
                totalPromptTokens = 12000,
                totalCompletionTokens = 3500,
                turns = 14,
                durationSeconds = 85.0f,
                reportedIssuePaths = new List<string> { "Canvas/P1", "Canvas/HallucinatedElement" }
            };

            var comparison = UIBenchmarkEvaluator.Evaluate(gt, milfoy, baseline);

            string md = UIBenchmarkEvaluator.FormatMarkdownReport(comparison);
            if (string.IsNullOrEmpty(md))
                throw new Exception("Expected non-empty markdown report");
            if (!md.Contains("# Milfoy Dual-Agent Benchmark Report: BENCH_TEST_042"))
                throw new Exception("Expected markdown report title with benchmarkId");
            if (!md.Contains("MilfoyAgent") || !md.Contains("BaselineAgent"))
                throw new Exception("Expected markdown report to contain agent names");
            if (!md.Contains("| **Total Tokens** |") || !md.Contains("| **Precision** |"))
                throw new Exception("Expected markdown report to contain metric rows");
            if (!md.Contains("-93.5%"))
                throw new Exception("Expected markdown report to contain -93.5% token reduction");

            string csv = UIBenchmarkEvaluator.FormatCsvReport(comparison);
            if (string.IsNullOrEmpty(csv))
                throw new Exception("Expected non-empty CSV report");
            if (!csv.Contains("Metric,BaselineAgent,MilfoyAgent,Efficiency Gain / Delta"))
                throw new Exception("Expected CSV report header with agent names");
            if (!csv.Contains("Total Tokens,15500,1000,-93.5%"))
                throw new Exception("Expected CSV report total tokens row");
            if (!csv.Contains("Tool / Turn Count,14,2,-85.7%"))
                throw new Exception("Expected CSV report turn count row");
            if (!csv.Contains("Precision,50.0%,100.0%,+50.0%"))
                throw new Exception("Expected CSV report precision row");

            Assert.IsNotEmpty(md);
            Assert.IsTrue(md.Contains("MilfoyAgent"));
            Assert.IsTrue(md.Contains("-93.5%"));
            Assert.IsNotEmpty(csv);
            Assert.IsTrue(csv.Contains("Total Tokens"));
        }

        [Test]
        public void CLI_ParseBenchmarkPreset_ResolvesAllPresetsAndDefaults()
        {
            Assert.AreEqual(UIBenchmarkPresetType.CleanReference, UIDepthInspectorCLI.ParseBenchmarkPreset("CleanReference"));
            Assert.AreEqual(UIBenchmarkPresetType.CleanReference, UIDepthInspectorCLI.ParseBenchmarkPreset("cleanreference"));
            Assert.AreEqual(UIBenchmarkPresetType.CasualHud, UIDepthInspectorCLI.ParseBenchmarkPreset("CasualHud"));
            Assert.AreEqual(UIBenchmarkPresetType.CasualHud, UIDepthInspectorCLI.ParseBenchmarkPreset("casualhud"));
            Assert.AreEqual(UIBenchmarkPresetType.DeepProduction, UIDepthInspectorCLI.ParseBenchmarkPreset("DeepProduction"));
            Assert.AreEqual(UIBenchmarkPresetType.ChaoticStress, UIDepthInspectorCLI.ParseBenchmarkPreset("ChaoticStress"));
            Assert.AreEqual(UIBenchmarkPresetType.Custom, UIDepthInspectorCLI.ParseBenchmarkPreset("Custom"));
            Assert.AreEqual(UIBenchmarkPresetType.CasualHud, UIDepthInspectorCLI.ParseBenchmarkPreset(""));
            Assert.AreEqual(UIBenchmarkPresetType.CasualHud, UIDepthInspectorCLI.ParseBenchmarkPreset(null));
            Assert.AreEqual(UIBenchmarkPresetType.CasualHud, UIDepthInspectorCLI.ParseBenchmarkPreset("NonExistentPreset"));
        }

        [Test]
        public void CLI_GenerateBenchmark_ProducesGroundTruthAndInstructions()
        {
            string testDir = Path.Combine("BenchmarkTrials", "Test_CLI_Generate");
            try
            {
                var gt = UIDepthInspectorCLI.GenerateBenchmark(UIBenchmarkPresetType.CleanReference, seed: 123, outDir: testDir);
                if (gt == null)
                    throw new Exception("Expected generated ground truth to be non-null");

                string gtFile = Path.Combine(testDir, "benchmark-ground-truth.json");
                string instrFile = Path.Combine(testDir, "challenge-instructions.md");
                string milfoyTemplate = Path.Combine(testDir, "milfoy-agent", "trial-record-template.json");
                string baselineTemplate = Path.Combine(testDir, "baseline-agent", "trial-record-template.json");

                if (!File.Exists(gtFile))
                    throw new Exception($"Expected {gtFile} to exist");
                if (!File.Exists(instrFile))
                    throw new Exception($"Expected {instrFile} to exist");
                if (!File.Exists(milfoyTemplate))
                    throw new Exception($"Expected {milfoyTemplate} to exist");
                if (!File.Exists(baselineTemplate))
                    throw new Exception($"Expected {baselineTemplate} to exist");

                string gtContent = File.ReadAllText(gtFile);
                var loadedGt = UIBenchmarkGroundTruth.FromJson(gtContent);
                if (loadedGt.seed != 123)
                    throw new Exception($"Expected loaded seed 123, got {loadedGt.seed}");
                if (loadedGt.preset != "CleanReference")
                    throw new Exception($"Expected preset CleanReference, got {loadedGt.preset}");

                Assert.IsTrue(File.Exists(gtFile));
                Assert.IsTrue(File.Exists(instrFile));
                Assert.IsTrue(File.Exists(milfoyTemplate));
                Assert.IsTrue(File.Exists(baselineTemplate));
            }
            finally
            {
                UISyntheticSceneGenerator.ClearBenchmarkUI();
                if (Directory.Exists(testDir))
                {
                    try { Directory.Delete(testDir, true); } catch { }
                }
            }
        }

        [Test]
        public void CLI_EvaluateBenchmark_CalculatesComparisonAndWritesReports()
        {
            string testDir = Path.Combine("BenchmarkTrials", "Test_CLI_Evaluate");
            try
            {
                Directory.CreateDirectory(testDir);

                var gt = new UIBenchmarkGroundTruth { benchmarkId = "CLI_TEST_BENCH", seed = 77 };
                gt.anomalies.Add(new InjectedAnomalyEntry { anomalyId = "A1", targetPath = "Canvas/P1", type = "ANOMALY_GHOST_BLOCKER" });

                var milfoyTrial = new AgentTrialRecord
                {
                    agentName = "MilfoyTestAgent",
                    totalPromptTokens = 500,
                    totalCompletionTokens = 100,
                    turns = 2,
                    durationSeconds = 2.5f,
                    reportedIssuePaths = new List<string> { "Canvas/P1" }
                };

                var baselineTrial = new AgentTrialRecord
                {
                    agentName = "BaselineTestAgent",
                    totalPromptTokens = 8000,
                    totalCompletionTokens = 2000,
                    turns = 10,
                    durationSeconds = 45.0f,
                    reportedIssuePaths = new List<string> { "Canvas/P1", "Canvas/FakeElement" }
                };

                string gtPath = Path.Combine(testDir, "ground-truth.json");
                string milfoyPath = Path.Combine(testDir, "milfoy-result.json");
                string baselinePath = Path.Combine(testDir, "baseline-result.json");

                File.WriteAllText(gtPath, UIBenchmarkGroundTruth.ToJson(gt));
                File.WriteAllText(milfoyPath, AgentTrialRecord.ToJson(milfoyTrial));
                File.WriteAllText(baselinePath, AgentTrialRecord.ToJson(baselineTrial));

                var comparison = UIDepthInspectorCLI.EvaluateBenchmark(gtPath, milfoyPath, baselinePath, testDir);

                if (comparison == null)
                    throw new Exception("Expected comparison result to not be null");
                if (comparison.milfoyMetrics.precision != 1.0f)
                    throw new Exception($"Expected milfoy precision 1.0, got {comparison.milfoyMetrics.precision}");
                if (comparison.baselineMetrics.precision != 0.5f)
                    throw new Exception($"Expected baseline precision 0.5, got {comparison.baselineMetrics.precision}");

                string mdPath = Path.Combine(testDir, "benchmark-report.md");
                string csvPath = Path.Combine(testDir, "benchmark-report.csv");

                if (!File.Exists(mdPath))
                    throw new Exception($"Expected report file {mdPath} to exist");
                if (!File.Exists(csvPath))
                    throw new Exception($"Expected report file {csvPath} to exist");

                string md = File.ReadAllText(mdPath);
                string csv = File.ReadAllText(csvPath);

                if (!md.Contains("MilfoyTestAgent") || !csv.Contains("BaselineTestAgent"))
                    throw new Exception("Expected reports to contain test agent names");

                Assert.IsTrue(File.Exists(mdPath));
                Assert.IsTrue(File.Exists(csvPath));
                Assert.AreEqual(1.0f, comparison.milfoyMetrics.precision);
                Assert.AreEqual(0.5f, comparison.baselineMetrics.precision);
            }
            finally
            {
                if (Directory.Exists(testDir))
                {
                    try { Directory.Delete(testDir, true); } catch { }
                }
            }
        }
    }
}
