<template>
  <div>
    <el-drawer v-model="drawer" :title="dialogTitle" size="70%">
      <el-tabs v-model="activeTabName" class="demo-tabs" stretch>
        <el-tab-pane label="详情" name="detail">
          <div class="z-tab-container">
            <AdvancedKeyValue
                :obj="deviceRef"
                :config="columns"
                :hide-key-list="['owner', 'identityValue', 'tenantName', 'customerName', 'tenantId', 'customerId', 'identityId']"
                :label-width="160"></AdvancedKeyValue>
          </div>
        </el-tab-pane>
        <el-tab-pane label="属性" name="second">Config</el-tab-pane>
        <el-tab-pane label="遥测" name="third">Role</el-tab-pane>
        <el-tab-pane label="告警" name="fourth">Task</el-tab-pane>
        <el-tab-pane label="规则" name="fourth">Task</el-tab-pane>
      </el-tabs>
    </el-drawer>
  </div>
</template>
<script lang="ts" setup>
import {createDeviceCrudOptions} from "/@/views/iot/devices/deviceCrudOptions";
import AdvancedKeyValue from "/@/components/AdvancedKeyValue/AdvancedKeyValue.vue";

const drawer = ref(false);
const dialogTitle = ref(`设备详情`);
const activeTabName = ref('detail')
const deviceRef = ref()
const {crudOptions: { columns }} = createDeviceCrudOptions({expose: null})
const openDialog = (device:any)=>{
  drawer.value = true
  deviceRef.value = device
  dialogTitle.value = `${deviceRef.value.name}设备详情`
};
defineExpose({
  openDialog,
});

</script>
<style lang="scss" scoped>

</style>
