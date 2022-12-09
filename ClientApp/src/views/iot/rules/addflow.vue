<template>
  <div>
    <el-drawer v-model="state.drawer" :title="state.dialogtitle" size="60%">
      <div class="add-form-container">
        <el-form :model="state.dataForm" size="default" label-width="90px">
          <el-row :gutter="35">
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="规则链名称">
                <el-input
                  v-model="state.dataForm.name"
                  placeholder="请输入规则链名称"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="事件类型">
                <el-select
                  v-model="state.dataForm.mountType"
                  placeholder="请选择事件类型"
                >
                  <el-option
                    v-for="item in state.mountTypes"
                    :key="item.value"
                    :label="item.value"
                    :value="item.value"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="规则链描述">
                <el-input
                  v-model="state.dataForm.ruleDesc"
                  placeholder="请输入规则链描述信息"
                  clearable
                ></el-input>
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

<script lang="ts" setup>
import { ref, toRefs, reactive, onMounted, defineComponent, watchEffect } from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { ruleApi } from "/@/api/flows";
import { appmessage } from "/@/api/iapiresult";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
interface ruleform {
  drawer: boolean;
  dialogtitle: string;
  dataForm: ruleaddoreditdto;
  mountTypes: Array<any>;
}
const emit = defineEmits(["close", "submit"]);
const state = reactive<ruleform>({
  drawer: false,
  dialogtitle: "",
  mountTypes: [
    {
      value: "None",
      label: "None",
    },
    {
      value: "RAW",
      label: "RAW",
    },
    {
      value: "Telemetry",
      label: "遥测",
    },
    {
      value: "Attribute",
      label: "属性",
    },
    {
      value: "RPC",
      label: "远程控制",
    },
    {
      value: "Connected",
      label: "在线",
    },
    {
      value: "Disconnected",
      label: "离线",
    },
    {
      value: "TelemetryArray",
      label: "遥测数组",
    },
    {
      value: "Alarm",
      label: "告警",
    },
    {
      value: "DeleteDevice",
      label: "删除设备",
    },
    {
      value: "CreateDevice",
      label: "创建设备",
    },
    {
      value: "Activity",
      label: "活动事件",
    },
    {
      value: "Inactivity",
      label: "非活跃状态",
    },
  ],
  dataForm: {
    ruleId: NIL_UUID,
    name: "",
    ruleDesc: "",
    mountType: "",
  },
});

const openDialog = (ruleid: string) => {
  console.log(ruleid);
  if (ruleid === NIL_UUID) {
    state.dataForm = {
      ruleId: NIL_UUID,
      name: "",
      ruleDesc: "",
      mountType: "",
    };
    state.dialogtitle = "新增规则链";
  } else {
    state.dialogtitle = "修改规则链";
    ruleApi()
      .getrule(ruleid)
      .then((res) => {
        state.dataForm = res.data;
      });
  }
  state.drawer = true;
};
// 关闭弹窗
const closeDialog = () => {
  state.drawer = false;
  emit("close",state.dataForm);  
};

watchEffect(() => {});

onMounted(() => {});
const onSubmit = () => {
  if (state.dataForm.ruleId === NIL_UUID) {
    ruleApi()
      .postrule(state.dataForm)
      .then((res: appmessage<boolean>) => {
        if (res.code === 10000 && res.data) {
          ElMessage.success("新增成功");
          emit("submit",state.dataForm);  
          emit("close",state.dataForm);  
          state.drawer = false;
        } else {
          ElMessage.warning("新增失败:" + res.msg);
        }
      });
  } else {
    ruleApi()
      .putrule(state.dataForm)
      .then((res: appmessage<boolean>) => {
        if (res.code === 10000 && res.data) {
          ElMessage.success("修改成功");
        } else {
          ElMessage.warning("修改失败:" + res.msg);
        }
      });
  }
};

defineExpose({
  openDialog,
});
</script>
