using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UIDepthInspector.Editor.Benchmark;

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
    }
}
