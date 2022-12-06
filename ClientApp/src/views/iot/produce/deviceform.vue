<template>
  <div>
    <el-drawer v-model="state.drawer" :title="state.dialogtitle" size="75%">
      <div class="add-form-container">
        <el-form
          size="default"
          :model="state.dataForm"
          label-width="150px"
          :rules="rules"
          ref="dataFormRef"
        >
          <el-row :gutter="35">
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="设备名称" prop="name">
                <el-input
                  v-model="state.dataForm.name"
                  placeholder="请输入设备名称"
                  clearable
                ></el-input>
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
import { ref, reactive, onMounted, watchEffect } from "vue";
import { FormRules, ElMessage, FormInstance } from "element-plus";
import { createDevice } from "/@/api/produce";
import { appmessage } from "/@/api/iapiresult";
interface dataForm {
  name: string;
}

interface produceformdata {
  drawer: boolean;
  dialogtitle: string;
  dataForm: dataForm;
  produceid: string;
}

const dataFormRef = ref();
const rules = reactive<FormRules>({
  name: [
    { required: true, type: "string", message: "请输入设备名称", trigger: "blur" },
    { min: 2, message: "设备名称长度应大于1", trigger: "blur" },
  ],
});
const state = reactive<produceformdata>({
  drawer: false,
  dialogtitle: "",
  dataForm: {
    name: "",
  },
  produceid: "",
});

const openDialog = (produceid: string) => {
  state.produceid = produceid;
  state.drawer = true;
};
// 关闭弹窗
const closeDialog = () => {
  state.drawer = false;
};

watchEffect(() => {});

onMounted(() => {});
const onSubmit = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid, fields) => {
    if (valid) {
      var result = await createDevice(state.produceid, state.dataForm);
      if (result["code"] === 10000) {
        ElMessage.success("新增成功");
        state.drawer = false;
      } else {
        ElMessage.warning("新增失败:" + result["msg"]);
      }
    } else {
      console.log("提交失败!", fields);
    }
  });
};


defineExpose({
  openDialog,
});
</script>
