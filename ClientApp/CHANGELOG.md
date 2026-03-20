# IoTSharp ClientApp Changelog

All notable changes to the IoTSharp frontend console should be documented in this file.

The format is inspired by Keep a Changelog and follows project-oriented release notes instead of template history.

## Unreleased

### Changed

- Continued removing `vue-next-admin` product-facing traces from the frontend experience
- Renamed the first-run route from `setup` to `installer`
- Reworked the installer screen to match the new login-page visual language

## 3.5.0

### Added

- New installer page under `/installer`
- New installer form resources for the first-run experience

### Changed

- Redesigned the dashboard with a more product-oriented IoT operations layout
- Redesigned the login experience with stronger branding and clearer entry flow
- Refined device detail pages for better information hierarchy and readability
- Replaced visible `vue-next-admin` branding with `IoTSharp` in key layout areas
- Updated development API behavior to use same-origin requests with Vite proxy forwarding

### Fixed

- Improved local frontend-to-backend connectivity in development
- Reduced the chance of browser-side API failures caused by hard-coded backend origins

## 3.4.x and Earlier

Earlier frontend history in this repository included imported template changelog content that no longer accurately represented IoTSharp product changes.

From this point forward, this changelog records IoTSharp ClientApp changes only.
