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
		radial-gradient(circle at top left, rgba(96, 165, 250, 0.14), transparent 24%),
		radial-gradient(circle at bottom right, rgba(125, 211, 252, 0.2), transparent 28%),
		linear-gradient(180deg, #f7fbff 0%, #eef6ff 100%);
}

.iotsharp-shell__mesh {
	position: absolute;
	inset: 0;
	background-image:
		linear-gradient(rgba(148, 163, 184, 0.05) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.05) 1px, transparent 1px);
	background-size: 42px 42px;
	opacity: 0.55;
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
