<template>
	<div>
		<el-drawer
			v-model="state.drawer"
			size="96%"
			class="produce-mapping-drawer"
			append-to-body
			destroy-on-close
		>
			<ConsoleDrawerWorkspace
				eyebrow="Product Mapping"
				:title="state.dialogTitle"
				description="把产品抽象字段和实际设备字段在同一工作区内完成关联，后续上报属性或遥测时就能按这套映射自动路由。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button @click="closeDialog">取消</el-button>
					<el-button plain :icon="Plus" @click="addRow">新增映射</el-button>
					<el-button type="primary" :icon="Check" :loading="saving" @click="save">保存映射</el-button>
				</template>

				<section class="produce-mapping-layout">
					<article class="editor-card editor-card--main">
						<div class="editor-card__header">
							<div>
								<h3>映射工作区</h3>
								<p>左侧选择产品抽象字段，右侧绑定具体设备与字段名称，适合在多设备场景下逐步完善路由规则。</p>
							</div>
						</div>

						<el-table :data="state.mappings" class="editor-table" empty-text="暂无映射，点击新增映射开始配置。">
							<el-table-column label="产品字段" min-width="220">
								<template #default="{ row }">
									<el-select v-model="row.produceKeyName" placeholder="选择产品字段" clearable filterable>
										<el-option
											v-for="key in state.produceKeys"
											:key="key.keyName"
											:label="key.keyName"
											:value="key.keyName"
										>
											<div class="mapping-option">
												<span>{{ key.keyName }}</span>
												<el-tag size="small" :type="key.dataSide === 'ClientSide' ? 'success' : 'info'">
													{{ key.dataSide }}
												</el-tag>
											</div>
										</el-option>
									</el-select>
								</template>
							</el-table-column>

							<el-table-column label="数据类别" min-width="180">
								<template #default="{ row }">
									<el-select v-model="row.dataCatalog" placeholder="选择数据类别" @change="onCatalogChange(row)">
										<el-option
											v-for="item in catalogOptions"
											:key="item.value"
											:label="item.label"
											:value="item.value"
										/>
									</el-select>
								</template>
							</el-table-column>

							<el-table-column label="关联设备" min-width="220">
								<template #default="{ row }">
									<el-select
										v-model="row.deviceId"
										placeholder="选择设备"
										clearable
										filterable
										@change="onDeviceChange(row)"
									>
										<el-option
											v-for="device in state.devices"
											:key="device.id"
											:label="device.name"
											:value="device.id"
										/>
									</el-select>
								</template>
							</el-table-column>

							<el-table-column label="设备字段" min-width="220">
								<template #default="{ row }">
									<el-select
										v-if="getDeviceKeys(row.deviceId, row.dataCatalog).length"
										v-model="row.deviceKeyName"
										placeholder="选择或输入设备字段"
										filterable
										allow-create
										default-first-option
										clearable
									>
										<el-option
											v-for="key in getDeviceKeys(row.deviceId, row.dataCatalog)"
											:key="key"
											:label="key"
											:value="key"
										/>
									</el-select>
									<el-input v-else v-model="row.deviceKeyName" placeholder="输入设备字段名称" clearable />
								</template>
							</el-table-column>

							<el-table-column label="备注" min-width="180">
								<template #default="{ row }">
									<el-input v-model="row.description" placeholder="补充路由说明" clearable />
								</template>
							</el-table-column>

							<el-table-column label="操作" width="100" fixed="right">
								<template #default="{ $index }">
									<el-button text type="danger" @click="removeRow($index)">删除</el-button>
								</template>
							</el-table-column>
						</el-table>
					</article>

					<aside class="produce-mapping-side">
						<article class="editor-card">
							<div class="editor-card__header">
								<div>
									<h3>映射说明</h3>
									<p>先确定抽象字段，再绑定设备和具体字段名称，能让同类设备共享同一套产品模型。</p>
								</div>
							</div>
							<ul class="editor-tips">
								<li><strong>遥测数据</strong> 更适合实时上报和时序类字段。</li>
								<li><strong>属性数据</strong> 更适合配置、状态和静态档案类字段。</li>
								<li>如果右侧设备暂时没有历史字段，也可以直接手动输入目标字段名。</li>
							</ul>
						</article>

						<article class="editor-card">
							<div class="editor-card__header">
								<div>
									<h3>当前摘要</h3>
									<p>帮助你快速判断抽象字段和设备字段的覆盖情况。</p>
								</div>
							</div>
							<div class="side-summary">
								<div v-for="item in summaryItems" :key="item.label" class="side-summary__item">
									<span>{{ item.label }}</span>
									<strong>{{ item.value }}</strong>
								</div>
							</div>
						</article>
					</aside>
				</section>
			</ConsoleDrawerWorkspace>
		</el-drawer>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { Check, Plus } from '@element-plus/icons-vue';
import { NIL as NIL_UUID, v4 as uuidv4 } from 'uuid';
import {
	type ProduceDataMappingDto,
	getProduceData,
	getProduceDataMappings,
	saveProduceDataMappings,
} from '/@/api/produce';
import { deviceApi } from '/@/api/devices';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';

const deviceApis = deviceApi();

interface MappingRow extends ProduceDataMappingDto {
	_rowId: string;
}

interface ProduceKey {
	keyName: string;
	dataSide: string;
	type: string;
}

interface DeviceInfo {
	id: string;
	name: string;
}

interface MappingState {
	drawer: boolean;
	dialogTitle: string;
	produceId: string;
	produceKeys: ProduceKey[];
	devices: DeviceInfo[];
	deviceKeysCache: Record<string, { telemetry: string[]; attribute: string[] }>;
	mappings: MappingRow[];
}

const catalogOptions = [
	{ value: 'TelemetryData', label: '遥测数据' },
	{ value: 'AttributeData', label: '属性数据' },
];

const emit = defineEmits(['close', 'submit']);
const saving = ref(false);

const state = reactive<MappingState>({
	drawer: false,
	dialogTitle: '产品数据映射',
	produceId: NIL_UUID,
	produceKeys: [],
	devices: [],
	deviceKeysCache: {},
	mappings: [],
});

const badges = computed(() => [
	'字段路由',
	`${state.produceKeys.length} 个抽象字段`,
	`${state.devices.length} 台候选设备`,
]);

const metrics = computed(() => [
	{
		label: '映射总数',
		value: state.mappings.length,
		hint: '当前产品已经建立的数据映射数量。',
		tone: 'primary' as const,
	},
	{
		label: '关联设备',
		value: new Set(state.mappings.map((item) => item.deviceId).filter(Boolean)).size,
		hint: '已有映射覆盖到的设备数量。',
		tone: 'accent' as const,
	},
	{
		label: '抽象字段',
		value: state.produceKeys.length,
		hint: '来自产品模型的可映射抽象字段数量。',
		tone: 'success' as const,
	},
	{
		label: '缓存字段',
		value: Object.values(state.deviceKeysCache).reduce((total, item) => total + item.attribute.length + item.telemetry.length, 0),
		hint: '已提前拉取的设备字段数量，便于快速选择。',
		tone: 'warning' as const,
	},
]);

const summaryItems = computed(() => [
	{ label: '遥测映射', value: state.mappings.filter((item) => item.dataCatalog === 'TelemetryData').length },
	{ label: '属性映射', value: state.mappings.filter((item) => item.dataCatalog === 'AttributeData').length },
	{ label: '已绑定设备', value: state.mappings.filter((item) => item.deviceId).length },
	{ label: '待补全映射', value: state.mappings.filter((item) => !item.produceKeyName || !item.deviceId || !item.deviceKeyName).length },
]);

const openDialog = async (produceId: string, devices: DeviceInfo[]) => {
	state.produceId = produceId;
	state.devices = devices ?? [];
	state.deviceKeysCache = {};
	state.mappings = [];

	try {
		const produceResponse = await getProduceData(produceId);
		state.produceKeys = (produceResponse.data ?? []).map((item: any) => ({
			keyName: item.keyName,
			dataSide: item.dataSide,
			type: item.type,
		}));
	}
	catch {
		state.produceKeys = [];
	}

	try {
		const mappingResponse = await getProduceDataMappings(produceId);
		state.mappings = (mappingResponse.data ?? []).map((item: ProduceDataMappingDto) => ({
			_rowId: uuidv4(),
			id: item.id,
			produceKeyName: item.produceKeyName,
			dataCatalog: item.dataCatalog,
			deviceId: item.deviceId,
			deviceKeyName: item.deviceKeyName,
			description: item.description ?? '',
		}));

		const usedDeviceIds = [...new Set(state.mappings.map((item) => item.deviceId).filter(Boolean))];
		for (const deviceId of usedDeviceIds) {
			await loadDeviceKeys(deviceId);
		}
	}
	catch {
		state.mappings = [];
	}

	state.drawer = true;
};

const closeDialog = () => {
	state.drawer = false;
};

const addRow = () => {
	state.mappings.push({
		_rowId: uuidv4(),
		id: NIL_UUID,
		produceKeyName: '',
		dataCatalog: 'TelemetryData',
		deviceId: '',
		deviceKeyName: '',
		description: '',
	});
};

const removeRow = (index: number) => {
	state.mappings.splice(index, 1);
};

const onCatalogChange = (row: MappingRow) => {
	row.deviceKeyName = '';
};

const onDeviceChange = async (row: MappingRow) => {
	if (!row.deviceId) {
		row.deviceKeyName = '';
		return;
	}

	await loadDeviceKeys(row.deviceId);
	row.deviceKeyName = '';
};

const loadDeviceKeys = async (deviceId: string) => {
	if (!deviceId || state.deviceKeysCache[deviceId]) return;

	try {
		const [attrResponse, telemetryResponse] = await Promise.all([
			deviceApis.getDeviceAttributes(deviceId),
			deviceApis.getDeviceLatestTelemetry(deviceId),
		]);

		state.deviceKeysCache[deviceId] = {
			attribute: (attrResponse.data ?? []).map((item: any) => item.keyName as string).filter(Boolean),
			telemetry: (telemetryResponse.data ?? []).map((item: any) => item.keyName as string).filter(Boolean),
		};
	}
	catch {
		state.deviceKeysCache[deviceId] = { attribute: [], telemetry: [] };
	}
};

const getDeviceKeys = (deviceId: string, catalog: string) => {
	if (!deviceId || !state.deviceKeysCache[deviceId]) return [];
	return catalog === 'AttributeData'
		? state.deviceKeysCache[deviceId].attribute
		: state.deviceKeysCache[deviceId].telemetry;
};

const validateMappings = () => {
	const invalidRow = state.mappings.find((item) => !item.produceKeyName || !item.dataCatalog || !item.deviceId || !item.deviceKeyName);
	if (invalidRow) {
		ElMessage.warning('请补全产品字段、数据类别、关联设备和设备字段');
		return false;
	}

	return true;
};

const save = async () => {
	if (saving.value || !validateMappings()) return;
	saving.value = true;

	const payload = {
		produceId: state.produceId,
		mappings: state.mappings.map((item) => ({
			id: item.id,
			produceKeyName: item.produceKeyName,
			dataCatalog: item.dataCatalog,
			deviceId: item.deviceId,
			deviceKeyName: item.deviceKeyName,
			description: item.description ?? '',
		})),
	};

	try {
		const result = await saveProduceDataMappings(payload as any);
		if (result.code === 10000) {
			ElMessage.success('映射保存成功');
			state.drawer = false;
			emit('submit', payload);
		}
		else {
			ElMessage.warning(`映射保存失败: ${result.msg}`);
		}
	}
	catch (error: any) {
		ElMessage.error(`映射保存出错: ${error.message ?? error}`);
	}
	finally {
		saving.value = false;
	}
};

defineExpose({
	openDialog,
	closeDialog,
});
</script>

<style lang="scss" scoped>
:deep(.produce-mapping-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.produce-mapping-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.produce-mapping-layout {
	display: grid;
	grid-template-columns: minmax(0, 1.85fr) 320px;
	gap: 18px;
}

.produce-mapping-side {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.editor-card {
	padding: 22px;
	border-radius: 24px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background:
		radial-gradient(circle at top right, rgba(59, 130, 246, 0.08), transparent 32%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
	min-width: 0;
}

.editor-card__header {
	margin-bottom: 16px;

	h3 {
		margin: 0 0 8px;
		color: #123b6d;
		font-size: 20px;
		letter-spacing: -0.04em;
	}

	p {
		margin: 0;
		color: #64748b;
		font-size: 13px;
		line-height: 1.75;
	}
}

.editor-table {
	border-radius: 20px;
	overflow: hidden;
}

.editor-card :deep(.el-table th.el-table__cell) {
	background: #f8fbff;
}

.editor-card :deep(.el-input__wrapper),
.editor-card :deep(.el-select__wrapper) {
	border-radius: 14px;
}

.mapping-option {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 10px;
}

.editor-tips {
	margin: 0;
	padding-left: 18px;
	color: #475569;
	font-size: 13px;
	line-height: 1.8;
}

.side-summary {
	display: grid;
	gap: 12px;
}

.side-summary__item {
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(191, 219, 254, 0.72);
	background: rgba(255, 255, 255, 0.78);
}

.side-summary__item span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.side-summary__item strong {
	display: block;
	margin-top: 8px;
	color: #123b6d;
	font-size: 18px;
	font-weight: 700;
}

@media (max-width: 1080px) {
	.produce-mapping-layout {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	:deep(.produce-mapping-drawer) {
		width: 100% !important;
	}

	:deep(.produce-mapping-drawer .el-drawer__body) {
		padding: 12px;
	}

	.editor-card {
		padding: 16px;
		border-radius: 20px;
	}
}
</style>
