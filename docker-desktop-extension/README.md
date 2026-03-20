# IoTSharp Docker Desktop Extension

This folder contains the first Docker Desktop extension package for IoTSharp.

## Goals

- Ship a one-click local preview for Docker Desktop users
- Start IoTSharp with SQLite automatically through an embedded compose stack
- Provide a clean packaging foundation for Docker Hub and GHCR distribution

## Local build

```powershell
pwsh ./docker-desktop-extension/build-extension.ps1 -ImageName iotsharp/iotsharp-dd-extension:0.1.0
docker extension install iotsharp/iotsharp-dd-extension:0.1.0
```

## Runtime layout

- UI metadata lives in `metadata.json`
- The embedded compose stack lives in `vm/extension-compose.yml`
- The extension dashboard UI lives in `ui/`
- The IoTSharp runtime is pre-published into `build/publish/` before the image build
