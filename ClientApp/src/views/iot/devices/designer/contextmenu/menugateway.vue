<template>
  <transition name="el-zoom-in-center">
    <div aria-hidden="true" class="el-dropdown__popper el-popper is-light is-pure custom-contextmenu" role="tooltip"
      data-popper-placement="bottom" :style="`top: ${dropdowns.y + 5}px;left: ${dropdowns.x}px;`" :key="Math.random()"
      v-show="state.isShow">
      <ul class="el-dropdown-menu">
        <li v-for="(v, k) in state.menuitems" class="el-dropdown-menu__item" aria-disabled="false" tabindex="-1"
          :key="k" @click="onCurrentClick(v['command'])">
          <SvgIcon :name="v['icon']" />
          <span>{{ v['txt'] }}{{ state.sender['node']['bizdata']['name'] }}</span>
        </li>
      </ul>
      <div class="el-popper__arrow" style="left: 10px"></div>
    </div>
  </transition>
</template>
  
<script lang="ts" setup>
import { computed, reactive, onMounted, onUnmounted } from "vue";

const props = defineProps({
  dropdown: {
    type: Object,
  },
});
const emit = defineEmits(["click", "contextmenu", "ongatewaycommand"]);

const state = reactive({
  isShow: false,
  menuitems: [

  ],
  sender: {}

});
// 父级传过来的坐标 x,y 值
const dropdowns = computed(() => {
  return <any>props.dropdown;
});
// 当前项菜单点击
const onCurrentClick = (command: string) => {
  emit(
    "ongatewaycommand",
    { command, sender:state.sender }

  );
};
// 打开右键菜单：判断是否固定，固定则不显示关闭按钮
const openContextmenu = (sender: any) => {
  state.sender = sender;
  state.menuitems = sender.node?.bizdata?.command?.contextmenu
  closeContextmenu();
  setTimeout(() => {
    state.isShow = true;
  }, 10);
};
// 关闭右键菜单
const closeContextmenu = () => {
  state.isShow = false;
};
// 监听页面监听进行右键菜单的关闭
onMounted(() => {
  document.body.addEventListener("click", closeContextmenu);
  document.body.addEventListener("contextmenu", closeContextmenu);
});
// 页面卸载时，移除右键菜单监听事件
onUnmounted(() => {
  document.body.removeEventListener("click", closeContextmenu);
  document.body.removeEventListener("contextmenu", closeContextmenu);
});

defineExpose({
  openContextmenu,
});
</script>
  
<style scoped lang="scss">
.custom-contextmenu {
  transform-origin: center top;
  z-index: 2190;
  position: fixed;

  .el-dropdown-menu__item {
    font-size: 12px !important;
    white-space: nowrap;

    i {
      font-size: 12px !important;
    }
  }
}
</style>
  