# IoTSharp.Contracts

IoTSharp.Contracts is the versioned contract package for IoTSharp platform and edge integration.

M2 #027 publishes these edge contracts as the current cloud-edge boundary:

- `edge-node-v1`: EdgeNode registration, heartbeat, capability, runtime status, and platform snapshot DTOs.
- `collection-config-v1`: collection runtime configuration and assignment snapshot DTOs.
- `edge-task-v1`: EdgeTask request, state, receipt, and state machine DTOs.

M2 #029 keeps `semantic-core-v1` as the semantic and protocol-binding basis for M3 Collection Template work. It covers protocol-neutral assets, semantic points, Modbus TCP/RTU, OPC UA, MQTT, and custom protocol bindings.

The package also includes JSON Schema files under `schemas/`, markdown design notes under `docs/`, and sample payloads under `examples/`.
