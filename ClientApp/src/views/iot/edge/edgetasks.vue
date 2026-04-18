<template>
	<div class="edge-task-page">
		<ConsolePageShell
			eyebrow="Edge Task Console"
			title="Edge 任务闭环"
			description="统一查看 Dispatch、Receipt 与 History，确认平台任务是否已被 Gateway / PiXiu 消费并完成回执。"
			:badges="badges"
		>
			<template #actions>
				<el-input v-model="filters.name" placeholder="筛选设备名称" clearable class="edge-task-page__filter" />
				<el-select v-model="filters.runtimeType" placeholder="运行时" clearable class="edge-task-page__filter">
					<el-option label="Gateway" value="gateway" />
					<el-option label="Pixiu" value="pixiu-runtime" />
				</el-select>
				<el-select v-model="filters.status" placeholder="状态" clearable class="edge-task-page__filter">
					<el-option label="Pending" value="Pending" />
					<el-option label="Accepted" value="Accepted" />
					<el-option label="Succeeded" value="Succeeded" />
					<el-option label="Failed" value="Failed" />
				</el-select>
				<el-button type="primary" @click="loadTasks">刷新任务</el-button>
			</template>

			<div class="edge-task-page__card">
				<el-table :data="rows" stripe v-loading="loading">
					<el-table-column prop="deviceName" label="设备" min-width="180" show-overflow-tooltip />
					<el-table-column prop="runtimeType" label="运行时" min-width="120" />
					<el-table-column prop="currentStatus" label="当前状态" min-width="120" />
					<el-table-column prop="lastUpdatedAt" label="最后更新时间" min-width="180" show-overflow-tooltip />
					<el-table-column prop="taskId" label="任务ID" min-width="260" show-overflow-tooltip />
					<el-table-column label="操作" width="120">
						<template #default="scope">
							<el-button link type="primary" @click="openTimeline(scope.row)">详情</el-button>
						</template>
					</el-table-column>
				</el-table>
			</div>
		</ConsolePageShell>

		<el-drawer v-model="drawerVisible" title="任务时间线" size="50%">
			<div v-if="currentTask" class="edge-task-page__timeline">
				<div class="edge-task-page__summary">
					<strong>{{ currentTask.deviceName }}</strong>
					<span>{{ currentTask.taskId }}</span>
				</div>
				<el-timeline>
					<el-timeline-item v-for="item in currentTask.events" :key="`${item.category}-${item.at}`" :timestamp="item.at">
						<div class="edge-task-page__timeline-card">
							<div class="edge-task-page__timeline-head">
								<strong>{{ item.category }}</strong>
								<span>{{ item.status }}</span>
							</div>
							<p>{{ item.message || '无消息' }}</p>
							<el-input type="textarea" :rows="4" :model-value="item.payload" readonly />
						</div>
					</el-timeline-item>
				</el-timeline>
			</div>
		</el-drawer>
	</div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { edgeApi, type EdgeTaskTimeline } from '/@/api/edge';

const api = edgeApi();
const loading = ref(false);
const rows = ref<EdgeTaskTimeline[]>([]);
const drawerVisible = ref(false);
const currentTask = ref<EdgeTaskTimeline | null>(null);
const filters = reactive({
	name: '',
	status: '',
	runtimeType: '',
});

const badges = computed(() => [`记录 ${rows.value.length} 条`]);

const openTimeline = (row: EdgeTaskTimeline) => {
	currentTask.value = row;
	drawerVisible.value = true;
};

const loadTasks = async () => {
	loading.value = true;
	try {
		const response = await api.getTaskList({ offset: 0, limit: 100, ...filters });
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

.edge-task-page__filter {
	width: 180px;
	margin-right: 12px;
}

.edge-task-page__summary {
	display: flex;
	flex-direction: column;
	gap: 6px;
	margin-bottom: 18px;
}

.edge-task-page__timeline-card {
	padding: 12px 14px;
	border-radius: 16px;
	background: rgba(248, 250, 252, 0.9);
	border: 1px solid rgba(226, 232, 240, 0.9);
}

.edge-task-page__timeline-head {
	display: flex;
	justify-content: space-between;
	margin-bottom: 8px;
}
</style>