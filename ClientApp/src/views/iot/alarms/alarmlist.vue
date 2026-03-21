<template>
	<div v-if="standaloneMode" class="alarm-page">
		<ConsolePageShell
			eyebrow="Alarm Workspace"
			title="告警处置与筛选中心"
			description="把告警时间范围、状态筛选、来源对象和处置动作收拢到统一的告警工作台中，便于从后台首页继续深入排查。"
			:badges="alarmBadges"
			:metrics="alarmMetrics"
		>
			<template #actions>
				<el-button type="primary" @click="getData">刷新告警</el-button>
				<el-button @click="resetFilters">重置筛选</el-button>
			</template>

			<div class="alarm-page__filter-card">
				<div class="alarm-page__section-head">
					<div>
						<div class="alarm-page__section-eyebrow">Alarm Filters</div>
						<h3>筛选条件</h3>
						<p>按时间、状态、级别和来源对象快速缩小告警范围，定位需要优先处理的问题。</p>
					</div>
				</div>

				<el-form size="default" label-width="100px" class="alarm-page__form">
					<el-row :gutter="24">
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="确认时间">
								<el-date-picker v-model="query.AckDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="清除时间">
								<el-date-picker v-model="query.ClearDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="开始时间">
								<el-date-picker v-model="query.StartDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
					</el-row>

					<el-row :gutter="24">
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="结束时间">
								<el-date-picker v-model="query.EndDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警类型">
								<el-input v-model="query.AlarmType" placeholder="请输入告警类型" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警状态">
								<el-select v-model="query.alarmStatus" placeholder="请选择告警状态">
									<el-option v-for="item in alarmStatusOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
					</el-row>

					<el-row :gutter="24">
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警级别">
								<el-select v-model="query.serverity" placeholder="请选择告警级别">
									<el-option v-for="item in serverityOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="来源类型">
								<el-select v-model="query.originatorType" placeholder="请选择来源类型">
									<el-option v-for="item in originatorTypeOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="来源对象">
								<el-select
									v-model="query.OriginatorId"
									placeholder="请选择来源对象"
									filterable
									remote
									reserve-keyword
									:remote-method="getOriginators"
									:loading="tableData.originatorloading"
								>
									<el-option v-for="item in tableData.originatorOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
					</el-row>

					<el-form-item class="alarm-page__form-actions">
						<el-button type="primary" @click="handleSearch">
							<el-icon><ele-Search /></el-icon>
							查询
						</el-button>
						<el-button @click="resetFilters">重置</el-button>
					</el-form-item>
				</el-form>
			</div>

			<div class="alarm-page__table-card">
				<div class="alarm-page__section-head">
					<div>
						<div class="alarm-page__section-eyebrow">Alarm Table</div>
						<h3>告警列表</h3>
						<p>支持确认、清除和展开来源对象详情，适合运维和项目交付场景下的快速处置。</p>
					</div>
				</div>

				<el-table :data="tableData.rows" row-key="id" v-loading="loading" class="alarm-page__table">
					<el-table-column type="expand">
						<template #default="props">
							<el-descriptions title="来源对象">
								<el-descriptions-item label="Id">{{ props.row.originator?.id }}</el-descriptions-item>
								<el-descriptions-item label="名称">{{ props.row.originator?.name }}</el-descriptions-item>
							</el-descriptions>
						</template>
					</el-table-column>
					<el-table-column prop="alarmType" label="告警类型" show-overflow-tooltip />
					<el-table-column prop="ackDateTime" label="创建时间" show-overflow-tooltip />
					<el-table-column prop="startDateTime" label="开始时间" show-overflow-tooltip />
					<el-table-column prop="endDateTime" label="结束时间" show-overflow-tooltip />
					<el-table-column prop="clearDateTime" label="清除时间" show-overflow-tooltip />
					<el-table-column prop="alarmStatus" label="告警状态" show-overflow-tooltip>
						<template #default="scope">
							<el-tag
								size="small"
								:style="{ color: 'white', borderColor: alarmStatusTAG.get(scope.row.alarmStatus)?.color }"
								:color="alarmStatusTAG.get(scope.row.alarmStatus)?.color"
								disable-transitions
							>
								{{ alarmStatusTAG.get(scope.row.alarmStatus)?.text }}
							</el-tag>
						</template>
					</el-table-column>

					<el-table-column prop="serverity" label="严重程度" show-overflow-tooltip>
						<template #default="scope">
							<el-tag
								size="small"
								:style="{ color: 'white', borderColor: serverityBadge.get(scope.row.serverity)?.color }"
								:color="serverityBadge.get(scope.row.serverity)?.color"
								disable-transitions
							>
								{{ serverityBadge.get(scope.row.serverity)?.text }}
							</el-tag>
						</template>
					</el-table-column>

					<el-table-column prop="originatorType" label="来源类型" show-overflow-tooltip>
						<template #default="scope">
							<el-tag
								size="small"
								:style="{ color: 'white', borderColor: originatorTypeTAG.get(scope.row.originatorType)?.color }"
								:color="originatorTypeTAG.get(scope.row.originatorType)?.color"
								disable-transitions
							>
								{{ originatorTypeTAG.get(scope.row.originatorType)?.text }}
							</el-tag>
						</template>
					</el-table-column>

					<el-table-column label="操作" show-overflow-tooltip width="200">
						<template #default="scope">
							<el-button
								v-if="scope.row.alarmStatus === 'Active_UnAck' || scope.row.alarmStatus === 'Cleared_UnAck'"
								size="small"
								text
								type="primary"
								@click="acquireAlarm(scope.row)"
							>
								确认告警
							</el-button>
							<el-button
								v-if="scope.row.alarmStatus === 'Active_Ack' || scope.row.alarmStatus === 'Active_UnAck'"
								size="small"
								text
								type="primary"
								@click="clearAlarm(scope.row)"
							>
								清除告警
							</el-button>
						</template>
					</el-table-column>
				</el-table>

				<el-pagination
					@size-change="onHandleSizeChange"
					@current-change="onHandleCurrentChange"
					class="alarm-page__pagination"
					:pager-count="5"
					:page-sizes="[10, 20, 30]"
					v-model:current-page="tableData.param.pageNum"
					background
					v-model:page-size="tableData.param.pageSize"
					layout="total, sizes, prev, pager, next, jumper"
					:total="tableData.total"
				/>
			</div>
		</ConsolePageShell>
	</div>

	<component :is="wrapper" v-else>
		<div class="alarm-page__embedded">
			<div class="alarm-page__filter-card is-embedded">
				<el-form size="default" label-width="100px" class="alarm-page__form">
					<el-row :gutter="24">
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="确认时间">
								<el-date-picker v-model="query.AckDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="清除时间">
								<el-date-picker v-model="query.ClearDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="开始时间">
								<el-date-picker v-model="query.StartDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
					</el-row>

					<el-row :gutter="24">
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="结束时间">
								<el-date-picker v-model="query.EndDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警类型">
								<el-input v-model="query.AlarmType" placeholder="请输入告警类型" />
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警状态">
								<el-select v-model="query.alarmStatus" placeholder="请选择告警状态">
									<el-option v-for="item in alarmStatusOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
					</el-row>

					<el-row :gutter="24">
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警级别">
								<el-select v-model="query.serverity" placeholder="请选择告警级别">
									<el-option v-for="item in serverityOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col v-if="!props.originator" :xs="24" :sm="12" :md="8">
							<el-form-item label="来源类型">
								<el-select v-model="query.originatorType" placeholder="请选择来源类型">
									<el-option v-for="item in originatorTypeOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col v-if="!props.originator" :xs="24" :sm="12" :md="8">
							<el-form-item label="来源对象">
								<el-select
									v-model="query.OriginatorId"
									placeholder="请选择来源对象"
									filterable
									remote
									reserve-keyword
									:remote-method="getOriginators"
									:loading="tableData.originatorloading"
								>
									<el-option v-for="item in tableData.originatorOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
					</el-row>

					<el-form-item class="alarm-page__form-actions">
						<el-button type="primary" @click="handleSearch">
							<el-icon><ele-Search /></el-icon>
							查询
						</el-button>
					</el-form-item>
				</el-form>
			</div>

			<div class="alarm-page__table-card is-embedded">
				<el-table :data="tableData.rows" row-key="id" v-loading="loading" class="alarm-page__table">
					<el-table-column type="expand">
						<template #default="props">
							<el-descriptions title="来源对象">
								<el-descriptions-item label="Id">{{ props.row.originator?.id }}</el-descriptions-item>
								<el-descriptions-item label="名称">{{ props.row.originator?.name }}</el-descriptions-item>
							</el-descriptions>
						</template>
					</el-table-column>
					<el-table-column prop="alarmType" label="告警类型" show-overflow-tooltip />
					<el-table-column prop="ackDateTime" label="创建时间" show-overflow-tooltip />
					<el-table-column prop="startDateTime" label="开始时间" show-overflow-tooltip />
					<el-table-column prop="endDateTime" label="结束时间" show-overflow-tooltip />
					<el-table-column prop="clearDateTime" label="清除时间" show-overflow-tooltip />
					<el-table-column prop="alarmStatus" label="告警状态" show-overflow-tooltip>
						<template #default="scope">
							<el-tag
								size="small"
								:style="{ color: 'white', borderColor: alarmStatusTAG.get(scope.row.alarmStatus)?.color }"
								:color="alarmStatusTAG.get(scope.row.alarmStatus)?.color"
								disable-transitions
							>
								{{ alarmStatusTAG.get(scope.row.alarmStatus)?.text }}
							</el-tag>
						</template>
					</el-table-column>

					<el-table-column prop="serverity" label="严重程度" show-overflow-tooltip>
						<template #default="scope">
							<el-tag
								size="small"
								:style="{ color: 'white', borderColor: serverityBadge.get(scope.row.serverity)?.color }"
								:color="serverityBadge.get(scope.row.serverity)?.color"
								disable-transitions
							>
								{{ serverityBadge.get(scope.row.serverity)?.text }}
							</el-tag>
						</template>
					</el-table-column>

					<el-table-column prop="originatorType" label="来源类型" show-overflow-tooltip>
						<template #default="scope">
							<el-tag
								size="small"
								:style="{ color: 'white', borderColor: originatorTypeTAG.get(scope.row.originatorType)?.color }"
								:color="originatorTypeTAG.get(scope.row.originatorType)?.color"
								disable-transitions
							>
								{{ originatorTypeTAG.get(scope.row.originatorType)?.text }}
							</el-tag>
						</template>
					</el-table-column>

					<el-table-column label="操作" show-overflow-tooltip width="200">
						<template #default="scope">
							<el-button
								v-if="scope.row.alarmStatus === 'Active_UnAck' || scope.row.alarmStatus === 'Cleared_UnAck'"
								size="small"
								text
								type="primary"
								@click="acquireAlarm(scope.row)"
							>
								确认告警
							</el-button>
							<el-button
								v-if="scope.row.alarmStatus === 'Active_Ack' || scope.row.alarmStatus === 'Active_UnAck'"
								size="small"
								text
								type="primary"
								@click="clearAlarm(scope.row)"
							>
								清除告警
							</el-button>
						</template>
					</el-table-column>
				</el-table>

				<el-pagination
					@size-change="onHandleSizeChange"
					@current-change="onHandleCurrentChange"
					class="alarm-page__pagination"
					:pager-count="5"
					:page-sizes="[10, 20, 30]"
					v-model:current-page="tableData.param.pageNum"
					background
					v-model:page-size="tableData.param.pageSize"
					layout="total, sizes, prev, pager, next, jumper"
					:total="tableData.total"
				/>
			</div>
		</div>
	</component>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { acquire, clear, getAlarmList, getoriginators } from '/@/api/alarm';
import type { appmessage } from '/@/api/iapiresult';
import {
	alarmStatusOptions,
	alarmStatusTAG,
	originatorTypeOptions,
	originatorTypeTAG,
	serverityBadge,
	serverityOptions,
} from '/@/views/iot/alarms/alarmSearchOptions';
import type { TableDataRow, TableDataState } from '/@/views/iot/alarms/model';

const loading = ref(false);
const props = defineProps({
	originator: {
		type: Object,
		default: null,
	},
	wrapper: {
		type: String,
		default: 'el-card',
	},
});

const standaloneMode = computed(() => !props.originator && props.wrapper === 'el-card');

const tableData = reactive<TableDataState>({
	originatorOptions: [],
	originatorloading: false,
	rows: [],
	total: 0,
	loading: false,
	param: {
		pageNum: 1,
		pageSize: 10,
	},
});

const query = reactive({
	AckDateTime: '',
	ClearDateTime: '',
	StartDateTime: '',
	EndDateTime: '',
	AlarmType: '',
	alarmStatus: '-1',
	serverity: '-1',
	originatorType: '-1',
	OriginatorId: '',
});

const activeAlarmCount = computed(() => tableData.rows.filter((item) => item.alarmStatus?.startsWith('Active')).length);
const pendingAckCount = computed(() =>
	tableData.rows.filter((item) => item.alarmStatus === 'Active_UnAck' || item.alarmStatus === 'Cleared_UnAck').length
);
const criticalAlarmCount = computed(() => tableData.rows.filter((item) => item.serverity === 'Critical').length);
const originatorTypeCount = computed(() => new Set(tableData.rows.map((item) => item.originatorType).filter(Boolean)).size);

const getOptionLabel = (options: Array<{ value: string; label: string }>, value: string) =>
	options.find((item) => item.value === value)?.label || '全部';

const alarmBadges = computed(() => [
	`状态 ${getOptionLabel(alarmStatusOptions, query.alarmStatus)}`,
	`级别 ${getOptionLabel(serverityOptions, query.serverity)}`,
	`来源 ${getOptionLabel(originatorTypeOptions, query.originatorType)}`,
]);

const alarmMetrics = computed(() => [
	{
		label: '告警总量',
		value: tableData.total,
		hint: '当前筛选条件下返回的告警总数。',
		tone: 'primary' as const,
	},
	{
		label: '活跃告警',
		value: activeAlarmCount.value,
		hint: '仍处于 Active 状态的告警数量。',
		tone: 'warning' as const,
	},
	{
		label: '待确认',
		value: pendingAckCount.value,
		hint: '仍需人工确认的告警。',
		tone: 'accent' as const,
	},
	{
		label: '高优先级',
		value: criticalAlarmCount.value,
		hint: `当前页涉及 ${originatorTypeCount.value} 类来源对象。`,
		tone: 'success' as const,
	},
]);

const onHandleSizeChange = (val: number) => {
	tableData.param.pageSize = val;
	tableData.param.pageNum = 1;
	getData();
};

const onHandleCurrentChange = (val: number) => {
	tableData.param.pageNum = val;
	getData();
};

const handleSearch = () => {
	tableData.param.pageNum = 1;
	getData();
};

const resetFilters = () => {
	query.AckDateTime = '';
	query.ClearDateTime = '';
	query.StartDateTime = '';
	query.EndDateTime = '';
	query.AlarmType = '';
	query.alarmStatus = '-1';
	query.serverity = '-1';
	query.originatorType = '-1';
	query.OriginatorId = '';
	tableData.param.pageNum = 1;
	getData();
};

const getOriginators = async (val: string) => {
	tableData.originatorloading = true;

	try {
		const res = await getoriginators({
			OriginatorType: query.originatorType,
			originatorName: val,
		});
		tableData.originatorOptions = res.data.map((item: any) => ({ value: item.id, label: item.name }));
	} finally {
		tableData.originatorloading = false;
	}
};

const clearAlarm = (row: TableDataRow) => {
	clear(row.id!).then((res: appmessage<boolean>) => {
		if (res && res.data) {
			ElMessage.success('清除成功');
			getData();
		} else {
			ElMessage.warning(`清除失败：${res.msg}`);
		}
	});
};

const acquireAlarm = (row: TableDataRow) => {
	acquire(row.id!).then((res: appmessage<boolean>) => {
		if (res && res.data) {
			ElMessage.success('确认成功');
			getData();
		} else {
			ElMessage.warning(`确认失败：${res.msg}`);
		}
	});
};

const getData = async () => {
	const params: Record<string, any> = {
		offset: tableData.param.pageNum - 1,
		limit: tableData.param.pageSize,
		alarmStatus: query.alarmStatus,
		originatorId: query.OriginatorId,
		ClearDateTime: query.ClearDateTime,
		AckDateTime: query.AckDateTime,
		StartDateTime: query.StartDateTime,
		originatorType: query.originatorType,
		AlarmType: query.AlarmType,
		Name: '',
		EndDateTime: query.EndDateTime,
		OriginatorName: '',
		serverity: query.serverity,
	};

	if (props.originator) {
		const { id, deviceType } = props.originator as Record<string, string>;
		params.originatorId = id;
		const originatorType = originatorTypeOptions.find((item) => item.key === deviceType);
		query.originatorType = originatorType ? originatorType.value : '-1';
		params.originatorType = query.originatorType;
	}

	try {
		loading.value = true;
		const res = await getAlarmList(params);
		tableData.rows = res.data.rows;
		tableData.total = res.data.total;
	} finally {
		loading.value = false;
	}
};

watch(
	() => props.originator,
	() => {
		tableData.param.pageNum = 1;
		getData();
	}
);

onMounted(() => {
	getData();
});
</script>

<style lang="scss" scoped>
.alarm-page,
.alarm-page__embedded {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

.alarm-page__filter-card,
.alarm-page__table-card {
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.alarm-page__filter-card.is-embedded,
.alarm-page__table-card.is-embedded {
	border-radius: 24px;
	box-shadow: none;
}

.alarm-page__section-eyebrow {
	margin-bottom: 10px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.alarm-page__section-head h3 {
	margin: 0;
	color: #123b6d;
	font-size: 22px;
	letter-spacing: -0.04em;
}

.alarm-page__section-head p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.75;
}

.alarm-page__form {
	margin-top: 18px;
}

.alarm-page__form-actions {
	margin-bottom: 0;
}

.alarm-page__table {
	margin-top: 18px;
}

.alarm-page__pagination {
	margin-top: 18px;
}

:deep(.alarm-page__table .el-table) {
	border-radius: 20px;
	overflow: hidden;
}

:deep(.alarm-page__table th.el-table__cell) {
	background: #f8fbff;
}

@media (max-width: 767px) {
	.alarm-page__filter-card,
	.alarm-page__table-card {
		padding: 18px;
		border-radius: 22px;
	}
}
</style>
