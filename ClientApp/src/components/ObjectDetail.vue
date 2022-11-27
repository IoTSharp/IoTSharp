<template>
  <div class="z-object-detail">
    <div class="z-row" v-for="item in list" :key="item.key">
      <div class="z-key truncate" :style="{width: `${labelWidth}px`}">{{ item.label }}:</div>
      <div class="z-value">{{ item.value }}</div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import {onMounted} from "vue";
import {sleep} from "/@/utils/other";

interface ConfigItem {
  type: string,
  label: string
}
const props = defineProps({
  obj: Object,
  labelWidth: Number,
  config: Object, // 字段配置， 每个字段以什么样式配置显示
})
const config:{
  [key: string]: ConfigItem
} = {
  active: {
    label: '在线状态',
    type: 'boolean'
  }
}
const list = computed(()=>{
  // return props.selectedKeys?.map(key => ({key: getKeyTranslate(key), value: props?.obj[key]}))
  if (props.obj) {
    if (Object.keys(config).length ) {
      return Object.entries(props.obj).filter(([key, value])=> {
        return config[key]
      }).map(([key,value])=> ({key, value, label: config[key].label}))
    }
    // 如果没有配置， 就显示所有的字段和值
    else return Object.entries(props.obj).map(([key,value])=> ({key, value, label: key}))
  }


})

onMounted(async ()=>{
  await sleep(1000)
  console.log(`%c-@ObjectDetail:25`, 'color:white;font-size:16px;background:blue;font-weight: bold;', config['active'])
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
  }

  .z-value {
    //width: 280px;
    display: inline-flex;
    padding: 0 4px;
    font-weight: bold;
    flex-shrink: 0;
    word-wrap: break-word;
    flex-wrap: wrap;
    white-space: normal;
  }

}

</style>
