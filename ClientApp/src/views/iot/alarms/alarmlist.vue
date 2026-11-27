<template>
	<div v-if="standaloneMode" class="alarm-page console-list-page">
		<ConsolePageShell
			eyebrow="Alarm Workspace"
			title="设备告警"
			description="筛选告警并核对严重程度和处置状态。"
			:metrics="isNarrow || !loaded || loadFailed ? [] : alarmMetrics"
		>
			<template #actions>
				<el-button :loading="loading" @click="getData">刷新告警</el-button>
			</template>

			<div class="alarm-page__filter-card">
				<el-form size="small" label-width="74px" class="alarm-page__form" @submit.prevent="handleSearch">
					<el-row :gutter="12">
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警状态">
								<el-select v-model="query.alarmStatus" placeholder="请选择告警状态">
									<el-option v-for="item in alarmStatusOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="告警级别">
								<el-select v-model="query.serverity" placeholder="请选择告警级别">
									<el-option v-for="item in serverityOptions" :key="item.value" :label="item.label" :value="item.value" />
								</el-select>
							</el-form-item>
						</el-col>
						<el-col :xs="24" :sm="12" :md="8">
							<el-form-item label="开始时间">
								<el-date-picker v-model="query.StartDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
							</el-form-item>
						</el-col>
					</el-row>
					<div v-show="advancedFiltersOpen" id="alarm-advanced-filters">
						<el-row :gutter="12">
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
								<el-form-item label="结束时间">
									<el-date-picker v-model="query.EndDateTime" type="daterange" start-placeholder="开始时间" end-placeholder="结束时间" />
								</el-form-item>
							</el-col>
						</el-row>
						<el-row :gutter="12">
							<el-col :xs="24" :sm="12" :md="8">
								<el-form-item label="告警类型">
									<el-input v-model="query.AlarmType" placeholder="请输入告警类型" />
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
					</div>
					<div class="alarm-page__filter-actions">
						<el-button type="primary" native-type="submit" :loading="loading">查询</el-button>
						<el-button @click="resetFilters">重置</el-button>
						<el-button link :aria-expanded="advancedFiltersOpen" aria-controls="alarm-advanced-filters" @click="advancedFiltersOpen = !advancedFiltersOpen">
							{{ advancedFiltersOpen ? '收起更多筛选' : '更多筛选' }}
						</el-button>
					</div>
				</el-form>
				<p v-if="appliedFilterSummary.length" class="alarm-page__applied">
					已应用：{{ appliedFilterSummary.join(' · ') }}
				</p>
			</div>

			<div class="alarm-page__table-card console-list-panel">
				<p v-if="loadFailed" class="alarm-page__notice" role="alert">
					告警加载失败。<el-button link type="primary" @click="getData">重试</el-button>
				</p>
				<div v-else-if="isNarrow" class="alarm-page__mobile" :aria-busy="loading" v-loading="loading">
					<p class="alarm-page__notice" role="status">{{ resultSummary }}</p>
					<ul v-if="tableData.rows.length" class="alarm-page__results" aria-label="告警结果">
						<li v-for="row in tableData.rows" :key="row.id">
							<details>
								<summary>
									<span class="alarm-page__identity">{{ row.alarmType || '未知告警' }}</span>
									<span class="alarm-page__meta">
										{{ row.originator?.name || row.originatorId || originatorTypeTAG.get(row.originatorType ?? '') || '未知来源' }}
										<span v-if="row.startDateTime"> · {{ formatAlarmDate(row.startDateTime) }}</span>
									</span>
									<span class="alarm-page__states">
										<span class="console-status" :class="badgeClass(serverityBadge.get(row.serverity ?? '')?.tone)">{{ serverityBadge.get(row.serverity ?? '')?.text || '未知级别' }}</span>
										<span class="console-status" :class="badgeClass(alarmStatusTAG.get(row.alarmStatus ?? '')?.tone)">{{ alarmStatusTAG.get(row.alarmStatus ?? '')?.text || '未知状态' }}</span>
									</span>
								</summary>
								<div class="alarm-page__mobile-detail">
									<p>来源 ID：{{ row.originatorId || '—' }}</p>
									<p>确认时间：{{ formatAlarmDate(row.ackDateTime) }}</p>
									<p>结束时间：{{ formatAlarmDate(row.endDateTime) }}</p>
									<p>清除时间：{{ formatAlarmDate(row.clearDateTime) }}</p>
									<div class="alarm-page__states">
										<el-button v-if="canAcknowledge(row)" @click="acquireAlarm(row)">确认告警</el-button>
										<el-button v-if="canClear(row)" @click="clearAlarm(row)">清除告警</el-button>
									</div>
								</div>
							</details>
						</li>
					</ul>
				</div>
				<div v-else class="console-list-table" role="region" aria-label="告警列表" tabindex="0">
					<el-table :data="tableData.rows" row-key="id" v-loading="loading" size="small" :empty-text="resultSummary">
						<el-table-column type="expand">
							<template #default="props">
								<el-descriptions title="来源对象">
									<el-descriptions-item label="Id">{{ props.row.originator?.id }}</el-descriptions-item>
									<el-descriptions-item label="名称">{{ props.row.originator?.name }}</el-descriptions-item>
								</el-descriptions>
							</template>
						</el-table-column>
						<el-table-column prop="alarmType" label="告警类型" show-overflow-tooltip />
						<el-table-column prop="ackDateTime" label="确认时间" show-overflow-tooltip />
						<el-table-column prop="startDateTime" label="开始时间" show-overflow-tooltip />
						<el-table-column prop="endDateTime" label="结束时间" show-overflow-tooltip />
						<el-table-column prop="clearDateTime" label="清除时间" show-overflow-tooltip />
						<el-table-column prop="alarmStatus" label="告警状态" show-overflow-tooltip>
							<template #default="scope">
								<span class="console-status" :class="badgeClass(alarmStatusTAG.get(scope.row.alarmStatus)?.tone)">{{ alarmStatusTAG.get(scope.row.alarmStatus)?.text || '未知状态' }}</span>
							</template>
						</el-table-column>

						<el-table-column prop="serverity" label="严重程度" show-overflow-tooltip>
							<template #default="scope">
								<span class="console-status" :class="badgeClass(serverityBadge.get(scope.row.serverity)?.tone)">{{ serverityBadge.get(scope.row.serverity)?.text || '未知级别' }}</span>
							</template>
						</el-table-column>

						<el-table-column prop="originatorType" label="来源类型" show-overflow-tooltip>
							<template #default="scope">
								<span class="console-status console-status--category">{{ originatorTypeTAG.get(scope.row.originatorType) || '未知' }}</span>
							</template>
						</el-table-column>

						<el-table-column label="操作" show-overflow-tooltip width="200">
							<template #default="scope">
								<el-button
									v-if="canAcknowledge(scope.row)"
									size="small"
									text
									type="primary"
									@click="acquireAlarm(scope.row)"
								>
									确认告警
								</el-button>
								<el-button
									v-if="canClear(scope.row)"
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
				</div>

				<el-pagination
					v-if="loaded && !loadFailed"
					@update:page-size="onHandleSizeChange"
					@update:current-page="onHandleCurrentChange"
					class="console-list-pagination"
					size="small"
					:pager-count="5"
					:page-sizes="[10, 20, 30]"
					:current-page="tableData.param.pageNum"
					background
					:page-size="tableData.param.pageSize"
					:layout="isNarrow ? 'prev, pager, next' : 'total, sizes, prev, pager, next, jumper'"
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
					<el-table-column prop="ackDateTime" label="确认时间" show-overflow-tooltip />
					<el-table-column prop="startDateTime" label="开始时间" show-overflow-tooltip />
					<el-table-column prop="endDateTime" label="结束时间" show-overflow-tooltip />
					<el-table-column prop="clearDateTime" label="清除时间" show-overflow-tooltip />
					<el-table-column prop="alarmStatus" label="告警状态" show-overflow-tooltip>
						<template #default="scope">
							<span class="console-status" :class="badgeClass(alarmStatusTAG.get(scope.row.alarmStatus)?.tone)">{{ alarmStatusTAG.get(scope.row.alarmStatus)?.text || '未知状态' }}</span>
						</template>
					</el-table-column>

					<el-table-column prop="serverity" label="严重程度" show-overflow-tooltip>
						<template #default="scope">
							<span class="console-status" :class="badgeClass(serverityBadge.get(scope.row.serverity)?.tone)">{{ serverityBadge.get(scope.row.serverity)?.text || '未知级别' }}</span>
						</template>
					</el-table-column>

					<el-table-column prop="originatorType" label="来源类型" show-overflow-tooltip>
						<template #default="scope">
							<span class="console-status console-status--category">{{ originatorTypeTAG.get(scope.row.originatorType) || '未知' }}</span>
						</template>
					</el-table-column>

					<el-table-column label="操作" show-overflow-tooltip width="200">
						<template #default="scope">
							<el-button
								v-if="canAcknowledge(scope.row)"
								size="small"
								text
								type="primary"
								@click="acquireAlarm(scope.row)"
							>
								确认告警
							</el-button>
							<el-button
								v-if="canClear(scope.row)"
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
import { useMediaQuery } from '@vueuse/core';
import dayjs from 'dayjs';
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
const loaded = ref(false);
const loadFailed = ref(false);
const advancedFiltersOpen = ref(false);
const isNarrow = useMediaQuery('(max-width: 767px)');
let latestRequest = 0;
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

const emptyQuery = {
	AckDateTime: null as Date[] | null,
	ClearDateTime: null as Date[] | null,
	StartDateTime: null as Date[] | null,
	EndDateTime: null as Date[] | null,
	AlarmType: '',
	alarmStatus: '-1',
	serverity: '-1',
	originatorType: '-1',
	OriginatorId: '',
};
const query = reactive({ ...emptyQuery });
const appliedQuery = ref({ ...emptyQuery });
const appliedOriginatorLabel = ref('');

const activeAlarmCount = computed(() => tableData.rows.filter((item) => item.alarmStatus?.startsWith('Active')).length);
const pendingAckCount = computed(() =>
	tableData.rows.filter((item) => item.alarmStatus === 'Active_UnAck' || item.alarmStatus === 'Cleared_UnAck').length
);
const criticalAlarmCount = computed(() => tableData.rows.filter((item) => item.serverity === 'Critical').length);
const originatorTypeCount = computed(() => new Set(tableData.rows.map((item) => item.originatorType).filter(Boolean)).size);

const badgeClass = (tone?: string) => tone ? 'console-status--' + tone : '';
const canAcknowledge = (row: TableDataRow) => row.alarmStatus === 'Active_UnAck' || row.alarmStatus === 'Cleared_UnAck';
const canClear = (row: TableDataRow) => row.alarmStatus === 'Active_Ack' || row.alarmStatus === 'Active_UnAck';
const formatAlarmDate = (value?: string) => value && dayjs(value).isValid() ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '—';

const appliedFilterSummary = computed(() => {
	const filters = appliedQuery.value;
	const rangeLabel = (label: string, value: Date[] | null) => value?.length === 2
		? label + ' ' + value.map((date) => dayjs(date).format('YYYY-MM-DD')).join(' 至 ')
		: '';
	return [
		alarmStatusOptions.find((item) => item.value !== '-1' && item.value === filters.alarmStatus)?.label,
		serverityOptions.find((item) => item.value !== '-1' && item.value === filters.serverity)?.label,
		originatorTypeOptions.find((item) => item.value !== '-1' && item.value === filters.originatorType)?.label,
		rangeLabel('开始时间', filters.StartDateTime),
		rangeLabel('结束时间', filters.EndDateTime),
		rangeLabel('确认时间', filters.AckDateTime),
		rangeLabel('清除时间', filters.ClearDateTime),
		filters.AlarmType,
		appliedOriginatorLabel.value,
	].filter(Boolean);
});

const resultSummary = computed(() => {
	if (loading.value) return '正在加载告警…';
	if (tableData.total) return '共 ' + tableData.total + ' 条告警';
	return appliedFilterSummary.value.length ? '没有符合条件的告警' : '暂无告警';
});

const alarmMetrics = computed(() => [
	{
		label: '告警总量',
		value: tableData.total,
		hint: '当前筛选条件下返回的告警总数。',
		tone: 'primary' as const,
	},
	{
		label: '本页活跃',
		value: activeAlarmCount.value,
		hint: '本页仍处于激活状态的告警。',
		tone: 'warning' as const,
	},
	{
		label: '本页待确认',
		value: pendingAckCount.value,
		hint: '仍需人工确认的告警。',
		tone: 'accent' as const,
	},
	{
		label: '本页高优先级',
		value: criticalAlarmCount.value,
		hint: `当前页涉及 ${originatorTypeCount.value} 类来源对象。`,
		tone: 'warning' as const,
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

/** 提交筛选快照，分页和刷新继续使用当前已应用条件。 */
const handleSearch = () => {
	appliedQuery.value = { ...query, AlarmType: query.AlarmType.trim() };
	appliedOriginatorLabel.value = tableData.originatorOptions.find((item) => item.value === query.OriginatorId)?.label || query.OriginatorId;
	tableData.param.pageNum = 1;
	advancedFiltersOpen.value = false;
	getData();
};

const resetFilters = () => {
	Object.assign(query, emptyQuery);
	handleSearch();
};

watch(() => query.originatorType, () => {
	query.OriginatorId = '';
	tableData.originatorOptions = [];
});

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

/** 加载已应用条件的分页，仅最新请求可以更新结果。 */
const getData = async () => {
	const requestId = ++latestRequest;
	const filters = appliedQuery.value;
	const params: Record<string, any> = {
		offset: tableData.param.pageNum - 1,
		limit: tableData.param.pageSize,
		AlarmStatus: Number(filters.alarmStatus),
		Serverity: Number(filters.serverity),
		OriginatorType: Number(filters.originatorType),
		AlarmType: filters.AlarmType || undefined,
		Name: '',
		OriginatorName: '',
	};

	// 可空日期和 Guid 筛选项没有值时省略，避免把空字符串交给 ASP.NET Core 绑定器。
	const dateRanges = {
		AckDateTime: filters.AckDateTime,
		ClearDateTime: filters.ClearDateTime,
		StartDateTime: filters.StartDateTime,
		EndDateTime: filters.EndDateTime,
	};
	Object.entries(dateRanges).forEach(([key, value]) => {
		if (Array.isArray(value) && value.length === 2) {
			params[key] = value;
		}
	});

	if (filters.OriginatorId) {
		params.OriginatorId = filters.OriginatorId;
	}

	if (props.originator) {
		const { id, deviceType } = props.originator as Record<string, string>;
		params.OriginatorId = id;
		const originatorType = originatorTypeOptions.find((item) => item.key === deviceType);
		params.OriginatorType = Number(originatorType?.value ?? '-1');
	}

	try {
		loading.value = true;
		loadFailed.value = false;
		const res = await getAlarmList(params);
		if (requestId !== latestRequest) return;
		tableData.rows = res.data.rows;
		tableData.total = res.data.total;
		loaded.value = true;
	} catch {
		if (requestId === latestRequest) loadFailed.value = true;
	} finally {
		if (requestId === latestRequest) loading.value = false;
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
.alarm-page__embedded {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

.alarm-page__filter-card,
.alarm-page__table-card {
	padding: 10px 16px;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-surface);
	box-shadow: var(--iotsharp-shadow-panel);
}

.alarm-page__filter-card.is-embedded,
.alarm-page__table-card.is-embedded {
	padding: 20px 22px;
	border-radius: 24px;
	box-shadow: none;
}

.alarm-page .alarm-page__filter-card {
	flex-shrink: 0;
}

.alarm-page__embedded .alarm-page__form {
	margin-top: 18px;
}

.alarm-page .alarm-page__form :deep(.el-form-item) {
	margin-bottom: 8px;
}

.alarm-page .alarm-page__form :deep(.el-date-editor),
.alarm-page .alarm-page__form :deep(.el-select) {
	width: 100%;
}

.alarm-page .alarm-page__form .alarm-page__form-actions {
	margin-bottom: 0;
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

.alarm-page__filter-actions,
.alarm-page__states {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	gap: 8px;
}

.alarm-page__filter-actions .el-button + .el-button,
.alarm-page__states .el-button + .el-button {
	margin-left: 0;
}

.alarm-page__notice,
.alarm-page__applied {
	margin: 8px 0;
	color: var(--iotsharp-text-soft);
	font-size: 13px;
	overflow-wrap: anywhere;
}

.alarm-page__results {
	margin: 0;
	padding: 0;
	list-style: none;
}

.alarm-page__results li + li {
	border-top: 1px solid var(--iotsharp-border);
}

.alarm-page__results summary {
	padding: 9px 2px;
	cursor: pointer;
}

.alarm-page__identity {
	color: var(--iotsharp-ink);
	font-weight: 650;
	overflow-wrap: anywhere;
}

.alarm-page__meta,
.alarm-page__mobile-detail {
	display: block;
	margin: 3px 0 7px;
	color: var(--iotsharp-text-soft);
	font-size: 13px;
	overflow-wrap: anywhere;
}

.alarm-page__mobile-detail {
	padding: 0 10px 8px;
}

@media (max-width: 767px) {
	.alarm-page__filter-card,
	.alarm-page__table-card {
		padding: 10px;
	}
}

</style>
