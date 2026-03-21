<template>
	<div class="iotsharp-shell">
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
	min-height: 100%;
	background: #f2f3f5;
}

.iotsharp-shell__container {
	height: 100vh;
	min-height: 100vh;
	background: #f2f3f5;
}

.iotsharp-shell__header {
	flex: 0 0 auto;
}

.iotsharp-shell__body {
	flex: 1 1 auto;
	min-height: 0;
	background: #f2f3f5;
}

.iotsharp-shell__workspace {
	flex: 1 1 auto;
	min-width: 0;
	min-height: 0;
	padding: 0 16px 16px;
	background: #f2f3f5;
}

@media (max-width: 767px) {
	.iotsharp-shell__workspace {
		padding: 0 10px 10px;
	}
}
</style>
