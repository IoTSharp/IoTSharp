<template>
  <div class="z-card">
    <div class="z-header ">
      <div class="title">
        <component class="icon" :is="icon" :style="{color: themeConfig.primary}"></component>
        <div class="pl-10px">{{ title }}</div>
      </div>
      <slot name="more"></slot>
    </div>
    <div class="content mt-20px">
      <slot></slot>
    </div>
  </div>
</template>
<script setup lang="ts">
import { useThemeConfig } from '/@/stores/themeConfig';
import {storeToRefs} from "pinia";
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
defineProps(['icon','title'])


</script>
<style scoped lang="scss">
.z-card {
  .z-header{
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .title {
    align-items: center;
    display: flex;
    font-size: 16px;
    font-weight: bold;
    .icon {
      scale:1.4;
    }
  }
  .content {
    height: 100%;
  }

  width: 100%;
  border-radius: 4px;
  transition: all ease 0.3s;
  padding: 20px;
  overflow: hidden;
  background: var(--el-color-white);
  color: var(--el-text-color-primary);
  border: 1px solid var(--next-border-color-light);
  &:hover {
    box-shadow: 0 2px 12px var(--next-color-dark-hover);
    transition: all ease 0.3s;
  }
  &-icon {
    width: 70px;
    height: 70px;
    border-radius: 8px;
    flex-shrink: 1;
    i {
      color: var(--el-text-color-placeholder);
    }
  }
  &-title {
    font-size: 15px;
    font-weight: bold;
    height: 30px;
  }
}
</style>
