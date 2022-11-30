<template>
  <div class="z-logo-container">
    <div v-if="!hideIcon" class="z-logo-icon" v-html="logoIcon" ></div>
    <div class="z-logo-text" v-html="logoText" :style="{fill: themeConfig.isIsDark ? 'white': ''}"></div>
  </div>
</template>
<script lang="ts" setup>
import logoIcon from '/@/assets/logo-icon.svg?raw'
import logoText from '/@/assets/logo-text.svg?raw'
import { useThemeConfig } from '/@/stores/themeConfig';
import {storeToRefs} from "pinia";
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
defineProps({
  hideIcon: {
    type: Boolean,
    default: false
  },
  hideText: {
    type: Boolean,
    default: false
  }
})
</script>

<style lang="scss" scoped>
.z-logo-container {
  box-sizing: border-box;
  display: flex;
  gap: 12px;
  align-items: center;

  .z-logo-icon {
    width:30px;

  }
  .z-logo-text {
    width:120px;
  }
  div {
    display: flex;
  }
}
</style>
