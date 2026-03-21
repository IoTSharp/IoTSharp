<template>
	<div>
		<el-drawer
			v-model="state.drawer"
			size="96%"
			class="produce-dict-drawer"
			append-to-body
			destroy-on-close
		>
			<ConsoleDrawerWorkspace
				eyebrow="Product Dictionary"
				:title="state.dialogtitle"
				description="继续补齐字段的展示名、单位、默认值和标签元数据，让产品模型从抽象字段进入可展示、可解释、可映射的阶段。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button @click="closeDialog">取消</el-button>
					<el-button plain :icon="Plus" @click="onAddRow">新增词条</el-button>
					<el-button type="primary" :icon="Check" :loading="saving" @click="save">保存字典</el-button>
				</template>

				<section class="produce-dict-layout">
					<article class="editor-card editor-card--main">
						<div class="editor-card__header">
							<div>
								<h3>字段元数据工作区</h3>
								<p>这里负责描述字段如何被展示、如何转换以及如何被外部系统识别，是产品建模中最接近业务语义的一层。</p>
							</div>
						</div>

						<el-table :data="state.rows" class="editor-table" empty-text="暂无字典词条，点击新增词条开始补充。">
							<el-table-column label="字段名称" min-width="180">
								<template #default="{ row }">
									<el-input v-model="row.keyName" placeholder="字段名称" clearable />
								</template>
							</el-table-column>

							<el-table-column label="展示名称" min-width="180">
								<template #default="{ row }">
									<el-input v-model="row.displayName" placeholder="展示名称" clearable />
								</template>
							</el-table-column>

							<el-table-column label="单位" min-width="120">
								<template #default="{ row }">
									<el-input v-model="row.unit" placeholder="℃ / kWh" clearable />
								</template>
							</el-table-column>

							<el-table-column label="单位换算表达式" min-width="220">
								<template #default="{ row }">
									<el-input v-model="row.unitExpression" placeholder="例如 value / 10" clearable />
								</template>
							</el-table-column>

							<el-table-column label="启用换算" width="110" align="center">
								<template #default="{ row }">
									<el-switch v-model="row.unitConvert" />
								</template>
							</el-table-column>

							<el-table-column label="字段备注" min-width="180">
								<template #default="{ row }">
									<el-input v-model="row.keyDesc" placeholder="字段说明" clearable />
								</template>
							</el-table-column>

							<el-table-column label="默认值" min-width="140">
								<template #default="{ row }">
									<el-input v-model="row.defaultValue" placeholder="默认值" clearable />
								</template>
							</el-table-column>

							<el-table-column label="默认展示" width="110" align="center">
								<template #default="{ row }">
									<el-switch v-model="row.display" />
								</template>
							</el-table-column>

							<el-table-column label="位置名称" min-width="140">
								<template #default="{ row }">
									<el-input v-model="row.place0" placeholder="位置名称" clearable />
								</template>
							</el-table-column>

							<el-table-column label="Tag" min-width="180">
								<template #default="{ row }">
									<el-input v-model="row.tag" placeholder="外部系统标识" clearable />
								</template>
							</el-table-column>

							<el-table-column label="数据类型" min-width="180">
								<template #default="{ row }">
									<el-select v-model="row.dataType" placeholder="选择数据类型">
										<el-option
											v-for="item in dataTypeOptions"
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

					<aside class="produce-dict-side">
						<article class="editor-card">
							<div class="editor-card__header">
								<div>
									<h3>编辑提醒</h3>
									<p>把字典当作字段的业务解释层来维护，后面映射到具体设备时会更顺手。</p>
								</div>
							</div>
							<ul class="editor-tips">
								<li><strong>展示名称</strong> 面向控制台和看板，建议业务化命名。</li>
								<li><strong>单位换算</strong> 适合协议原值和展示值不一致的场景。</li>
								<li><strong>Tag</strong> 建议保持稳定，便于对接外部系统。</li>
							</ul>
						</article>

						<article class="editor-card">
							<div class="editor-card__header">
								<div>
									<h3>当前摘要</h3>
									<p>快速感知字典完善程度，避免保存后还要回头补字段。</p>
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
import { editProduceDictionary, getProduceDictionary } from '/@/api/produce';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';

interface ProduceDictionaryRow {
	_rowId: string;
	id: string;
	keyName: string;
	displayName: string;
	unit: string;
	unitExpression: string;
	unitConvert: boolean;
	keyDesc: string;
	defaultValue: string;
	display: boolean;
	place0: string;
	tag: string;
	dataType: string;
}

interface ProduceDictionaryState {
	produceid: string;
	drawer: boolean;
	dialogtitle: string;
	rows: ProduceDictionaryRow[];
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

const emit = defineEmits(['close', 'submit']);
const saving = ref(false);

const state = reactive<ProduceDictionaryState>({
	produceid: '',
	drawer: false,
	dialogtitle: '产品字典',
	rows: [],
});

const badges = computed(() => [
	'字段元数据',
	`${state.rows.length} 个词条`,
	state.rows.some((item) => item.display) ? '含默认展示字段' : '待设置展示字段',
]);

const metrics = computed(() => [
	{
		label: '词条总数',
		value: state.rows.length,
		hint: '当前产品字典中维护的字段词条数量。',
		tone: 'primary' as const,
	},
	{
		label: '默认展示',
		value: state.rows.filter((item) => item.display).length,
		hint: '这些字段更适合在控制台优先展示。',
		tone: 'accent' as const,
	},
	{
		label: '启用换算',
		value: state.rows.filter((item) => item.unitConvert).length,
		hint: '需要按表达式处理的展示字段数量。',
		tone: 'success' as const,
	},
	{
		label: '数据类型',
		value: new Set(state.rows.map((item) => item.dataType).filter(Boolean)).size,
		hint: '已覆盖的数据类型种类数量。',
		tone: 'warning' as const,
	},
]);

const summaryItems = computed(() => [
	{ label: '已命名词条', value: state.rows.filter((item) => item.keyName.trim()).length },
	{ label: '已配置 Tag', value: state.rows.filter((item) => item.tag.trim()).length },
	{ label: '含默认值', value: state.rows.filter((item) => item.defaultValue.trim()).length },
	{ label: '待补充词条', value: state.rows.filter((item) => !item.keyName.trim() || !item.tag.trim()).length },
]);

const openDialog = async (produceid: string) => {
	state.produceid = produceid;
	state.rows = [];

	try {
		const response = await getProduceDictionary(produceid);
		state.rows = (response.data ?? []).map((item: any) => ({
			_rowId: uuidv4(),
			id: item.id ?? NIL_UUID,
			keyName: item.keyName ?? '',
			displayName: item.displayName ?? '',
			unit: item.unit ?? '',
			unitExpression: item.unitExpression ?? '',
			unitConvert: !!item.unitConvert,
			keyDesc: item.keyDesc ?? '',
			defaultValue: item.defaultValue ?? '',
			display: !!item.display,
			place0: item.place0 ?? '',
			tag: item.tag ?? '',
			dataType: item.dataType ?? '',
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
		id: NIL_UUID,
		keyName: '',
		displayName: '',
		unit: '',
		unitExpression: '',
		unitConvert: false,
		keyDesc: '',
		defaultValue: '',
		display: false,
		place0: '',
		tag: '',
		dataType: '',
	});
};

const deleterow = (row: ProduceDictionaryRow) => {
	state.rows = state.rows.filter((item) => item._rowId !== row._rowId);
};

const validateRows = () => {
	const invalidRow = state.rows.find((item) => !item.keyName.trim() || !item.tag.trim());
	if (invalidRow) {
		ElMessage.warning('请至少补全字段名称和 Tag 后再保存');
		return false;
	}

	return true;
};

const save = async () => {
	if (saving.value || !validateRows()) return;
	saving.value = true;

	try {
		const result = await editProduceDictionary({
			produceId: state.produceid,
			produceDictionaryData: state.rows.map((item) => ({
				id: item.id,
				keyName: item.keyName.trim(),
				displayName: item.displayName.trim(),
				unit: item.unit.trim(),
				unitExpression: item.unitExpression.trim(),
				unitConvert: item.unitConvert,
				keyDesc: item.keyDesc.trim(),
				defaultValue: item.defaultValue.trim(),
				display: item.display,
				place0: item.place0.trim(),
				tag: item.tag.trim(),
				dataType: item.dataType,
			})),
		});

		if (result.code === 10000) {
			ElMessage.success('产品字典已保存');
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
});
</script>

<style lang="scss" scoped>
:deep(.produce-dict-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.produce-dict-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.produce-dict-layout {
	display: grid;
	grid-template-columns: minmax(0, 1.85fr) 320px;
	gap: 18px;
}

.produce-dict-side {
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
	.produce-dict-layout {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	:deep(.produce-dict-drawer) {
		width: 100% !important;
	}

	:deep(.produce-dict-drawer .el-drawer__body) {
		padding: 12px;
	}

	.editor-card {
		padding: 16px;
		border-radius: 20px;
	}
}
</style>
