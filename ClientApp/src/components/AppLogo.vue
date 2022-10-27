<template>
  <div class="z-logo-container">
    <div v-if="!hideIcon">
      <img :src="logoIcon">
    </div>
    <div>
      <img :src="logoText">
    </div>

  </div>
</template>
<script lang="ts" setup>
import logoIcon from '/@/assets/logo-icon.svg'
import logoText from '/@/assets/logo-text.svg'
const props = defineProps({
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
  div {
    display: flex;
    align-items: center;
  }
  img {
    width: 100%;
  }
}
</style>
