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

### Changed

- Reworked the documentation site information architecture, homepage, blog presentation, and doc detail layout.
- Renamed `setup` user-facing flows and docs toward `installer`.
- Standardized release-oriented GitHub workflows around tag-based artifacts, packages, and Docker image publication.
- Updated frontend branding from template-origin wording to `IoTSharp`.
- Changed the frontend local development default port to `27915`.
- Improved GitHub Pages workflow and upgraded Docusaurus to the current 3.x line used in the repository.
- Closed the M3 Collection Template milestone after IoTEdge execution-chain validation for template-generated runtime configurations.

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
