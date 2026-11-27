<template>
  <template v-for="val in chils">
    <el-sub-menu :index="val.path" :key="val.path" v-if="val.children && val.children.length > 0" :popper-class="currentPath !== undefined ? 'iotsharp-nav-flyout' : undefined">
      <template #title>
        <el-icon class="z-menu-icon">
          <component :is="menuIconList[val.name] || menuIconList.default"></component>
        </el-icon>
        <span class="z-menu-label" :title="$t(val.meta.title)">{{ $t(val.meta.title) }}</span>
      </template>
      <SubItem :chil="val.children" :current-path="currentPath" />
    </el-sub-menu>
    <template v-else>
      <el-menu-item :index="val.path" :key="val.path" :aria-current="currentPath === val.path ? 'page' : undefined">
        <template v-if="!val.meta.isLink || (val.meta.isLink && val.meta.isIframe)">
          <el-icon class="z-menu-icon">
            <component :is="menuIconList[val.name] || menuIconList.default"></component>
          </el-icon>
          <span class="z-menu-label" :title="$t(val.meta.title)">{{ $t(val.meta.title) }}</span>
        </template>
        <template v-else>
          <a :href="val.meta.isLink" target="_blank" rel="opener" class="w100">
            <SvgIcon :name="val.meta.icon"/>
            {{ $t(val.meta.title) }}
          </a>
        </template>
      </el-menu-item>
    </template>
  </template>
</template>

<script lang="ts">
import {computed, defineComponent} from 'vue';
import {menuIconList} from "/@/layout/navMenu/menu-icons-config.js";

export default defineComponent({
  name: 'navMenuSubItem',
  props: {
    currentPath: String,
    chil: {
      type: Array,
      default: () => [],
    },
  },
  setup(props) {
    // 获取父级菜单数据
    const chils = computed(() => {
      return <any>props.chil;
    });
    return {
      chils, menuIconList
    };
  },
});
</script>
