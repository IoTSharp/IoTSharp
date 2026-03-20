<template>
	<div class="iotsharp-shell">
		<div class="iotsharp-shell__mesh"></div>
		<el-container class="layout-container iotsharp-shell__container" direction="vertical">
			<Header class="iotsharp-shell__header" />
			<el-container class="iotsharp-shell__body">
				<Aside />
				<el-container class="flex-center iotsharp-shell__workspace">
					<Main />
				</el-container>
			</el-container>
			<el-backtop target=".layout-main .el-scrollbar__wrap"></el-backtop>
		</el-container>
	</div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import Aside from '/@/layout/component/aside.vue';
import Header from '/@/layout/component/header.vue';
import Main from '/@/layout/component/main.vue';

export default defineComponent({
	name: 'layoutDefaults',
	components: { Aside, Header, Main },
});
</script>

<style scoped lang="scss">
.iotsharp-shell {
	position: relative;
	min-height: 100%;
	background:
		radial-gradient(circle at top left, rgba(88, 100, 255, 0.08), transparent 22%),
		radial-gradient(circle at bottom right, rgba(142, 196, 255, 0.18), transparent 26%),
		linear-gradient(180deg, #f8fbff 0%, #f1f6fc 100%);
}

.iotsharp-shell__mesh {
	position: absolute;
	inset: 0;
	background-image:
		linear-gradient(rgba(148, 163, 184, 0.04) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.04) 1px, transparent 1px);
	background-size: 36px 36px;
	opacity: 0.65;
	pointer-events: none;
}

.iotsharp-shell__container {
	position: relative;
	z-index: 1;
	height: 100vh;
	min-height: 100vh;
}

.iotsharp-shell__header {
	flex: 0 0 auto;
}

.iotsharp-shell__body {
	flex: 1 1 auto;
	min-height: 0;
}

.iotsharp-shell__workspace {
	flex: 1 1 auto;
	min-width: 0;
	min-height: 0;
	padding: 0 18px 18px 0;
}

@media (max-width: 767px) {
	.iotsharp-shell__workspace {
		padding: 0 10px 10px 0;
	}
}
</style>
