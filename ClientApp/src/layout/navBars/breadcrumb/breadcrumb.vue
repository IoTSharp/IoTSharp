<template>
	<div v-if="isShowBreadcrumb" class="layout-navbars-breadcrumb">
		<div class="layout-navbars-breadcrumb__toggle" @click="onThemeConfigChange">
			<SvgIcon :name="themeConfig.isCollapse ? 'ele-Expand' : 'ele-Fold'" :size="16" />
		</div>
		<el-breadcrumb class="layout-navbars-breadcrumb__list">
			<transition-group name="breadcrumb">
				<el-breadcrumb-item v-for="(v, k) in breadcrumbList" :key="!v.meta.tagsViewName ? v.meta.title : v.meta.tagsViewName">
					<span v-if="k === breadcrumbList.length - 1" class="layout-navbars-breadcrumb__current">
						<SvgIcon v-if="themeConfig.isBreadcrumbIcon" :name="v.meta.icon" class="layout-navbars-breadcrumb__icon" />
						<div v-if="!v.meta.tagsViewName">{{ $t(v.meta.title) }}</div>
						<div v-else>{{ v.meta.tagsViewName }}</div>
					</span>
					<a v-else class="layout-navbars-breadcrumb__link" @click.prevent="onBreadcrumbClick(v)">
						<SvgIcon v-if="themeConfig.isBreadcrumbIcon" :name="v.meta.icon" class="layout-navbars-breadcrumb__icon" />
						{{ $t(v.meta.title) }}
					</a>
				</el-breadcrumb-item>
			</transition-group>
		</el-breadcrumb>
	</div>
</template>

<script lang="ts">
import { toRefs, reactive, computed, onMounted, defineComponent } from 'vue';
import { onBeforeRouteUpdate, useRoute, useRouter } from 'vue-router';
import { Local } from '/@/utils/storage';
import other from '/@/utils/other';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useRoutesList } from '/@/stores/routesList';

interface BreadcrumbState {
	breadcrumbList: Array<any>;
	routeSplit: Array<string>;
	routeSplitFirst: string;
	routeSplitIndex: number;
}

export default defineComponent({
	name: 'layoutBreadcrumb',
	setup() {
		const stores = useRoutesList();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const { routesList } = storeToRefs(stores);
		const route = useRoute();
		const router = useRouter();
		const state = reactive<BreadcrumbState>({
			breadcrumbList: [],
			routeSplit: [],
			routeSplitFirst: '',
			routeSplitIndex: 1,
		});

		const isShowBreadcrumb = computed(() => {
			initRouteSplit(route.path);
			const { layout, isBreadcrumb } = themeConfig.value;
			if (layout === 'classic' || layout === 'transverse') return false;
			return isBreadcrumb;
		});

		const onBreadcrumbClick = (v: any) => {
			const { redirect, path } = v;
			if (redirect) router.push(redirect);
			else router.push(path);
		};

		const onThemeConfigChange = () => {
			themeConfig.value.isCollapse = !themeConfig.value.isCollapse;
			Local.remove('themeConfig');
			Local.set('themeConfig', themeConfig.value);
		};

		const getBreadcrumbList = (arr: Array<string>) => {
			arr.forEach((item: any) => {
				state.routeSplit.forEach((v: any, k: number, arrs: any) => {
					if (state.routeSplitFirst === item.path) {
						state.routeSplitFirst += `/${arrs[state.routeSplitIndex]}`;
						state.breadcrumbList.push(item);
						state.routeSplitIndex++;
						if (item.children) getBreadcrumbList(item.children);
					}
				});
			});
		};

		const initRouteSplit = (path: string) => {
			if (!themeConfig.value.isBreadcrumb || !routesList.value.length) return false;
			state.breadcrumbList = [routesList.value[0]];
			state.routeSplit = path.split('/');
			state.routeSplit.shift();
			state.routeSplitFirst = `/${state.routeSplit[0]}`;
			state.routeSplitIndex = 1;
			getBreadcrumbList(routesList.value);
			if ((route.name === 'dashboard' || route.name === 'home') && state.breadcrumbList.length > 0) state.breadcrumbList.shift();
			if (state.breadcrumbList.length > 0) {
				state.breadcrumbList[state.breadcrumbList.length - 1].meta.tagsViewName = other.setTagsViewNameI18n(route);
			}
		};

		onMounted(() => {
			initRouteSplit(route.path);
		});

		onBeforeRouteUpdate((to) => {
			initRouteSplit(to.path);
		});

		return {
			onThemeConfigChange,
			isShowBreadcrumb,
			themeConfig,
			onBreadcrumbClick,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb {
	flex: 1;
	min-width: 0;
	height: inherit;
	display: flex;
	align-items: center;
	gap: 14px;
}

.layout-navbars-breadcrumb__toggle {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 38px;
	height: 38px;
	border-radius: 12px;
	background: #f5f9ff;
	color: #335476;
	cursor: pointer;
	flex-shrink: 0;
	transition:
		background 0.2s ease,
		color 0.2s ease;

	&:hover {
		background: #eaf3ff;
		color: #2563eb;
	}
}

.layout-navbars-breadcrumb__list {
	min-width: 0;
}

.layout-navbars-breadcrumb__current {
	display: inline-flex;
	align-items: center;
	gap: 6px;
	color: #123b6d;
	font-weight: 700;
}

.layout-navbars-breadcrumb__link {
	color: #6c7f93;
	font-weight: 500;
	text-decoration: none;
	transition: color 0.2s ease;

	&:hover {
		color: #2563eb;
	}
}

.layout-navbars-breadcrumb__icon {
	font-size: 14px;
}

:deep(.el-breadcrumb__separator) {
	color: #9cb2c9;
}

@media (max-width: 767px) {
	.layout-navbars-breadcrumb__list {
		display: none;
	}
}
</style>
