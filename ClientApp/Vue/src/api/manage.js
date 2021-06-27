import request from '@/utils/request'

const api = {
  user: '/user',
  role: '/role',
  service: '/service',
  permission: '/permission',
  permissionNoPager: '/permission/no-pager',
  orgTree: '/org/tree',
  addtenant: '/Tenants',
  gettenant: '/Tenants',
  tenantlist: '/Tenants',
  updateTenant: '/Tenants',
  deleteTenant: '/Tenants',
  customerlist: '/Customers/Tenant',
  getcustomer: '/Customers',
  addcustomer: '/Customers',
  updatecustomer: '/Customers',
  deletecustomer: '/Customers',
  devicelist: '/Devices/Customers',
  getdevice: '/Devices',
  adddevice: '/Devices',
  updatedevice: '/Devices',
  deletedevice: '/Devices',
}

export default api

export function getUserList(parameter) {
  return request({
    url: api.user,
    method: 'get',
    params: parameter,
  })
}

export function getRoleList(parameter) {
  return request({
    url: api.role,
    method: 'get',
    params: parameter,
  })
}

export function getServiceList(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function getPermissions(parameter) {
  return request({
    url: api.permissionNoPager,
    method: 'get',
    params: parameter,
  })
}

export function getOrgTree(parameter) {
  return request({
    url: api.orgTree,
    method: 'get',
    params: parameter,
  })
}

// id == 0 add     post
// id != 0 update  put
export function saveService(parameter) {
  return request({
    url: api.service,
    method: parameter.id === 0 ? 'post' : 'put',
    data: parameter,
  })
}

export function saveSub(sub) {
  return request({
    url: '/sub',
    method: sub.id === 0 ? 'post' : 'put',
    data: sub,
  })
}

export function getTenantList(parameter) {
  return request({
    url: api.tenantlist,
    method: 'get',
    params: parameter,
    headers: {
      'Content-Type': 'application/json;charset=UTF-8',
    },
  })
}

export function getTenant(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function addTenant(parameter) {
  return request({
    url: api.service,
    method: 'post',
    params: parameter,
  })
}

export function updateTenant(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function deleteTenant(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function getDeviceList(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function addDevice(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function updateDevice(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function deleteDevice(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function getCustomerList(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function addCustomer(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function updateCustomer(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}

export function deleteCustomer(parameter) {
  return request({
    url: api.service,
    method: 'get',
    params: parameter,
  })
}
