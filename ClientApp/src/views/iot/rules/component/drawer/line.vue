<template>
  <div class="pt15 pr15 pb15 pl15">
    <el-form :model="state.line" size="default" label-width="50px">
      <el-form-item label="来往">
        <el-input v-model="state.line.contact" placeholder="来往" clearable disabled></el-input>
      </el-form-item>
      <el-form-item label="类型">
        <el-input v-model="state.line.type" placeholder="类型" clearable disabled></el-input>
      </el-form-item>
      <el-form-item label="名称">
        <el-input v-model="state.line.linename" placeholder="请输入名称" clearable></el-input>
      </el-form-item>

      <el-form-item label="条件">
        <el-input v-model="state.line.condition" placeholder="请输入条件" clearable></el-input>
      </el-form-item>
    </el-form>
    <div class="codeTip">
        <div>线节点上可以设置名称显示在线段上方，条件字段输入时需要注意，Node节点取数方式为input.Name来获取节点Json传值：</div>
<pre>
{
   "Name":"张三",
   "Age":18
}
</pre>
        <div>但线节点接收Json传值时，直接使用字段，条件写为：</div>
        <el-tag type="Info">Age>18</el-tag>即可，不能拼接input.
    </div>
  </div>
</template>

<script lang="ts" setup>
import { reactive } from "vue";

const props = defineProps({
  modelValue: {
    type: Object,
    default: {},
  },
});
// 定义接口来定义对象的类型
interface WorkflowDrawerLineState {
  line: any;
}

const emit = defineEmits(["linechange", "close"]);
const state = reactive<WorkflowDrawerLineState>({
  line: props.modelValue,
});
watch(
  () => props.modelValue,
  () => {
    state.line = props.modelValue
  }
);

// 获取父组件数据
// const getParentData = (linedata: any) => {
//   state.line.linename = linedata.linename ?? "";
//   state.line.condition = linedata.condition ?? "";
//   state.line.namespace = linedata.namespace ?? "";
// };
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
  onLineTextChange
});

</script>

<style scoped lang="scss">
    .codeTip{
        padding:5px 15px;
        font-size: 12px;
        clear:both;
        background: whitesmoke;
        border: 1px solid #dadada;
        color: #404040;
        pre{
            background: #1e1e1e;
            color: #c6c6c6;
            padding: 4px;
        }
    }
</style>
