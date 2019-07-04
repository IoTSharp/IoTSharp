import request from '@/utils/request'

// Get all of the customer's devices.
export function getDevices(id) {
  return request({
    url: '/Devices/Customers/' + id,
    method: 'get'
  })
}

// Get a device's detail
export function getDevice(id) {
  return request({
    url: '/Devices/' + id,
    method: 'get'
  })
}

// Get a device's credentials
export function GetIdentity(id) {
  return request({
    url: '/Devices/' + id,
    method: 'get'
  })
}

// Create a new device
export function creatDevice(data) {
  return request({
    url: '/Devices/',
    method: 'post',
    data
  })
}

// DELETE: api/Devices/5
export function deleteDevice(data) {
  return request({
    url: '/Devices',
    method: 'delete',
    data
  })
}
// for pagin usage,current not support
export function getDevicePv(pv) {
  return request({
    url: '/Devices/pv',
    method: 'get',
    params: { pv }
  })
}
// modify device
export function updateDevice(id, data) {
  return request({
    url: `/Devices/${id}`,
    method: 'put',
    data
  })
}
// Get a device's credentials
export function getDeviceAccessToken(id) {
  return request({
    url: `/Devices/${id}/Identity`,
    method: 'get'
  })
}
// Request attribute values by device access token from the server
export function getDeviceAttributes(id) {
  return request({
    url: `/Devices/${id}/AttributeLatest`,
    method: 'get'
  })
}

// Request telemetry values  by device access token from the server
export function getDeviceTelemetryLatest(id) {
  return request({
    url: `/Devices/${id}/TelemetryLatest`,
    method: 'get'
  })
}

