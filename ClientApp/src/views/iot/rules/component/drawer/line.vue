<template>
  <div class="pt15 pr15 pb15 pl15">
    <el-form :model="state.line" size="default" label-width="50px">
      <el-form-item label="来往">
        <el-input
          v-model="state.line.contact"
          placeholder="来往"
          clearable
          disabled
        ></el-input>
      </el-form-item>
      <el-form-item label="类型">
        <el-input
          v-model="state.line.type"
          placeholder="类型"
          clearable
          disabled
        ></el-input>
      </el-form-item>
      <el-form-item label="名称">
        <el-input
          v-model="state.line.linename"
          placeholder="请输入名称"
          clearable
        ></el-input>
      </el-form-item>

      <el-form-item label="条件">
        <el-input
          v-model="state.line.condition"
          placeholder="请输入条件"
          clearable
        ></el-input>
      </el-form-item>
    </el-form>
  </div>
</template>

<script lang="ts" setup>
import { reactive } from "vue";

// 定义接口来定义对象的类型
interface WorkflowDrawerLineState {
  line: any;
}

const emit = defineEmits(["linechange", "close"]);
const state = reactive<WorkflowDrawerLineState>({
  line: {},
});

// 获取父组件数据
const getParentData = (linedata: any) => {
  state.line.linename = linedata.linename ?? "";
  state.line.condition = linedata.condition ?? "";
  state.line.namespace = linedata.namespace ?? "";
};
// 重置
const onLineTextReset = () => {
  state.line.linename = "";
};
// 保存
const onLineTextChange = () => {
  emit("linechange", state.line);
  emit("close");
};
defineExpose({
  getParentData,onLineTextChange
});

</script>
