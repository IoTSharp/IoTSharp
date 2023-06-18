<template>
  <div>
    <el-drawer v-model="state.drawer" :title="state.dialogtitle" size="75%">
      <div class="add-form-container">
        <el-form
          :model="state.dataForm"
          size="default"
          :rules="rules"
          label-width="150px"
          ref="dataFormRef"
        >
          <el-row :gutter="35">
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="产品名称" prop="name">
                <el-input
                  v-model="state.dataForm.name"
                  placeholder="请输入产品名称"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="产品图标" prop="icon">
                <!-- <el-input
                  v-model="state.dataForm.icon"
                  placeholder="请输入产品icon"
                  clearable
                ></el-input> -->
                <IconSelector v-model="state.dataForm.icon" />
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="默认网关类型" prop="gatewayType">
                <el-select
                  v-model="state.dataForm.gatewayType"
                  placeholder="请选择默认网关类型"
                >
                  <el-option
                    v-for="item in state.gatewayTypes"
                    :key="item"
                    :label="item"
                    :value="item"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col
              :xs="24"
              :sm="24"
              :md="24"
              :lg="24"
              :xl="24"
              class="mb20"
              v-if="state.dataForm.gatewayType === 'Customize'"
            >
              <el-form-item label="默认网关配置">
                <!-- <el-input
                  v-model="dataForm.gatewayConfigurationJson"
                  placeholder="请输入超时"
                  clearable
                ></el-input> -->

                <monaco
                  height="300px"
                  @change="oneditorchange"
                  width="80%"
                  theme="vs-dark"
                  v-model="state.dataForm.gatewayConfigurationJson"
                  language="json"
                  selectOnLineNumbers="true"
                ></monaco>
              </el-form-item>
            </el-col>

            <el-col
              :xs="24"
              :sm="24"
              :md="24"
              :lg="24"
              :xl="24"
              class="mb20"
              v-if="
                state.dataForm.gatewayType !== 'Unknow' &&
                state.dataForm.gatewayType !== 'Customize'
              "
            >
              <el-form-item label="默认网关配置名称">
                <el-input
                  v-model="state.dataForm.gatewayConfigurationName"
                  placeholder="请输入默认网关配置名称"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="默认产品类型" prop="defaultDeviceType">
                <el-select
                  v-model="state.dataForm.defaultDeviceType"
                  placeholder="请选择认证方式"
                >
                  <el-option
                    v-for="item in state.deviceTypes"
                    :key="item"
                    :label="item"
                    :value="item"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="默认超时" prop="defaultTimeout">
                <el-input
                  v-model="state.dataForm.defaultTimeout"
                  placeholder="请输入默认超时"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="认证方式" prop="defaultIdentityType">
                <el-select
                  v-model="state.dataForm.defaultIdentityType"
                  placeholder="请选择认证方式"
                >
                  <el-option
                    v-for="item in state.identityTypes"
                    :key="item"
                    :label="item"
                    :value="item"
                  />
                </el-select>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item>
                <el-button type="primary" @click="onSubmit(dataFormRef)">保存</el-button>
                <el-button @click="closeDialog">取消</el-button>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </div>
    </el-drawer>
  </div>
</template>

<script lang="ts" setup>
import { ref, toRefs, reactive, onMounted, defineComponent, watchEffect } from "vue";
import { ElMessageBox, ElMessage, FormRules, FormInstance } from "element-plus";

import { appmessage } from "/@/api/iapiresult";
import { getProduce, updateProduce, saveProduce } from "/@/api/produce";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
import monaco from "/@/components/monaco/monaco.vue";
import IconSelector from "/@/components/iconSelector/index.vue";
interface produceformdata {
  drawer: boolean;
  dialogtitle: string;
  dataForm: produceaddoreditdto;
  gatewayTypes: Array<string>;
  identityTypes: Array<string>;
  deviceTypes: Array<string>;
}
const emit = defineEmits([
  "close",
  "submit",
]);
var dataFormRef = ref();
const rules = reactive<FormRules>({
  name: [
    { required: true, type: "string", message: "请输入设备名称", trigger: "blur" },
    { min: 2, message: "设备名称长度应大于1", trigger: "blur" },
  ],

  gatewayType: [{ required: true, message: "请选择网关类型", trigger: "change" }],

  defaultIdentityType: [
    { required: true, type: "string", message: "请选择网关类型", trigger: "change" },
  ],
});
const state = reactive<produceformdata>({
  drawer: false,
  dialogtitle: "",
  gatewayTypes: ["Unknow", "Customize", "Modbus", "Bacnet", "OPC_UA", "CanOpen"],
  identityTypes: ["AccessToken", "DevicePassword", "X509Certificate"],
  deviceTypes: ["Device", "Gateway"],
  dataForm: {
    id: NIL_UUID,
    name: "",
    icon: "",
    defaultTimeout: 300,
    gatewayType: "",
    description: "",
    gatewayConfigurationName: "",
    defaultDeviceType: "",
    gatewayConfigurationJson: "",
    gatewayConfiguration: "",
    defaultIdentityType: "",
  },
});

const openDialog = (produceid: string) => {
  if (produceid === NIL_UUID) {
    state.dataForm = {
      id: NIL_UUID,
      name: "",
      icon: "",
      defaultTimeout: 300,
      description: "",
      gatewayType: "",
      gatewayConfigurationName: "",
      defaultDeviceType: "",
      gatewayConfigurationJson: "",
      gatewayConfiguration: "",
      defaultIdentityType: "",
    };
    state.dialogtitle = "新增产品";
  } else {
    state.dialogtitle = "修改产品";
    getProduce(produceid).then((res) => {
      state.dataForm.defaultDeviceType = res.data.defaultDeviceType;
      state.dataForm.defaultIdentityType = res.data.defaultIdentityType;
      state.dataForm.defaultTimeout = res.data.defaultTimeout;
      state.dataForm.description = res.data.description;
      state.dataForm.gatewayType = res.data.gatewayType;
      state.dataForm.icon = res.data.icon;
      state.dataForm.id = res.data.id;
      state.dataForm.name = res.data.name;
      if (
        state.dataForm.gatewayType !== "Unknow" &&
        state.dataForm.gatewayType !== "Customize"
      ) {
        state.dataForm.gatewayConfigurationName = res.data.gatewayConfiguration;
      } else if (state.dataForm.gatewayType === "Customize") {
        state.dataForm.gatewayConfigurationJson = res.data.gatewayConfiguration;
      }
    });
  }
  state.drawer = true;
};
// 关闭弹窗
const closeDialog = () => {
  state.drawer = false;
};
const oneditorchange = (content: string) => {};

watchEffect(() => {});

onMounted(() => {});
const onSubmit = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid, fields) => {
    if (valid) {
      if (state.dataForm.id === NIL_UUID) {
        if (state.dataForm.gatewayType === "Customize") {
          state.dataForm.gatewayConfiguration =
            state.dataForm.gatewayConfigurationJson ?? "";
        } else if (
          state.dataForm.gatewayType !== "Unknow" &&
          state.dataForm.gatewayType !== "Customize"
        ) {
          state.dataForm.gatewayConfiguration =
            state.dataForm.gatewayConfigurationName ?? "";
        }

        var result = await saveProduce(state.dataForm);
        if (result["code"] === 10000) {
          ElMessage.success("新增成功");
          emit("close", state.dataForm);
          state.drawer = false;
   
        } else {
          ElMessage.warning("新增失败:" + result["msg"]);
        }
      } else {
        if (state.dataForm.gatewayType === "Customize") {
          state.dataForm.gatewayConfiguration =
            state.dataForm.gatewayConfigurationJson ?? "";
        } else if (
          state.dataForm.gatewayType !== "Unknow" &&
          state.dataForm.gatewayType !== "Customize"
        ) {
          state.dataForm.gatewayConfiguration =
            state.dataForm.gatewayConfigurationName ?? "";
        }
        var result = await updateProduce(state.dataForm);
        if (result["code"] === 10000) {
          ElMessage.success("修改成功");
          emit("close", state.dataForm);
          state.drawer = false;
        } else {
          ElMessage.warning("修改失败:" + result["msg"]);
        }
      }
    } else {
    }
  });
};

defineExpose({
  openDialog,
});
</script>
