<template>
	<div v-if="isShowBreadcrumb && breadcrumbList.length" class="layout-page-breadcrumb">
		<el-breadcrumb class="layout-page-breadcrumb__list">
			<transition-group name="breadcrumb">
				<el-breadcrumb-item v-for="(v, k) in breadcrumbList" :key="!v.meta.tagsViewName ? v.meta.title : v.meta.tagsViewName">
					<span v-if="k === breadcrumbList.length - 1" class="layout-page-breadcrumb__current">
						<SvgIcon v-if="themeConfig.isBreadcrumbIcon" :name="v.meta.icon" class="layout-page-breadcrumb__icon" />
						<span v-if="!v.meta.tagsViewName">{{ $t(v.meta.title) }}</span>
						<span v-else>{{ v.meta.tagsViewName }}</span>
					</span>
					<a v-else class="layout-page-breadcrumb__link" @click.prevent="onBreadcrumbClick(v)">
						<SvgIcon v-if="themeConfig.isBreadcrumbIcon" :name="v.meta.icon" class="layout-page-breadcrumb__icon" />
						{{ $t(v.meta.title) }}
					</a>
				</el-breadcrumb-item>
			</transition-group>
		</el-breadcrumb>
	</div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, reactive, toRefs } from 'vue';
import { onBeforeRouteUpdate, useRoute, useRouter } from 'vue-router';
import other from '/@/utils/other';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useRoutesList } from '/@/stores/routesList';

export default defineComponent({
	name: 'layoutBreadcrumb',
	setup() {
		const stores = useRoutesList();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const { routesList } = storeToRefs(stores);
		const route = useRoute();
		const router = useRouter();
		const state = reactive({
			breadcrumbList: [] as any[],
			routeSplit: [] as string[],
			routeSplitFirst: '',
			routeSplitIndex: 1,
		});

		const isShowBreadcrumb = computed(() => {
			initRouteSplit(route.path);
			const { layout, isBreadcrumb } = themeConfig.value;
			if (layout === 'classic' || layout === 'transverse') return false;
			return isBreadcrumb;
		});

		const onBreadcrumbClick = (item: any) => {
			const { redirect, path } = item;
			if (redirect) router.push(redirect);
			else router.push(path);
		};

		const getBreadcrumbList = (arr: any[]) => {
			arr.forEach((item: any) => {
				state.routeSplit.forEach((splitItem: string, _index: number, arrs: string[]) => {
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
			if (!themeConfig.value.isBreadcrumb || !routesList.value.length) return;

			state.breadcrumbList = [routesList.value[0]];
			state.routeSplit = path.split('/');
			state.routeSplit.shift();
			state.routeSplitFirst = `/${state.routeSplit[0]}`;
			state.routeSplitIndex = 1;

			getBreadcrumbList(routesList.value);

			if ((route.name === 'dashboard' || route.name === 'home') && state.breadcrumbList.length > 0) {
				state.breadcrumbList.shift();
			}

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
			themeConfig,
			isShowBreadcrumb,
			onBreadcrumbClick,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-page-breadcrumb {
	display: flex;
	align-items: center;
	min-height: 24px;
	padding: 2px 0;
}

.layout-page-breadcrumb__list {
	min-width: 0;
}

.layout-page-breadcrumb__link {
	display: inline-flex;
	align-items: center;
	gap: 6px;
	color: #86909c;
	font-weight: 500;
	text-decoration: none;
	transition: color 0.2s ease;

	&:hover {
		color: #165dff;
	}
}

.layout-page-breadcrumb__current {
	display: inline-flex;
	align-items: center;
	gap: 6px;
	color: #1d2129;
	font-weight: 600;
}

.layout-page-breadcrumb__icon {
	font-size: 14px;
}

:deep(.el-breadcrumb__separator) {
	color: #c9cdd4;
}

@media (max-width: 767px) {
	.layout-page-breadcrumb {
		padding-top: 0;
	}
}
</style>
