<template>

  <el-card header="服务侧属性" class="df-card">
    <fs-form ref="formserverRef" v-bind="formserverOptions" />
  </el-card>
  <el-card header="客户侧属性" class="df-card">
    <fs-form ref="formclientRef" v-bind="formclientOptions" />
  </el-card>
  <el-card header="任意侧属性" class="df-card">
    <fs-form ref="formanyRef" v-bind="formanyOptions" />
  </el-card>
  <div class="df-card">
    <el-button type="primary" @click="formSubmit">保存属性</el-button>
    <el-button  @click="formClose">返回</el-button>
  </div>


</template>

<script lang="ts" setup>
import { defineComponent, ref } from "vue";
import { ElMessage } from "element-plus";
import { useCrud, useExpose, useColumns, dict } from "@fast-crud/fast-crud";
import monaco from "/@/components/monaco/monaco.vue";
import { deviceApi } from "/@/api/devices";
import { appmessage } from "/@/api/iapiresult";
import dayjs from "dayjs";
const props = defineProps({
  deviceId: {
    type: String,
    default: ''
  }
})
interface propformstate {
  drawer: boolean;
  dialogtitle: string;
  devid: string;
}
const emit = defineEmits(["close", "submit"]);
const state = reactive<propformstate>({ drawer: false, dialogtitle: "设备属性修改", devid: props.deviceId });
const formserverRef = ref();
const formserverOptions = ref();
const formclientRef = ref();
const formclientOptions = ref();
const formanyRef = ref();
const formanyOptions = ref();
const buidForm = async () => {
  var result = await deviceApi().getDeviceAttributes(state.devid);
  var serveropt = {
    form: {
      labelWidth: "120px",
      display: "flex",
    },
    columns: {},
  };
  var clientopt = {
    form: {
      labelWidth: "120px",
      display: "flex",
    },
    columns: {},
  };
  var anyopt = {
    form: {
      labelWidth: "120px",
      display: "flex",
    },
    columns: {},
  };
  var serverval = {};
  var anyval = {};
  var clientval = {};
  for (var item of result.data) {
    switch (item.dataSide) {
      case "AnySide":
        anyval[item.keyName] = item.value;
        anyopt.columns[item.keyName] = buildWegits(item, {});
        break;
      case "ServerSide":
        serverval[item.keyName] = item.value;
        serveropt.columns[item.keyName] = buildWegits(item, {});
        break;
      case "ClientSide":
        clientval[item.keyName] = item.value;
        clientopt.columns[item.keyName] = buildWegits(item, {});
        break;
    }
  }
  const { buildFormOptions } = useColumns();
  formserverOptions.value = buildFormOptions(serveropt);
  formclientOptions.value = buildFormOptions(clientopt);
  formanyOptions.value = buildFormOptions(anyopt);
  formserverRef.value.setFormData(serverval);
  formclientRef.value.setFormData(clientval);
  formanyRef.value.setFormData(anyval);
};
const buildWegits = (data: any, cfg?: any) => {
  switch (data.dataType) {
    case "Boolean":
      return {
        title: data.keyName,
        type: "dict-switch",
        form: {
          col: { span: 24 },
          dict: dict({
            data: [
              { value: true, label: "开启" },
              { value: false, label: "关闭" },
            ],
          }),
        },
      };
      break;
    case "DateTime":
      return {
        title: data.keyName,
        type: "datetime",
        search: {
          show: true,
          width: 185,
          component: {},
        },
        valueBuilder({ value, row, key }) {
          if (value != null) {
            row[key] = dayjs(value);
          }
        },
        valueResolve({ value, row, key }) {
          if (value != null) {
            row[key] = value.valueOf();
          }
        },
      };
      break;
    case "Long":
      return {
        title: data.keyName,
        search: { show: true },
        type: "number",
      };
      break;
    case "String":
      return {
        title: data.keyName,
        type: "text", //虽然不写也能正确显示组件，但不建议省略它
        search: { show: true },
        form: {
          component: {
            maxlength: 20,
          },
        },
      };
      break;
    case "Double":
      return {
        title: data.keyName,
        type: "number",
        form: {
          component: {
            step: 0.1,
          },
        },
      };
      break;
    case "Json":
      return {
        title: data.keyName,
        form: {
          col: { span: 24 },
          component: {
            name: shallowRef(monaco),
            vModel: "modelValue",
            on: {
              change(context) { },
            },
          },
          rules: [{ required: true, message: "此项必填" }],
        },
      };
      break;
    case "XML":
      return {
        title: data.keyName,
        form: {
          col: { span: 24 },
          component: {
            name: shallowRef(monaco),
            vModel: "modelValue",
            on: {
              change(context) { },
            },
          },
          rules: [{ required: true, message: "此项必填" }],
        },
      };
      break;
    case "Binary":
      return {
        title: data.keyName,
        form: {
          col: { span: 24 },
          component: {
            name: shallowRef(monaco),
            vModel: "modelValue",
            on: {
              change(context) { },
            },
          },
          rules: [{ required: true, message: "此项必填" }],
        },
      };
      break;
    default:
      return {
        title: data.keyName,
        type: "text", //虽然不写也能正确显示组件，但不建议省略它
        search: { show: true },
        form: {
          component: {
            maxlength: 1000,
          },
        },
      };
  }
};
const formSubmit = async () => {
  var data = {
    anyside: formanyRef.value.getFormData(),
    clientside: formclientRef.value.getFormData(),
    serverside: formserverRef.value.getFormData(),
  };
  var result = await deviceApi().editDeviceAttributes(state.devid, data);
 if (result["code"] === 10000) {
    ElMessage.success("属性数据更新成功");
    emit('submit', data)
    emit('close', data)
    state.drawer = false;
  } else {
    ElMessage.warning("属性数据更新失败:" + result["msg"]);
  }
};
const formReset = () => {
  formserverRef.value.reset();
  formclientRef.value.reset();
  formanyRef.value.reset();
};
const formClose = () => {
  var data = {
    anyside: formanyRef.value.getFormData(),
    clientside: formclientRef.value.getFormData(),
    serverside: formserverRef.value.getFormData(),
  };
  emit('close', data)
};
onMounted(() => {
  buidForm();
});
defineExpose({
});
</script>
<style lang="scss" scoped>
.df-card {
  margin-top: 1rem;
}
</style>