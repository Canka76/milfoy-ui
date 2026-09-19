using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UIDepthInspector.Editor.Benchmark;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;
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
    }
}
