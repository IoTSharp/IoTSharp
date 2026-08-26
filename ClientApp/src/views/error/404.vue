<template>
	<ConsoleErrorState
		code="404"
		title="没有找到这个页面"
		description="页面地址可能已经变更，或当前导航没有提供该功能入口。返回运行概览后可继续工作。"
		action-label="返回运行概览"
		@action="onGoHome"
	/>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import ConsoleErrorState from '/@/components/console/ConsoleErrorState.vue';

const router = useRouter();
const onGoHome = () => void router.push('/dashboard');
</script>
