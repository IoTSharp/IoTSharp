<template>
  <div>
    <el-drawer v-model="drawer" :title="dialogTitle" size="70%">
      <el-tabs v-model="activeTabName" class="demo-tabs" stretch>
        <el-tab-pane label="详情" name="basic">
          <div class="z-tab-container">
            <AdvancedKeyValue
                :obj="deviceRef"
                :config="columns"
                :hide-key-list="['owner', 'identityValue', 'tenantName', 'customerName', 'tenantId', 'customerId']"
                :label-width="160">
              <template #footer v-if="deviceRef.identityType==='X509Certificate'">
                <div class="z-row">
                  <div class="z-key" style="width: 160px">证书:</div>
                  <div class="z-value"><ElButton @click="downloadCert">下载证书</ElButton></div>
                </div>
              </template>
            </AdvancedKeyValue>

          </div>
        </el-tab-pane>
        <el-tab-pane label="属性" name="props">
          <div class="z-tab-container">
            <DeviceDetailProps :deviceId="deviceRef.id"></DeviceDetailProps>
          </div>

        </el-tab-pane>
        <el-tab-pane label="遥测" name="telemetry">
          <div class="z-tab-container">
            <DeviceDetailTelemetry :deviceId="deviceRef.id"></DeviceDetailTelemetry>
          </div>
        </el-tab-pane>
        <el-tab-pane label="遥测历史" name="telemetryHistory" :lazy="true">
          <div class="z-tab-container">
            <DeviceDetailTelemetryHistory :deviceId="deviceRef.id"></DeviceDetailTelemetryHistory>
          </div>
        </el-tab-pane>
        <el-tab-pane label="告警" name="alarm">
          <div class="z-tab-container">
            <alarmlist :originator="deviceRef" wrapper="div"></alarmlist>
          </div>
        </el-tab-pane>
        <el-tab-pane label="规则" name="rules">
          <div class="z-tab-container">
            <DeviceDetailRules :deviceId="deviceRef.id"></DeviceDetailRules>
          </div>
        </el-tab-pane>
        <el-tab-pane label="规则历史" name="rulesHistory">
          <div class="z-tab-container">
            <flowevents :creator="deviceRef.id" :creatorname="deviceRef.name"  wrapper="div"></flowevents>
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-drawer>
  </div>
</template>
<script lang="ts" setup>
import { deviceDetailBaseInfoColumns } from '/@/views/iot/devices/detail/deviceDetailBaseInfoColumns'
import AdvancedKeyValue from "/@/components/AdvancedKeyValue/AdvancedKeyValue.vue";
import DeviceDetailProps from "/@/views/iot/devices/detail/DeviceDetailProps.vue";
import DeviceDetailRules from "/@/views/iot/devices/detail/DeviceDetailRules.vue";
import DeviceDetailTelemetry from "/@/views/iot/devices/detail/DeviceDetailTelemetry.vue";
import DeviceDetailTelemetryHistory from "/@/views/iot/devices/detail/DeviceDetailTelemetryHistory.vue";
import Alarmlist from "/@/views/iot/alarms/alarmlist.vue";
import Flowevents from "/@/views/iot/rules/flowevents.vue";
import { deviceApi } from "/@/api/devices";
import { downloadByData } from "/@/utils/download";

const drawer = ref(false);
const dialogTitle = ref(`设备详情`);
const activeTabName = ref('basic')
const deviceRef = ref()
const columns = deviceDetailBaseInfoColumns
const openDialog = (device: any) => {
  drawer.value = true
  deviceRef.value = Object.assign({},device )
  // Object.assign(deviceRef, device)
  dialogTitle.value = `${deviceRef.value.name}设备详情`
};
const downloadCert = async () =>{
  const res = await deviceApi().downloadCertificates(deviceRef.value.id)
  downloadByData(res, `${deviceRef.value.id}.zip`)
}
defineExpose({
  openDialog,
});

</script>
<style lang="scss" scoped>
.z-tab-container {
  padding: 0 16px 16px;
}
</style>
