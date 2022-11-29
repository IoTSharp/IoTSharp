<template>
  <el-tag :type="colorType">{{computedValue}}</el-tag>
</template>
<script setup>
const props = defineProps({
// 字段配置， 每个字段以什么样式配置显示
  value: {
    type: [Boolean, Number, String],
    default: false,
  },
  dict: {
    type: Array,
    default: () => []
  },

})
const colorType = ref()
const computedValue = computed(() => {
  const item = props.dict.find((dictItem)=> dictItem.value === props.value)
  if (item?.color) {
    colorType.value = item.color
  }
  return item ? item.label: props.value
})
</script>
<style scoped>
.z-switch {
  display: inline-flex;
  align-items: center;
}
.z-status {
  margin-right: 4px;
  background-color: Silver;
  width: 4px;
  height: 4px;
  border-radius: 50%;
  display: inline-block;
}
</style>
