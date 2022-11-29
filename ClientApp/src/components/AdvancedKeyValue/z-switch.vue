<template>
  <span class="z-switch" :style="{color}">
    <span class="z-status" :style="{backgroundColor: color}"></span>
    {{computedValue}}
  </span>
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
const color = computed(()=>{
  return props.value ? 'var(--el-color-primary)': 'Red'
})
const computedValue = computed(() => {
  const item = props.dict.find((dictItem)=> dictItem.value === props.value)
  console.log(`%c-@z-switch:19`, 'color:white;font-size:16px;background:blue;font-weight: bold;', item)
  return item.label
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
