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

### Changed

- Reworked the documentation site information architecture, homepage, blog presentation, and doc detail layout.
- Renamed `setup` user-facing flows and docs toward `installer`.
- Standardized release-oriented GitHub workflows around tag-based artifacts, packages, and Docker image publication.
- Updated frontend branding from template-origin wording to `IoTSharp`.
- Changed the frontend local development default port to `27915`.
- Improved GitHub Pages workflow and upgraded Docusaurus to the current 3.x line used in the repository.

### Fixed

- Stabilized CodeQL configuration by removing fragile autobuild assumptions for the mixed .NET and frontend repository layout.
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
