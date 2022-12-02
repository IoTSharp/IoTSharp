<template>
  <div class="z-crud">
    <fs-crud ref="crudRef" v-bind="crudBinding"/>
  </div>
</template>

<script lang="ts">
import {useCrud} from "@fast-crud/fast-crud";
import {useExpose} from "@fast-crud/fast-crud";
import {createTenantListCrudOptions} from "./crudOptions/tenantListCrudOptions"
export default defineComponent({
  name: "TenantList", // 实际开发中可以修改一下name
  setup() {
    // crud组件的ref
    const crudRef = ref();
    // crud 配置的ref
    const crudBinding = ref();
    // 暴露的方法
    const {crudExpose} = useExpose({crudRef, crudBinding});
    // 你的crud配置
    const {crudOptions} = createTenantListCrudOptions({crudExpose});
    // 初始化crud配置
    // eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
    const {resetCrudOptions} = useCrud({crudExpose, crudOptions});
    // 你可以调用此方法，重新初始化crud配置
    // resetCrudOptions(options)
    // 页面打开后获取列表数据
    onMounted(() => {
      crudExpose.doRefresh();
    });
    return {
      crudBinding,
      crudRef,
    };
  },
});
</script>

<style scoped lang="scss">
.z-crud {
  height: calc(100vh - 160px);
}
</style>
