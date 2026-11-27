<template>
  <el-menu
      class="z-vertical-menu"
      ref="menuRef"
      :aria-label="t('message.consoleLayout.navigation')"
      router
      :default-active="defaultActive"
      :default-openeds="activeParents"
      background-color="transparent"
      :collapse="isCollapse"
      :unique-opened="getThemeConfig.isUniqueOpened"
      :collapse-transition="false"
      @select="$emit('navigate')"
  >
    <template v-for="val in menuLists" :key="val.path">
      <el-sub-menu :index="val.path" v-if="val.children && val.children.length > 0" popper-class="iotsharp-nav-flyout">
        <template #title>
          <el-icon class="z-menu-icon">
          <component :is="menuIconList[val.name] || menuIconList.default"></component>
          </el-icon>
          <span class="z-menu-label" :title="$t(val.meta.title)">{{ $t(val.meta.title) }}</span>
        </template>
        <SubItem :chil="val.children" :current-path="defaultActive" />
      </el-sub-menu>
      <template v-else>
        <el-menu-item :index="val.path" :aria-current="defaultActive === val.path ? 'page' : undefined">
          <el-icon class="z-menu-icon">
            <component :is="menuIconList[val.name] || menuIconList.default"></component>
          </el-icon>
          <template #title v-if="!val.meta.isLink || (val.meta.isLink && val.meta.isIframe)">
            <span class="z-menu-label" :title="$t(val.meta.title)">{{ $t(val.meta.title) }}</span>
          </template>
          <template #title v-else>
            <a :href="val.meta.isLink" target="_blank" rel="opener" class="w100">{{ $t(val.meta.title) }} </a>
          </template>
        </el-menu-item>
      </template>
    </template>
  </el-menu>
</template>

<script lang="ts">
import {computed, defineComponent, ref, watch} from 'vue';
import {useRoute} from 'vue-router';
import {useI18n} from 'vue-i18n';
import type {MenuInstance} from 'element-plus';
import {storeToRefs} from 'pinia';
import {useThemeConfig} from '/@/stores/themeConfig';
import SubItem from '/@/layout/navMenu/subItem.vue';
import {menuIconList} from "/@/layout/navMenu/menu-icons-config.js";

interface NavigationItem {
  path: string;
  children?: NavigationItem[];
}

/** 找出当前菜单项所属的各级分组，用于直达页面和恢复侧栏。 */
function findMenuParents(items: NavigationItem[], path: string): string[] | undefined {
  for (const item of items) {
    if (item.path === path) return [];
    const parents = item.children && findMenuParents(item.children, path);
    if (parents) return [item.path, ...parents];
  }
}

export default defineComponent({
  name: 'navMenuVertical',
  components: {SubItem},
  emits: ['navigate'],
  props: {
    expanded: Boolean,
    menuList: {
      type: Array,
      default: () => [],
    },
  },
  setup(props) {
    const storesThemeConfig = useThemeConfig();
    const {themeConfig} = storeToRefs(storesThemeConfig);
    const route = useRoute();
    const {t} = useI18n();
    const menuRef = ref<MenuInstance>();
    // 获取父级菜单数据
    const menuLists = computed(() => {
      return <any>props.menuList;
    });
    // 获取布局配置信息
    const getThemeConfig = computed(() => {
      return themeConfig.value;
    });
    // 菜单高亮（详情时，父级高亮）
    const defaultActive = computed(() => {
      const path = route.meta.isDynamic ? String(route.meta.isDynamicPath || route.path) : route.path;
      const segments = path.split('/');
      return segments.length >= 4 && route.meta.isHide ? segments.slice(0, 3).join('/') : path;
    });
    const isCollapse = computed(() => !props.expanded && themeConfig.value.isCollapse);
    const activeParents = computed(() => findMenuParents(menuLists.value, defaultActive.value) ?? []);
    watch([defaultActive, isCollapse], () => {
      const parents = activeParents.value;
      const group = parents[parents.length - 1];
      if (!isCollapse.value && group) menuRef.value?.open(group);
    }, { flush: 'post' });
    return {
      t,
      menuIconList,
      menuLists,
      menuRef,
      getThemeConfig,
      defaultActive,
      activeParents,
      isCollapse,
    };
  },
});
</script>
<style lang="scss">
.z-vertical-menu,
.iotsharp-nav-flyout {
  width: 100%;
  --el-menu-base-level-padding: 12px;
  --el-menu-level-padding: 12px;
  border: none;

  .z-menu-icon {
    margin-right: 10px;
    font-size: 16px;
    flex-shrink: 0;
    color: var(--iotsharp-nav-icon);
  }

  .el-menu {
    width: auto;
    border: none;
    background: transparent;
  }

  .el-menu-item,
  .el-sub-menu__title {
    height: 40px !important;
    line-height: 40px !important;
    margin: 2px 0;
    border-radius: 6px;
    color: var(--iotsharp-nav-ink);
    font-size: 14px;
    font-weight: 500;
  }

  .z-menu-label {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .el-sub-menu__title .z-menu-label {
    margin-inline-end: 18px;
  }

  .el-menu-item:focus-visible,
  .el-sub-menu__title:focus-visible,
  .el-sub-menu:focus-visible > .el-sub-menu__title {
    outline: 2px solid var(--iotsharp-nav-indicator);
    outline-offset: -3px;
  }

  .el-menu-item:hover,
  .el-sub-menu__title:hover {
    background-color: var(--iotsharp-nav-hover) !important;
    color: var(--iotsharp-nav-ink);
  }

  .el-menu-item.is-active {
    background-color: var(--iotsharp-nav-active) !important;
    color: var(--iotsharp-nav-ink);
    font-weight: 600;
    box-shadow: inset 3px 0 var(--iotsharp-nav-indicator);
  }

  .el-sub-menu.is-active > .el-sub-menu__title {
    color: var(--iotsharp-nav-ink);
    font-weight: 600;
  }

  .el-sub-menu.is-opened > .el-sub-menu__title {
    background-color: var(--iotsharp-nav-hover) !important;
  }

  .el-menu-item.is-active .z-menu-icon,
  .el-sub-menu.is-active > .el-sub-menu__title .z-menu-icon,
  .el-menu-item:hover .z-menu-icon,
  .el-sub-menu__title:hover .z-menu-icon {
    color: var(--iotsharp-nav-ink);
  }

  .el-menu.el-menu--inline {
    margin: 3px 0 6px 18px;
    padding: 3px 4px;
    border-inline-start: 1px solid var(--iotsharp-nav-divider);
    border-start-end-radius: 6px;
    border-end-end-radius: 6px;
    background: var(--iotsharp-nav-submenu) !important;
  }

  .el-menu--inline .el-menu-item,
  .el-menu--inline .el-sub-menu__title {
    min-width: auto;
    height: 34px !important;
    padding-inline: 8px !important;
    color: var(--iotsharp-nav-subtext);
    font-size: 13px;
    line-height: 34px !important;

    .z-menu-icon {
      width: 14px;
      margin-inline-end: 8px;
      font-size: 14px;
    }
  }

  .el-menu--inline .el-menu-item.is-active {
    color: var(--iotsharp-nav-ink);
  }

  .el-sub-menu__icon-arrow {
    color: var(--iotsharp-nav-icon);
  }

  &.el-menu--collapse {
    width: 100% !important;
    .z-menu-icon {
      margin-right: 0;
    }
  }
}

.iotsharp-nav-flyout.el-popper {
  width: auto;
  min-width: 180px;
  padding: 0 !important;
  border: 1px solid var(--iotsharp-nav-divider);
  background: var(--iotsharp-nav-gradient);

  &::before { display: none; }

  .el-menu--popup { padding: 5px; }
}
</style>
