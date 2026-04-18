<template>
	<div class="edge-task-page">
		<ConsolePageShell
			eyebrow="Edge Task Console"
			title="Edge 任务闭环"
			description="统一查看 Dispatch、Receipt 与 History，确认平台任务是否已被 Gateway / PiXiu 消费并完成回执。"
			:badges="badges"
		>
			<template #actions>
				<el-button type="primary" @click="loadTasks">刷新任务</el-button>
			</template>

			<div class="edge-task-page__card">
				<el-table :data="rows" stripe v-loading="loading">
					<el-table-column prop="at" label="时间" min-width="180" show-overflow-tooltip />
					<el-table-column prop="runtimeType" label="运行时" min-width="120" />
					<el-table-column prop="category" label="类别" min-width="100" />
					<el-table-column prop="status" label="状态" min-width="120" />
					<el-table-column prop="taskId" label="任务ID" min-width="260" show-overflow-tooltip />
					<el-table-column prop="message" label="消息" min-width="220" show-overflow-tooltip />
				</el-table>
			</div>
		</ConsolePageShell>
	</div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { edgeApi } from '/@/api/edge';

const api = edgeApi();
const loading = ref(false);
const rows = ref<any[]>([]);

const badges = computed(() => [`记录 ${rows.value.length} 条`]);

const loadTasks = async () => {
	loading.value = true;
	try {
		const response = await api.getTaskList({ offset: 0, limit: 100 });
		rows.value = response?.data?.rows ?? [];
	} finally {
		loading.value = false;
	}
};

onMounted(() => {
	loadTasks();
});
</script>

<style lang="scss" scoped>
.edge-task-page__card {
	padding: 20px;
	border-radius: 24px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(255, 255, 255, 0.96);
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}
</style>