<template>
  <div class="z-crud">
    <fs-crud v-if="props.deviceId" ref="crudRef" v-bind="crudBinding"/>
  </div>
</template>

<script setup lang="ts">
import {useCrud} from "@fast-crud/fast-crud";
import {useExpose} from "@fast-crud/fast-crud";
import {createDeviceTelemetryRealtimeCrudOptions} from "/@/views/iot/devices/detail/deviceTelemetryRealtimeCrudOptions";
import {getCurrentInstance, nextTick} from "vue";
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
watch(() => props.deviceId, async () => {
  if (!props.deviceId) return;

  // watch deviceId , 根据Device id 重新配置 crud， 再进行刷新
  const res =  createDeviceTelemetryRealtimeCrudOptions({expose: crudExpose}, props.deviceId, state);
  crudOptions = res.crudOptions
  resetCrudOptions(crudOptions)
  await nextTick();
  crudExpose.doRefresh();
}, { immediate: true })
// 订阅遥测状态事件，避免在没有设备 ID 时触发请求。
onMounted(() => {
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
  min-height: var(--device-detail-pane-height, calc(100vh - 160px));
}
</style>
