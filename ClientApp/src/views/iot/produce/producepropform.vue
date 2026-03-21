<template>
	<div>
		<el-drawer
			v-model="state.drawer"
			size="92%"
			class="produce-prop-drawer"
			append-to-body
			destroy-on-close
		>
			<ConsoleDrawerWorkspace
				eyebrow="Product Model"
				:title="state.dialogtitle"
				description="在这里维护产品抽象字段，统一字段名称、数据类型和数据侧，为字典、映射和后续设备接入提供稳定基础。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button @click="closeDialog">取消</el-button>
					<el-button plain :icon="Plus" @click="onAddRow">新增字段</el-button>
					<el-button type="primary" :icon="Check" :loading="saving" @click="save">保存模型</el-button>
				</template>

				<section class="produce-prop-layout">
					<article class="editor-card editor-card--main">
						<div class="editor-card__header">
							<div>
								<h3>字段建模工作区</h3>
								<p>字段名称定义抽象能力，数据类型决定存储方式，数据侧用于约束数据从设备还是平台流动。</p>
							</div>
						</div>

						<el-table :data="state.rows" class="editor-table" empty-text="暂无字段，点击新增字段开始建模。">
							<el-table-column label="字段名称" min-width="260">
								<template #default="{ row }">
									<el-input v-model="row.keyName" placeholder="例如 temperature" clearable />
								</template>
							</el-table-column>

							<el-table-column label="数据类型" min-width="220">
								<template #default="{ row }">
									<el-select v-model="row.type" placeholder="选择数据类型">
										<el-option
											v-for="item in dataTypeOptions"
											:key="item.value"
											:label="item.label"
											:value="item.value"
										/>
									</el-select>
								</template>
							</el-table-column>

							<el-table-column label="数据侧" min-width="220">
								<template #default="{ row }">
									<el-select v-model="row.dataSide" placeholder="选择数据侧">
										<el-option
											v-for="item in dataSideOptions"
											:key="item.value"
											:label="item.label"
											:value="item.value"
										/>
									</el-select>
								</template>
							</el-table-column>

							<el-table-column label="操作" width="100" fixed="right">
								<template #default="{ row }">
									<el-button text type="danger" @click="deleterow(row)">删除</el-button>
								</template>
							</el-table-column>
						</el-table>
					</article>

					<aside class="produce-prop-side">
						<article class="editor-card">
							<div class="editor-card__header">
								<div>
									<h3>建模建议</h3>
									<p>先用统一命名规范定义抽象字段，再进入字典和映射阶段补齐展示与路由逻辑。</p>
								</div>
							</div>
							<ul class="editor-tips">
								<li><strong>AnySide</strong> 适合需要双向同步的通用字段。</li>
								<li><strong>ClientSide</strong> 更适合设备上报类数据。</li>
								<li><strong>ServerSide</strong> 适合平台下发配置和控制类字段。</li>
							</ul>
						</article>

						<article class="editor-card">
							<div class="editor-card__header">
								<div>
									<h3>当前摘要</h3>
									<p>帮助你在编辑时快速确认模型规模和字段分布。</p>
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
import { v4 as uuidv4 } from 'uuid';
import { editProduceData, getProduceData } from '/@/api/produce';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';

interface ProducePropRow {
	id?: string;
	_rowId: string;
	keyName: string;
	dataSide: string;
	type: string;
}

interface ProducePropState {
	produceid: string;
	drawer: boolean;
	dialogtitle: string;
	rows: ProducePropRow[];
}

const dataTypeOptions = [
	{ value: 'Boolean', label: 'Boolean' },
	{ value: 'String', label: 'String' },
	{ value: 'Long', label: 'Long' },
	{ value: 'Double', label: 'Double' },
	{ value: 'Json', label: 'Json' },
	{ value: 'XML', label: 'XML' },
	{ value: 'Binary', label: 'Binary' },
	{ value: 'DateTime', label: 'DateTime' },
];

const dataSideOptions = [
	{ value: 'AnySide', label: 'AnySide' },
	{ value: 'ServerSide', label: 'ServerSide' },
	{ value: 'ClientSide', label: 'ClientSide' },
];

const emit = defineEmits(['close', 'submit']);
const saving = ref(false);

const state = reactive<ProducePropState>({
	produceid: '',
	drawer: false,
	dialogtitle: '产品属性模型',
	rows: [],
});

const badges = computed(() => [
	'抽象字段',
	`${state.rows.length} 个字段`,
	state.rows.length ? '支持继续扩展' : '等待开始建模',
]);

const metrics = computed(() => [
	{
		label: '字段总数',
		value: state.rows.length,
		hint: '当前产品模型中的抽象字段数量。',
		tone: 'primary' as const,
	},
	{
		label: '设备侧字段',
		value: state.rows.filter((item) => item.dataSide === 'ClientSide').length,
		hint: '通常用于设备主动上报的数据项。',
		tone: 'accent' as const,
	},
	{
		label: '平台侧字段',
		value: state.rows.filter((item) => item.dataSide === 'ServerSide').length,
		hint: '更适合控制、配置和平台下发场景。',
		tone: 'success' as const,
	},
	{
		label: '数据类型',
		value: new Set(state.rows.map((item) => item.type).filter(Boolean)).size,
		hint: '当前模型涉及的数据类型种类数量。',
		tone: 'warning' as const,
	},
]);

const summaryItems = computed(() => [
	{ label: '双向字段', value: state.rows.filter((item) => item.dataSide === 'AnySide').length },
	{ label: '已命名字段', value: state.rows.filter((item) => item.keyName.trim()).length },
	{ label: '待补充字段', value: state.rows.filter((item) => !item.keyName.trim() || !item.type || !item.dataSide).length },
	{ label: '时间类型字段', value: state.rows.filter((item) => item.type === 'DateTime').length },
]);

const openDialog = async (produceid: string) => {
	state.produceid = produceid;
	state.rows = [];

	try {
		const response = await getProduceData(produceid);
		state.rows = (response.data ?? []).map((item: any) => ({
			_rowId: uuidv4(),
			id: item.id,
			keyName: item.keyName ?? '',
			dataSide: item.dataSide ?? '',
			type: item.type ?? '',
		}));
	}
	catch {
		state.rows = [];
	}

	state.drawer = true;
};

const closeDialog = () => {
	state.drawer = false;
};

const onAddRow = () => {
	state.rows.push({
		_rowId: uuidv4(),
		id: '',
		keyName: '',
		dataSide: '',
		type: '',
	});
};

const deleterow = (row: ProducePropRow) => {
	state.rows = state.rows.filter((item) => item._rowId !== row._rowId);
};

const validateRows = () => {
	if (!state.rows.length) {
		ElMessage.warning('请至少新增一个字段后再保存');
		return false;
	}

	const invalidRow = state.rows.find((item) => !item.keyName.trim() || !item.dataSide || !item.type);
	if (invalidRow) {
		ElMessage.warning('请补全字段名称、数据类型和数据侧');
		return false;
	}

	return true;
};

const save = async () => {
	if (saving.value || !validateRows()) return;
	saving.value = true;

	try {
		const result = await editProduceData({
			produceId: state.produceid,
			produceData: state.rows.map((item) => ({
				id: item.id,
				keyName: item.keyName.trim(),
				dataSide: item.dataSide,
				type: item.type,
			})),
		});

		if (result.code === 10000) {
			ElMessage.success('产品属性模型已保存');
			state.drawer = false;
			emit('close', state.rows);
		}
		else {
			ElMessage.warning(`保存失败: ${result.msg}`);
			emit('close', state.rows);
		}
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
:deep(.produce-prop-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.produce-prop-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.produce-prop-layout {
	display: grid;
	grid-template-columns: minmax(0, 1.7fr) 320px;
	gap: 18px;
}

.produce-prop-side {
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
	.produce-prop-layout {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	:deep(.produce-prop-drawer) {
		width: 100% !important;
	}

	:deep(.produce-prop-drawer .el-drawer__body) {
		padding: 12px;
	}

	.editor-card {
		padding: 16px;
		border-radius: 20px;
	}
}
</style>
