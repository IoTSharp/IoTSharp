# Product MQTT Simulator

Simulates product-token device registration over MQTT and sends telemetry to IoTSharp.

## Create a product, register a device through MQTT, and verify telemetry

```powershell
dotnet run --project tools/ProductMqttSimulator -- `
  --api-base http://10.165.83.194:2927 `
  --bearer "<jwt>" `
  --mqtt-host 10.165.83.194 `
  --mqtt-port 1883
```

## Use an existing product token

```powershell
dotnet run --project tools/ProductMqttSimulator -- `
  --produce-token "<product-token>" `
  --device-name sim-device-001 `
  --mqtt-host 10.165.83.194 `
  --mqtt-port 1883
```

By default the simulator publishes to `devices/me/telemetry`, which targets the
MQTT-authenticated product device. Use `--topic-device-name <child-name>` when
you intentionally want to simulate a gateway child device.

When `--bearer` is provided, the simulator also polls IoTSharp APIs to confirm
that the product-created device exists and the latest telemetry value is visible.
