---
sidebar_position: 1
title: Release Distribution Plan
---

# Release Distribution Plan

## Current round

This round standardizes GitHub Actions around a single release trigger: pushing a Git tag.

- NuGet packages are published only on tag pushes.
- Application release assets are built only on tag pushes and uploaded to GitHub Releases.
- Docker images are published only on tag pushes and pushed to both `ghcr.io` and Docker Hub.

This keeps branch CI focused on validation and makes every published artifact traceable to a released version.

## What ships now

The current automation produces these release outputs:

- `IoTSharp` NuGet package from the main application project.
- Self-contained runtime archives for:
  - `win-x64`
  - `linux-x64`
  - `linux-arm`
  - `linux-arm64`
  - `linux-loongarch64`
  - `osx-x64`
- Multi-arch container images for:
  - `linux/amd64`
  - `linux/arm64`

At this stage, GitHub Releases carry archive assets, not native installers such as MSI, `.deb`, or `.rpm`.

## Assessment

### Linux packaging

Native Linux packages are worth doing, but they should be phased:

1. Prioritize `.deb`.
2. Add `.rpm` after `.deb` is stable.

Why `.deb` first:

- Debian and Ubuntu are the most common base systems for self-hosted .NET and Docker-adjacent workloads.
- The project already has a Docker and Linux-first deployment posture, so `.deb` gives the fastest path to real operator value.
- A `.deb` pipeline is enough to validate service installation, upgrade behavior, config preservation, and post-install scripts before duplicating the same lifecycle for RPM ecosystems.

Why `.rpm` should be second:

- RPM support is still valuable for RHEL, Rocky, AlmaLinux, and openSUSE families.
- It adds extra packaging and service validation work, so it is better treated as a follow-on package target once the Debian flow is proven.

### Windows installer

Windows should eventually have a real installer.

For IoTSharp, a Windows installer is more useful than a bare ZIP when the product is deployed by administrators who expect:

- service registration
- Start Menu shortcuts
- install/uninstall entries
- upgrade-safe configuration handling
- prerequisite checks

Recommended direction:

- Prefer MSI as the long-term primary Windows installer format.
- Use WiX as the packaging toolchain.

### WinGet

WinGet should be added after the Windows installer is stable, not before.

Reason:

- WinGet works best when the application already has a durable installer and versioned public download URL.
- The WinGet submission flow depends on a package manifest and repository submission process, so a stable GitHub Release asset layout should exist first.

## Release channels

The recommended release channels are:

- GitHub Release: canonical download page for archives and future native installers.
- NuGet.org: distribution channel for the .NET package.
- GitHub Packages: internal or GitHub-native package distribution mirror.
- GHCR: canonical container image for GitHub-centric deployments.
- Docker Hub: broader container distribution channel.
- WinGet: phase 2, after MSI release assets are stable.

## Phase plan

### Phase 1: completed in this round

- Restrict NuGet publishing to tag pushes.
- Restrict binary release asset publishing to tag pushes.
- Restrict Docker publishing to tag pushes.
- Publish Docker images to both GHCR and Docker Hub.

### Phase 2: native installer foundation

- Create a Windows installer project with WiX.
- Define installer behaviors for service registration, upgrade path, config retention, and uninstall cleanup.
- Produce versioned MSI assets in GitHub Releases.
- Standardize release asset naming and checksums.

### Phase 3: Linux native packages

- Create Debian packaging scripts and metadata.
- Install the app as a system service with systemd.
- Define post-install, upgrade, and config migration behavior.
- Add `.rpm` generation once the Debian package flow is stable.

### Phase 4: package manager publishing

- Add WinGet manifest generation and validation.
- Automate submission updates after MSI assets are released.
- Evaluate whether `.deb` should also be published to an APT repository and whether `.rpm` should later be published to a YUM/DNF repository.

## Required secrets and prerequisites

The new tag-based release workflows expect these repository secrets:

- `NUGET_API_KEY`
- `DOCKERHUB_USERNAME`
- `DOCKERHUB_TOKEN`

Recommended repository settings:

- protect the default branch
- reserve semantic version tags for releases
- make GitHub Releases the single source of truth for downloadable assets

## Next implementation tasks

The next engineering tasks should be executed in this order:

1. Design a Windows installer project using WiX and define service installation behavior.
2. Add checksum generation and checksum files to GitHub Release assets.
3. Define Debian packaging structure and systemd service layout.
4. Add RPM packaging after the Debian package is validated.
5. Add WinGet manifests and submission automation once MSI assets are stable.
