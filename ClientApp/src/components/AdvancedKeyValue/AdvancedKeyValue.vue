<template>
  <div class="z-object-detail">
    <div class="z-row" v-for="item in list" :key="item.key">
      <div class="z-key truncate" :style="{width: `${labelWidth}px`}">{{ item.title }}:</div>
      <div class="z-value">

        <z-switch v-if="item.type === 'dict-switch'" :dict="item.dict.data" :value="item.value">  </z-switch>
        <z-select v-else-if="item.type === 'dict-select'" :dict="item.dict.data" :value="item.value">  </z-select>
        <span v-else>{{ item.value }}</span>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import {onMounted} from "vue";
import {sleep} from "/@/utils/other";
import ZSelect from "./z-select.vue";
import ZSwitch from "./z-switch.vue";
const props = defineProps({
  obj: Object, //要展示的对象
  labelWidth: Number, // label 宽度
  config: { // 配置
    type: Object,
    default: () => {
      return {};
    }
  },
  hideKeyList: { // 需要隐藏的字段
    type: Array,
    default: () => []
  },
})
const list = computed(()=>{
  if (props.obj) {
    // 过滤掉需要隐藏的部分
    // eslint-disable-next-line no-unused-vars
    const filtered = Object.entries(props.obj).filter(([key, value]) => {
      return !props.hideKeyList.includes(key)
    })
    // 如果有配置
    return filtered.map(([key, value])=> {
      let item = {key, value, title: key}
      if (props.config[key]) {
        Object.assign(item , props.config[key])
      }
      if (key === 'deviceType') {
        // item.value = 'test'
      }
      return item
    })
  }

})

onMounted(async ()=>{
  await sleep(1000)
})
</script>
<style lang="scss" scoped>

.z-object-detail {
  padding: 6px;
  display: flex;
  flex-wrap: wrap;
  //margin: -12px;
  //width: calc(100% + 12px);
  //gap: 10px;

  .z-row {
    //margin-bottom: 10px;
    //margin-top: 10px;
    width: 100%;
    display: flex;
    //width: 50%;
    //width: 300px;
    //margin: 12px;
    padding: 6px 10px;
  }

  .z-key {
    padding-right: 10px;
    flex-shrink: 0;
    width: 100px;
    text-align: right;
    color: var(--el-color-info-dark-2)
  }

  .z-value {
    //width: 280px;
    display: inline-flex;
    padding: 0 4px;
    flex-shrink: 0;
    word-wrap: break-word;
    flex-wrap: wrap;
    white-space: normal;
  }

}

</style>
