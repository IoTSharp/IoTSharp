# IoTSharp ClientApp

IoTSharp ClientApp is the web console for the IoTSharp platform. It is used for device access management, telemetry visualization, rule chain operations, tenant collaboration, and system setup.

This frontend is now maintained as an IoTSharp application, not as a downstream documentation copy of any admin template.

## Overview

IoTSharp is an open-source IoT platform focused on:

- Device connectivity and lifecycle management
- Telemetry, attributes, alarms, and event visibility
- Rule engine and automation orchestration
- Multi-tenant operations and system administration
- Productized delivery for real-world industrial and enterprise scenarios

## Tech Stack

- Vue 3
- TypeScript
- Vite
- Pinia
- Vue Router
- Element Plus
- ECharts / X6 / Monaco Editor

## Development Goals

The current frontend direction is:

- Keep the engineering base simple and easy to evolve
- Replace template branding and template-oriented UX with IoTSharp product language
- Build a more modern, immersive, and AI-friendly frontend structure
- Preserve compatibility with the existing backend and business modules

## Requirements

- Node.js 16+
- npm 7+

Recommended browsers:

| Browser | Version |
| --- | --- |
| Edge | 79+ |
| Firefox | 78+ |
| Chrome | 64+ |
| Safari | 12+ |

## Getting Started

```bash
cd ClientApp
npm install --legacy-peer-deps
npm run dev
```

Default local development behavior:

- Frontend dev server: `http://localhost:8888`
- Backend API: `http://localhost:5000`
- In development, API requests are forwarded through the Vite proxy

## Available Scripts

```bash
npm run dev
npm run build
npm run lint-fix
```

## Environment

Important environment files:

- [`.env`](d:/GitHub/IoTSharp/ClientApp/.env)
- [`.env.development`](d:/GitHub/IoTSharp/ClientApp/.env.development)
- [`.env.production`](d:/GitHub/IoTSharp/ClientApp/.env.production)

Development uses same-origin API requests by default, then proxies `/api` traffic to the backend target configured in `.env.development`.

## Project Structure

```text
ClientApp/
  public/             Static assets
  src/
    api/              API wrappers
    components/       Shared UI building blocks
    layout/           App shell and navigation layout
    router/           Route registration and guards
    stores/           Pinia stores
    utils/            Frontend utilities
    views/            Product pages and feature screens
  vite.config.ts      Vite config and dev proxy
```

## Current UX Direction

Recent iterations are focused on:

- Dashboard redesign
- Login and installer experience redesign
- Device detail information density and readability improvements
- Full migration of visible product branding to `IoTSharp`

## Build

```bash
npm run build
```

The production output is generated in `ClientApp/dist`.

## Backend Coordination

This frontend depends on the IoTSharp backend APIs. For local development, make sure the backend is available before testing login, installer, telemetry, and dashboard data flows.

Typical local backend address:

- `http://localhost:5000`

## Repository

- Main repository: <https://github.com/IoTSharp/IoTSharp>
- Community and support: <https://github.com/IoTSharp/IoTSharp#community-support>

## License

Apache-2.0
