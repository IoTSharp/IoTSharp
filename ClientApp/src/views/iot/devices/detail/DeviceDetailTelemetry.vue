<template>
  <div class="z-crud">
    <fs-crud v-if="state.currentPageState === 'realtime'" ref="crudRef" v-bind="crudBinding"/>
    <div v-if="state.currentPageState === 'history'">
      <DeviceDetailTelemetryHistory
          :telemetry-keys="state.telemetryKeys"
          :deviceId="deviceId"></DeviceDetailTelemetryHistory>
    </div>
  </div>
</template>

<script setup lang="ts">
import {useCrud} from "@fast-crud/fast-crud";
import {useExpose} from "@fast-crud/fast-crud";
import {createDeviceTelemetryRealtimeCrudOptions} from "/@/views/iot/devices/detail/deviceTelemetryRealtimeCrudOptions";
import DeviceDetailTelemetryHistory from "/@/views/iot/devices/detail/DeviceDetailTelemetryHistory.vue";
import {getCurrentInstance} from "vue";
const {proxy} = <any>getCurrentInstance();
const state = reactive({
  currentPageState: 'realtime',
  telemetryKeys: []
})
const props = defineProps({
  deviceId: {
    type: String,
    default: ''
  }
})
// crud组件的ref
const crudRef = ref();
// crud 配置的ref
const crudBinding = ref();
// 暴露的方法
const {crudExpose} = useExpose({crudRef, crudBinding});
// 你的crud配置
let {crudOptions} = createDeviceTelemetryRealtimeCrudOptions({expose: crudExpose}, props.deviceId, state);
// 初始化crud配置
// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
const {resetCrudOptions} = useCrud({expose: crudExpose, crudOptions});
// 你可以调用此方法，重新初始化crud配置
// resetCrudOptions(options)
watch(() => props.deviceId, () => {
  // watch deviceId , 根据Device id 重新配置 crud， 再进行刷新
  const res =  createDeviceTelemetryRealtimeCrudOptions({expose: crudExpose}, props.deviceId, state);
  crudOptions = res.crudOptions
  resetCrudOptions(crudOptions)
  crudExpose.doRefresh();
})
// 页面打开后获取列表数据
onMounted( () => {
  crudExpose.doRefresh();
  proxy.mittBus.on('updateTelemetryPageSate', (pageSateName) => {
    state.currentPageState = pageSateName
  });
});
onUnmounted(()=>{
  proxy.mittBus.off('updateTelemetryPageSate', () => {
  });
})

</script>

<style scoped lang="scss">
.z-crud {
  height: calc(100vh - 160px);
}
</style>
