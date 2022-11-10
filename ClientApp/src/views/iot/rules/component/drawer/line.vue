<template>
  <div class="pt15 pr15 pb15 pl15">
    <el-form :model="line" size="default" label-width="50px">
      <el-form-item label="来往">
        <el-input v-model="line.contact" placeholder="来往" clearable disabled></el-input>
      </el-form-item>
      <el-form-item label="类型">
        <el-input v-model="line.type" placeholder="类型" clearable disabled></el-input>
      </el-form-item>
      <el-form-item label="label">
        <el-input
          v-model="line.linename"
          placeholder="请输入label内容"
          clearable
        ></el-input>
      </el-form-item>

      <el-form-item label="condition">
        <el-input
          v-model="line.condition"
          placeholder="请输入label内容"
          clearable
        ></el-input>
      </el-form-item>
    </el-form>
  </div>
</template>

<script lang="ts">
import { defineComponent, reactive, toRefs } from "vue";

// 定义接口来定义对象的类型
interface WorkflowDrawerLineState {
  line: any;
}

export default defineComponent({
  name: "pagesWorkflowDrawerLine",
  setup(props, { emit }) {
    const state = reactive<WorkflowDrawerLineState>({
      line: {},
    });

    // 获取父组件数据
    const getParentData = (linedata: any) => {
      state.line.linename = linedata.linename ?? "";
      state.line.condition = linedata.condition ?? "";
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
    return {
      getParentData,
      onLineTextReset,
      onLineTextChange,
      ...toRefs(state),
    };
  },
});
</script>
