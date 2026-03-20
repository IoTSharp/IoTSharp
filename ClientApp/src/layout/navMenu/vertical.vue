<template>
  <el-menu
      class="z-vertical-menu"
      router
      :default-active="defaultActive"
      background-color="transparent"
      :collapse="isCollapse"
      :unique-opened="getThemeConfig.isUniqueOpened"
      :collapse-transition="false"
  >
    <template v-for="val in menuLists">
      <el-sub-menu :index="val.path" v-if="val.children && val.children.length > 0" :key="val.path">
        <template #title>
          <!--					<SvgIcon :name="val.meta.icon" />-->
          <el-icon class="z-menu-icon">
            <component :is="menuIconList[val.name]"></component>
          </el-icon>
          <span>{{ $t(val.meta.title) }} </span>
        </template>
        <SubItem :chil="val.children"/>
      </el-sub-menu>
      <template v-else>
        <el-menu-item :index="val.path" :key="val.path">
          <!--					<SvgIcon :name="val.meta.icon" />-->
          <el-icon class="z-menu-icon">
            <component :is="menuIconList[val.name]"></component>
          </el-icon>
          <template #title v-if="!val.meta.isLink || (val.meta.isLink && val.meta.isIframe)">
            <span class="z-sub-menu-item">{{ $t(val.meta.title) }} </span>
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
import {toRefs, reactive, computed, defineComponent, onMounted, watch} from 'vue';
import {useRoute, onBeforeRouteUpdate} from 'vue-router';
import {storeToRefs} from 'pinia';
import {useThemeConfig} from '/@/stores/themeConfig';
import SubItem from '/@/layout/navMenu/subItem.vue';
import {menuIconList} from "/@/layout/navMenu/menu-icons-config.js";

export default defineComponent({
  name: 'navMenuVertical',
  components: {SubItem},
  props: {
    menuList: {
      type: Array,
      default: () => [],
    },
  },
  setup(props) {
    const storesThemeConfig = useThemeConfig();
    const {themeConfig} = storeToRefs(storesThemeConfig);
    const route = useRoute();
    const state = reactive({
      // 修复：https://gitee.com/lyt-top/vue-next-admin/issues/I3YX6G
      defaultActive: route.meta.isDynamic ? route.meta.isDynamicPath : route.path,
      isCollapse: false,
    });
    // 获取父级菜单数据
    const menuLists = computed(() => {
      return <any>props.menuList;
    });
    // 获取布局配置信息
    const getThemeConfig = computed(() => {
      return themeConfig.value;
    });
    // 菜单高亮（详情时，父级高亮）
    const setParentHighlight = (currentRoute: any) => {
      const {path, meta} = currentRoute;
      const pathSplit = meta.isDynamic ? meta.isDynamicPath.split('/') : path.split('/');
      if (pathSplit.length >= 4 && meta.isHide) return pathSplit.splice(0, 3).join('/');
      else return path;
    };
    // 设置菜单的收起/展开
    watch(
        themeConfig.value,
        () => {
          document.body.clientWidth <= 1000 ? (state.isCollapse = false) : (state.isCollapse = themeConfig.value.isCollapse);
        },
        {
          immediate: true,
        }
    );
    // 页面加载时
    onMounted(() => {
      state.defaultActive = setParentHighlight(route);
    });
    // 路由更新时
    onBeforeRouteUpdate((to) => {
      // 修复：https://gitee.com/lyt-top/vue-next-admin/issues/I3YX6G
      state.defaultActive = setParentHighlight(to);
      const clientWidth = document.body.clientWidth;
      if (clientWidth < 1000) themeConfig.value.isCollapse = false;
    });
    return {
      menuIconList,
      menuLists,
      getThemeConfig,
      ...toRefs(state),
    };
  },
});
</script>
<style lang="scss">
.z-vertical-menu {
  width: 100%;
  padding-top: 2px;

  .z-menu-icon {
    margin-right: 12px;
    font-size: 18px;
    flex-shrink: 0;
    color: #8aa0b7;
  }

  :deep(.el-menu-item),
  :deep(.el-sub-menu__title) {
    height: 48px;
    margin-bottom: 8px;
    border-radius: 15px;
    color: #50657d;
    font-weight: 600;
  }

  :deep(.el-menu-item:hover),
  :deep(.el-sub-menu__title:hover) {
    background: #f4f8ff;
    color: #1d4ed8;
  }

  :deep(.el-menu-item.is-active) {
    background: linear-gradient(135deg, rgba(37, 99, 235, 0.12), rgba(14, 165, 233, 0.08));
    color: #1d4ed8;
    font-weight: 700;
    box-shadow: inset 0 0 0 1px rgba(96, 165, 250, 0.2);
  }

  :deep(.el-sub-menu.is-active > .el-sub-menu__title) {
    color: #1d4ed8;
    font-weight: 700;
  }

  :deep(.el-menu-item.is-active .z-menu-icon),
  :deep(.el-sub-menu.is-active > .el-sub-menu__title .z-menu-icon),
  :deep(.el-menu-item:hover .z-menu-icon),
  :deep(.el-sub-menu__title:hover .z-menu-icon) {
    color: #1d4ed8;
  }

  &.el-menu--collapse {
    .z-menu-icon {
      margin-right: 0;
    }
  }
}
</style>
