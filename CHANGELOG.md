# Changelog

## [0.2.0](https://github.com/Canka76/milfoy-ui/compare/v0.1.0...v0.2.0) (2026-09-19)


### Features

* **benchmark:** add CLI entrypoints, trial runner script, and documentation ([06de0c4](https://github.com/Canka76/milfoy-ui/commit/06de0c44aed19e4151f5a1ac62996432e11771ae))
* **benchmark:** add preset configs and UISyntheticSceneGenerator with clean teardown ([dcd8ac9](https://github.com/Canka76/milfoy-ui/commit/dcd8ac9c89a3953fc5c0ddbb6068796920dc9d92))
* **benchmark:** add UIBenchmarkGroundTruth models and JSON serialization ([50b91cb](https://github.com/Canka76/milfoy-ui/commit/50b91cbb84f74e65a61be43d27671a7a3494032e))
* **benchmark:** add UIBenchmarkWindow Editor GUI and toolbar integration ([2d67285](https://github.com/Canka76/milfoy-ui/commit/2d67285e2c2a1e049e1fb363272e2b1ccc10babd))
* **benchmark:** implement UIBenchmarkEvaluator and comparative report formatting ([b480314](https://github.com/Canka76/milfoy-ui/commit/b480314cc2813aefbe5797f0374d29c9666dd268))
* **benchmark:** verify all 5 anomaly types in generator and diagnostic analyzer ([8154532](https://github.com/Canka76/milfoy-ui/commit/815453298fe2b3020ade9fa1826bbfa787002a27))
* **diagnostics:** add spatial occlusion analysis and zero-gc lookups ([d8bdeec](https://github.com/Canka76/milfoy-ui/commit/d8bdeecd0947a1a971047614f2dd1ca7b9784781))
* **export:** add token-optimized AI exporters and CLI dump modes ([f5d8183](https://github.com/Canka76/milfoy-ui/commit/f5d81838d55e4d084f7bd232e56763bcd60771d7))
* **export:** add zero-overhead scene-saved auto-exporter ([0ceab9a](https://github.com/Canka76/milfoy-ui/commit/0ceab9a914b5a23f122bb025b1a82ec7b440f5ba))
* **ui:** refine depth inspector window and viewport styling ([a34fcb3](https://github.com/Canka76/milfoy-ui/commit/a34fcb3c49dd71a659f71dd1e943b93aca6b6825))


### Bug Fixes

* **benchmark:** ensure UIDepthInspectorWindow is open before setting Selection in InspectInViewport ([f9bfa15](https://github.com/Canka76/milfoy-ui/commit/f9bfa15419f5b3159aafd5fd1a04efc37d8104d1))
* **benchmark:** flush static UIRenderTreeCollector state in test teardown ([ad9e3c6](https://github.com/Canka76/milfoy-ui/commit/ad9e3c636e2f95153f046e7d056d0c16ae7ec480))
* **benchmark:** integrate System.Random for procedural card count and set HideAndDontSave on default sprite ([232787a](https://github.com/Canka76/milfoy-ui/commit/232787a566cbd151422d20e899c6b105205cc40b))
* **benchmark:** refine evaluator path matching and unify delta formatting in markdown report ([1661ed5](https://github.com/Canka76/milfoy-ui/commit/1661ed5f4b742a598ccae88778d2db06306f15af))
* **benchmark:** register BenchmarkResult_Serialization test in AutoTestRunner ([4f96695](https://github.com/Canka76/milfoy-ui/commit/4f9669503c7e9c3ea2adfd4ff1ad77d6b1c0a5d3))
* **benchmark:** tolerate Unity domain unload exit code when report artifacts exist ([df83476](https://github.com/Canka76/milfoy-ui/commit/df83476a1dc3ff8a6363d39d4ccbb47abeb029ac))
