# AGENTS.md

This file records the current execution plan for contributors and automation working in this repository.

## Current Mission

IoTSharp is moving from a device platform to a platform with three coordinated layers:

1. access and collection
2. real-time rules
3. operations and release

All planning and implementation should support that direction.

## Current Priority Order

1. Build the edge and collection management foundation
2. Standardize collection modeling
3. Introduce release center for OTA, software updates, and configuration rollout
4. Upgrade rule-chain usability and observability without turning it into a long-running workflow engine
5. Align repository boundaries and contracts across organization projects

## AI Workbench Direction

AI is a cross-cutting capability for the three existing layers, not a separate rewrite track.

- Build AI on top of the IoTSharp control plane, permissions, audit, UI, and domain services.
- Prefer the latest Microsoft Agent Framework for agent orchestration.
- Prefer Microsoft.Extensions.AI for model/provider abstraction.
- Keep MCP as the primary tool boundary for exposing IoTSharp capabilities and integrating external tools.
- Treat Camel.NET integration as capability absorption for multi-model access, tool calling, memory, and multi-agent collaboration.
- Keep local model runtimes, speech services, and companion/desktop experiences deployable outside the main web host.

## Domain Boundary Rules

### IoTSharp
Owns:
- platform control plane
- tenants, products, assets, devices
- rule-chain runtime for real-time processing
- edge node management
- release center and lifecycle management
- APIs, UI, permissions, observability, and audit

### Gateway
Owns:
- southbound protocol execution
- collection runtime behavior
- protocol adapters and industrial connectivity
- reporting collection state and results back to IoTSharp

Gateway must integrate with IoTSharp through stable contracts for:
- registration
- heartbeat
- capability report
- configuration pull or push
- software and task delivery feedback

### PiXiu
Owns:
- lightweight edge runtime responsibilities
- site-side execution, buffering, diagnostics, and future edge autonomy
- synchronization with the IoTSharp control plane

PiXiu should follow the same edge management contracts as Gateway where possible, while remaining an independent deployable project.

### Other organization projects
Rules:
- do not move unrelated responsibilities into IoTSharp
- do not couple projects through hidden internal assumptions
- introduce versioned contracts before introducing cross-repository features
- maintain compatibility matrices for platform, Gateway, PiXiu, SDKs, and related agents

## Planning Rules

When adding or reviewing work, prioritize the following outcomes:

### Phase 1: edge foundation
Deliver:
- edge node model
- registration and heartbeat flow
- capability and status reporting
- unified management surface for Gateway and PiXiu
- AI-assisted edge diagnostics and operator guidance should plug into this phase through read-only skills first

### Phase 2: collection modeling
Deliver:
- protocol templates
- connection templates
- point templates
- sample policies and transform chains
- runtime persistence linked to designer configuration
- AI should assist with mapping suggestions, protocol configuration hints, and transform generation without bypassing review

### Phase 3: release center
Deliver:
- OTA package model
- collector software package model
- configuration template and version model
- rollout, gray release, rollback, and confirmation flow
- AI should help with release grouping, gray rollout planning, rollback suggestions, and operational Q&A

### Phase 4: rule-chain enhancement
Deliver:
- more standard nodes
- less script dependency
- better versioning, simulation, observability, and audit
- AI should explain rules, generate safe drafts, and support diagnostics without turning rule chains into long-running workflow engines

## Implementation Constraints

- Do not propose a full rewrite.
- Preserve existing access, product, asset, and FlowRule investments.
- Use layered upgrades.
- Keep real-time processing and long-running operations separated.
- Prefer explicit domain models over script-only behavior.
- Expose IoTSharp capabilities to AI through authorized skills and MCP tools, not direct database access.
- Require tenant isolation, audit trails, and human confirmation for higher-risk AI actions.

## Immediate Execution Focus

The next implementation step starts with edge management.

Any first-wave design or code change should help define:
- how Gateway and PiXiu register
- how IoTSharp tracks edge runtime state
- how configuration and release tasks are addressed to an edge runtime
- how cross-project responsibilities are kept clear inside the organization
- how AI workbench capabilities can consume these contracts through governed skills instead of hidden coupling
