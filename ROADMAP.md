# IoTSharp Roadmap

This roadmap records the current product direction for evolving IoTSharp from a device platform into a full industrial collection and operations platform.

## Product Direction

IoTSharp will evolve around a three-layer architecture:

1. **Access and collection layer**
   - Device access
   - Gateway and edge connectivity
   - Collection modeling and protocol templates
2. **Real-time rule layer**
   - Telemetry processing
   - Alarm and action rules
   - Data transformation and dispatch
3. **Operations and release layer**
   - OTA firmware delivery
   - Collector software package delivery
   - Configuration versioning and rollout
   - Batch release, gray rollout, rollback, and result confirmation

## Key Judgement

- The current direction is correct, but the platform is still at the stage of having a usable foundation rather than a fully productized industrial platform.
- Existing access, product, asset, and rule-chain capabilities should be retained and upgraded instead of rewritten.
- The next stage must make gateways, edge agents, collection configuration, and release management first-class platform capabilities.

## Domain Responsibilities

### Product
Product is the **technical template**.

It should own:
- device type definitions
- protocol and collection templates
- point models
- default attributes
- command templates
- software and firmware compatibility matrix

### Asset
Asset is the **business object and topology container**.

It should own:
- site
- workshop
- line
- area
- customer deployment topology
- business grouping and hierarchy

### Device
Device is the **runtime instance**.

It should own:
- identity
- online status
- telemetry and attributes
- alarms and commands
- relation to product, asset, and edge node

### Edge Node
Edge node is the **managed collection runtime**.

It should own:
- registration
- heartbeat
- version
- capabilities
- runtime status
- logs and diagnostics
- relation to Gateway, PiXiu, and future collection agents

### Rule Chain
Rule chain remains the **real-time data processing engine**.

It should focus on:
- ingest-time transformation
- routing
- enrichment
- alarm triggering
- telemetry and attribute publishing

It should not become the main engine for long-running OTA, approval, rollback, or large batch operations.

### Release Center
Release center is the **operations delivery domain**.

It should own:
- OTA packages
- collector software packages
- configuration templates and versions
- staged release plans
- rollout execution records
- rollback and confirmation workflow

## Planned Capability Areas

### Phase 1 - Edge and collection management foundation
Priority: highest

Goals:
- Make gateway and collection clients first-class platform entities
- Introduce unified edge node registration and heartbeat model
- Manage C# Gateway, PiXiu client, and future agents through a common platform contract
- Expose version, capability, health, status, and diagnostics metadata

Expected output:
- edge node domain model
- edge registration protocol
- heartbeat and capability reporting
- unified management UI entry
- project boundary agreement for Gateway and PiXiu integration

### Phase 2 - Collection modeling
Goals:
- Upgrade collection configuration from scripts and conventions into standard models
- Build protocol templates, connection templates, point templates, sample policies, and transform chains
- Support industrial mapping features such as endian handling, scaling, formulas, units, deadband, and downsampling
- Connect designer screens with runtime persistence and execution models

Expected output:
- collection template model
- point model and transform model
- runtime configuration persistence
- designer-to-runtime integration for Modbus, OPC UA, and gateway scenarios

### Phase 3 - Operations and release center
Goals:
- Add OTA firmware packages, collector software packages, and configuration release management
- Support staged rollout, gray release, rollback, and result confirmation
- Separate long-running operational orchestration from the existing rule engine

Expected output:
- release center domain model
- package and configuration version management
- rollout task orchestration
- delivery status and audit trail

### Phase 4 - Real-time rule enhancement
Goals:
- Keep the current FlowRule direction for real-time processing
- Add more standard nodes and industrial transforms to reduce script dependency
- Improve versioning, observability, simulation, and auditability
- Optionally integrate RulesEngine or NRules for richer decision logic

Expected output:
- richer standard node library
- lower-code transformation capability
- stronger rule-chain versioning and observability

### Phase 5 - Organization and ecosystem alignment
Goals:
- Clarify repository and product boundaries inside the organization
- Keep IoTSharp as the platform center, not a container for every edge implementation
- Coordinate release rhythm, API contracts, and ownership between related projects

Expected output:
- clear ownership map across IoTSharp and related repositories
- compatibility matrix for platform, Gateway, PiXiu, SDKs, and agents
- versioned contracts for registration, configuration, telemetry, and release tasks

## Project Relationship Strategy

### IoTSharp
The platform center.

Owns:
- tenant and device platform capabilities
- products and assets
- rule chain
- edge management control plane
- release center
- APIs, UI, permissions, audit, and lifecycle management

### Gateway
The protocol and southbound collection runtime.

Role:
- execute collection tasks
- adapt industrial protocols
- report runtime status and collection results to IoTSharp
- receive configuration and software tasks from IoTSharp

Rule:
- Gateway should integrate through stable contracts instead of re-embedding platform logic into the collector itself.

### PiXiu
The lightweight edge and site-side runtime.

Role:
- act as an edge agent where lightweight deployment or site autonomy is required
- handle local execution, buffering, diagnostics, and future edge-side coordination
- synchronize with IoTSharp through unified edge contracts

Rule:
- PiXiu should be treated as a managed edge runtime under the same edge management domain as Gateway, while preserving its independent deployment and lifecycle.

### Other organization projects
Related repositories should be aligned through explicit ownership and contracts.

Rules:
- platform concerns stay in IoTSharp
- protocol runtime concerns stay in Gateway or other collectors
- lightweight edge runtime concerns stay in PiXiu
- SDKs and agent projects follow platform contracts and compatibility matrices
- cross-project changes should be driven by versioned interfaces, not undocumented coupling

## Execution Principles

- Do not rewrite the existing platform wholesale.
- Preserve current device access, product mapping, asset hierarchy, and rule-chain investment.
- Add new domains in layers: edge management, collection modeling, release orchestration.
- Convert high-frequency script patterns into standard nodes or standard transformers.
- Use dedicated workflow/orchestration tooling for long-running delivery flows when needed.

## Immediate Next Step

Start with **Phase 1: edge and collection management foundation**.

The first implementation wave should define:
- edge node domain model
- Gateway and PiXiu registration contract
- heartbeat and capability reporting contract
- runtime status and diagnostics surface
- compatibility and ownership rules across related repositories
