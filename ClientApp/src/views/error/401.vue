<template>
	<ConsoleErrorState
		code="401"
		title="当前账号无权访问"
		description="该页面不在当前账号的授权范围内。请重新登录其他账号，或联系租户管理员调整权限。"
		action-label="重新登录"
		@action="onSetAuth"
	/>
</template>

<script setup lang="ts">
import ConsoleErrorState from '/@/components/console/ConsoleErrorState.vue';
import { Session } from '/@/utils/storage';

const onSetAuth = () => {
	Session.clear();
	window.location.reload();
};
</script>
