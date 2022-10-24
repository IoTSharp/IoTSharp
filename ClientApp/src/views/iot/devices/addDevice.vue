<template>
  <el-form :model="dataForm" size="default" label-width="90px">
    <el-row :gutter="35">
      
      <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
        <el-form-item label="设备名称">
          <el-input
            v-model="dataForm.name"
            placeholder="请输入设备名称"
            clearable
          ></el-input>
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
        <el-form-item label="设备类型">
          <el-input v-model="dataForm.deviceType" placeholder="请选择设备类型" clearable></el-input>
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
        <el-form-item label="超时">
          <el-input
            v-model="dataForm.timeout"
            placeholder="请输入超时"
            clearable
          ></el-input>
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
        <el-form-item label="认证方式">
          <el-input
            v-model="dataForm.identityType"
            placeholder="请选择认证方式"
            clearable
          ></el-input>
        </el-form-item>
      </el-col>
    </el-row>
  </el-form>
</template>

<script lang="ts">
import {
  ref,
  toRefs,
  reactive,
  onMounted,
  defineComponent,
  defineProps,
  watchEffect,
} from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { deviceApi } from "/@/api/devices";

interface deviceform {
  dataForm: deviceaddoreditdto;
  identityType: Array<string>;
  deviceType: Array<string>;
}

export default defineComponent({
  props: ['deviceid'],
  name: 'addDevice',
  components: {},
  setup(props) {
    const state = reactive<deviceform>({
      identityType: [],
      deviceType: [],
      dataForm: {
        id: '',
        name: '',
        timeout: 300,
        identityType: '',
        deviceType: '',
      },
    });

    var deviceid = '';

    watchEffect(() => {
      deviceid = props.deviceid;
      if (deviceid === '0000000-0000-0000-0000-000000000000') {
      } else {
        deviceApi()
          .getdevcie(deviceid)
          .then((res) => {});
      }
    });

    onMounted(() => {



    });
    return {	...toRefs(state),};
  },
});
</script>
