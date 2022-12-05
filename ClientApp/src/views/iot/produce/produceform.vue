<template>
  <div>
    <el-drawer v-model="drawer" :title="dialogtitle" size="75%">
      <div class="add-form-container">
        <el-form :model="dataForm" size="default" label-width="150px">
          <el-row :gutter="35">
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="产品名称">
                <el-input
                  v-model="dataForm.name"
                  placeholder="请输入产品名称"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="产品名称">
                <el-input
                  v-model="dataForm.icon"
                  placeholder="请输入产品icon"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="默认网关类型">
                <el-select
                  v-model="dataForm.gatewayType"
                  placeholder="请选择默认网关类型"
                >
                  <el-option
                    v-for="item in gatewayTypes"
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
              v-if="dataForm.gatewayType === 'Customize'"
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
                  v-model="dataForm.gatewayConfigurationJson"
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
                dataForm.gatewayType !== 'Unknow' && dataForm.gatewayType !== 'Customize'
              "
            >
              <el-form-item label="默认网关配置名称">
                <el-input
                  v-model="dataForm.gatewayConfigurationName"
                  placeholder="请输入默认网关配置名称"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="默认产品类型">
                <el-select
                  v-model="dataForm.defaultDeviceType"
                  placeholder="请选择认证方式"
                >
                  <el-option
                    v-for="item in deviceTypes"
                    :key="item"
                    :label="item"
                    :value="item"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="默认超时">
                <el-input
                  v-model="dataForm.defaultTimeout"
                  placeholder="请输入默认超时"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="认证方式">
                <el-select
                  v-model="dataForm.defaultIdentityType"
                  placeholder="请选择认证方式"
                >
                  <el-option
                    v-for="item in identityTypes"
                    :key="item"
                    :label="item"
                    :value="item"
                  />
                </el-select>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item>
                <el-button type="primary" @click="onSubmit">保存</el-button>
                <el-button @click="closeDialog">取消</el-button>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </div>
    </el-drawer>
  </div>
</template>

<script lang="ts">
import { ref, toRefs, reactive, onMounted, defineComponent, watchEffect } from "vue";
import { ElMessageBox, ElMessage } from "element-plus";

import { appmessage } from "/@/api/iapiresult";
import { getProduce, updateProduce,saveProduce } from "/@/api/produce";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
import monaco from "/@/components/monaco/monaco.vue";

interface produceformdata {
  drawer: boolean;
  dialogtitle: string;
  dataForm: produceaddoreditdto;
  gatewayTypes: Array<string>;
  identityTypes: Array<string>;
  deviceTypes: Array<string>;
}

export default defineComponent({
  name: "produceform",
  components: { monaco },
  setup(props) {
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
          id:NIL_UUID,
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
    const onSubmit = () => {
      if (state.dataForm.id === NIL_UUID) {
        // deviceApi()
        //   .postdevcie(state.dataForm)
        //   .then((res: appmessage<boolean>) => {
        //     if (res.code === 10000 && res.data) {
        //       ElMessage.success("新增成功");
        //     } else {
        //       ElMessage.warning("新增失败:" + res.msg);
        //     }
        //   });
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


        saveProduce(state.dataForm);
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

        updateProduce(state.dataForm);
      }
    };
    return { ...toRefs(state), onSubmit, openDialog, closeDialog, oneditorchange };
  },
});
</script>
