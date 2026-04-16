# IoTSharp NuGet Packages

This package is part of the IoTSharp delivery set.

## What you get

- Shared building blocks used by the IoTSharp platform and SDKs
- Packages versioned together with Git tags such as `v1.0.0`
- Package publishing to both NuGet.org and GitHub Packages

## Common usage flow

1. Install the package from NuGet.
2. Review the package-specific API surface in the source directory that matches the package name.
3. Follow the platform and deployment documentation at <https://iotsharp.net/>.

## Package map

| Package | Purpose |
| --- | --- |
| `IoTSharp` | Main IoTSharp application package and deployment assets |
| `IoTSharp.Data.JsonDB` | Query JSON payloads with SQL through an ADO.NET provider |
| `IoTSharp.Sdk.Http` | Access IoTSharp APIs over HTTP |
| `IoTSharp.Sdk.MQTT` | Connect devices and services to IoTSharp over MQTT |
| `IoTSharp.Extensions.*` | Reusable extension packages for ASP.NET Core, DI, EF Core, Quartz, REST, and crypto features |

## Documentation

- Product docs: <https://iotsharp.net/>
- Deployment guide: <https://iotsharp.net/reference/release-automation/>
- SDK notes: `/home/runner/work/IoTSharp/IoTSharp/IoTSharp.SDKs`
