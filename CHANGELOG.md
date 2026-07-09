# Changelog

All notable changes to this repository will be documented in this file.

This project follows a lightweight Keep a Changelog style. Version numbers in the application currently indicate `3.5.0`; earlier historical releases have not yet been fully backfilled into this file.

## [Unreleased]

### Added

- Docker Desktop extension skeleton and local SQLite demo flow.
- OpenClaw-oriented SQLite setup runbook and reusable prompt text.
- Custom Docusaurus theme wrappers for the docs landing page, blog list page, blog post page, and doc detail headers.
- Windows installer work for database-aware setup flow and service registration foundations.
- `IoTSharp.Agent` project for tray-style desktop integration work.
- `collection-config-v1` now carries Product Collection Template source metadata so edge runtimes can trace generated collection configurations back to Product templates.
- Platform-side collection configuration version snapshots with `CollectionConfigVersions`, version query APIs, and active assignment linkage for M4 #040.
- Edge/Gateway target collection configuration pull result with assignment, version ID, configuration hash, and `LastPulledAt` tracking for M4 #042.
- Configuration execution receipts for M4 #043 now validate reported version/hash and update assignment execution/applied state.
- Collection version status display for M4 #044 now exposes current/target versions, difference summary, and recent publish result in Edge APIs and console views.
- EdgeTask failure retry and audit support for M4 #045, including `POST /api/EdgeTask/{taskId}/Retry`, `GET /api/EdgeTask/{taskId}/Audit`, terminal receipt audit logs, and task timeline audit events.
- Minimal software package publication for M4 #046 with `ReleasePackage`, multi-provider `ReleasePackages` migrations, upload/download APIs, `SoftwareUpdate` EdgeTask dispatch, checksum-bound receipts, and `release-package-v1` contract artifacts.
- IoTEdge-side M4 #047/#048 execution support is now aligned with platform contracts: `SoftwareUpdate` tasks download, verify, stage, switch and receipt package results, while upload channels can buffer failed collection uploads locally and replay them after connectivity recovers.
- Release Center platform first version for M5 #050-#055 with `ReleasePlan`, `ReleaseTask`, `ReleaseReceipt`, multi-provider migrations, `/api/ReleaseCenter/Plans` APIs, Gateway/EdgeNode runtime software rollout, gray batches, confirmation, pause/resume, rollback tasks, receipt projection, and audit logging.
- Device script and firmware OTA execution-end contract groundwork for M5 #056/#057 with `DeviceScriptOta` and `FirmwareOta` EdgeTask types, `Device` task targets, AssetScope/DeviceScope expansion into per-device release tasks, checksum-bound package receipts, and EdgeTask/ReleasePackage contract samples for IoTEmbedded alignment.
- M5 #056 device script OTA now validates successful execution receipts against task-declared `scriptCrc32` and covers DeviceScope delivery through a Gateway channel to the child Device runtime target.
- M5 #057 firmware OTA now validates bootloader acceptance and rollback readiness/confirmation in successful `FirmwareOta` receipts, including Gateway-channel DeviceScope rollout and rollback plan coverage.
- #072 cloud-edge same-base reference deployment with `Deployments/cloud_edge_sonnetdb`, Docusaurus/static docs, SonnetDB cloud/edge compose templates, contract-boundary guidance, validation, and rollback notes.
- #09A CoAP performance and load-test tooling with `tools/IoTSharp.Benchmarks`, BenchmarkDotNet coverage for path matching and payload parsing, plus a real UDP CoAP load runner.
- #09B CoAP documentation closure with platform access instructions, old/new path differences, component consumption/version notes, rollback paths, and operations configuration kept in IoTSharp docs.

### Changed

- Reworked the documentation site information architecture, homepage, blog presentation, and doc detail layout.
- Renamed `setup` user-facing flows and docs toward `installer`.
- Standardized release-oriented GitHub workflows around tag-based artifacts, packages, and Docker image publication.
- Updated frontend branding from template-origin wording to `IoTSharp`.
- Changed the frontend local development default port to `27915`.
- Improved GitHub Pages workflow and upgraded Docusaurus to the current 3.x line used in the repository.
- Closed the M3 Collection Template milestone after IoTEdge execution-chain validation for template-generated runtime configurations.
- Closed the M4 platform milestone in the roadmap and moved M5 into active execution with Device script and firmware OTA remaining as follow-up execution-end work.
- Closed the SonnetDB single-dependency narrative track after adding the cloud-edge same-base reference deployment.
- Optimized CoAP business payload parsing to consume UTF-8 payload bytes directly and use source-generated metadata for alarm payloads.
- Closed #09B by documenting that legacy CoAP short paths are incompatible, MQTT is outside this CoAP route track, and CoAP.NET package/version/rollback details live in IoTSharp CoAP access docs.
- Closed #09C CoAP operations/security hardening with configurable listen boundaries, explicit DTLS PSK enablement, default `5684/udp` exposure removal, unified unauthorized token handling, host logging integration, and startup/port-conflict/security mapping regression coverage.
- Closed #09D CoAP dependency/version alignment by removing the unused stale `IoTSharp.CoAP.NET` 2.0.8 central package version and documenting the 3.0.0 fork consumption, NuGet release, and rollback paths.

### Fixed

- Stabilized CodeQL configuration by removing fragile autobuild assumptions for the mixed .NET and frontend repository layout.
- Removed obsolete AppVeyor references from repository documentation.
- Adjusted data provider configuration so pending EF model changes do not crash startup in SQLite demo/bootstrap scenarios.
- Improved Docker Desktop extension local build validation behavior for non-Marketplace development flows.

## [3.5.0] - 2026-03-20

### Added

- SQLite-first installer and AI-assisted onboarding documentation.
- Docker Desktop extension support for local trial experiences.
- Richer documentation site visuals and GitHub Pages deployment pipeline.
- Release packaging workflows for binaries, NuGet packages, and Docker images triggered by tags.
- Windows installer and desktop agent project foundations.

### Changed

- Frontend application identity, layout language, and documentation branding were aligned to `IoTSharp`.
- Installer experience and naming were updated to better match first-run initialization.
- Repository documentation was reorganized around product navigation, deployment, and operations.

### Fixed

- Login slide captcha was enforced as a real server-side flow.
- CI workflow versions and action references were refreshed across the repository.

## Historical Note

Older releases exist in Git history and GitHub Releases, but they have not yet been fully normalized into this changelog. Future updates should extend this file forward from the current documented baseline.
