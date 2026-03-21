<template>
	<div class="rule-page">
		<template v-if="state.viewstate === 'list'">
			<ConsolePageShell
				eyebrow="Rule Workspace"
				title="规则编排与运行入口"
				description="围绕规则名称筛选、挂载类型、节点展开和设计入口重新组织规则列表，让规则管理页更接近工作台式后台体验。"
				:badges="listBadges"
				:metrics="listMetrics"
			>
				<template #actions>
					<el-button type="primary" @click="getData">刷新规则</el-button>
					<el-button @click="create">新增规则</el-button>
				</template>

				<div class="rule-page__card">
					<div class="rule-page__card-head">
						<div>
							<div class="rule-page__card-eyebrow">Rule Table</div>
							<h3>规则列表</h3>
							<p>支持名称查询、节点展开、设计器与模拟测试，帮助你从列表直接切入规则配置流程。</p>
						</div>
					</div>

					<div class="rule-page__filters">
						<el-input v-model="query.name" placeholder="请输入规则名称" clearable @keyup.enter="handleSearch" />
						<el-button type="primary" @click="handleSearch">
							<el-icon><ele-Search /></el-icon>
							查询
						</el-button>
						<el-button type="success" @click="create">
							<el-icon><ele-FolderAdd /></el-icon>
							新增规则
						</el-button>
					</div>

					<el-table
						:data="state.tableData.rows"
						row-key="ruleId"
						table-layout="auto"
						v-loading="state.tableData.loading"
						@expand-change="expandchange"
					>
						<el-table-column type="expand">
							<template #default="props">
								<el-table :data="props.row.flows" class="rule-page__subtable">
									<el-table-column label="节点名称" prop="flowname" />
									<el-table-column label="类型" prop="flowType" />
									<el-table-column label="执行器" prop="nodeProcessClass" />
									<el-table-column label="脚本类型" prop="nodeProcessScriptType" />
								</el-table>
							</template>
						</el-table-column>

						<el-table-column prop="name" label="规则名称" show-overflow-tooltip />

						<el-table-column prop="mountType" label="挂载类型" show-overflow-tooltip>
							<template #default="scope">
								<el-tag
									effect="dark"
									size="small"
									:color="mountTypes.get(scope.row.mountType)?.color"
									disable-transitions
								>
									{{ mountTypes.get(scope.row.mountType)?.text }}
								</el-tag>
							</template>
						</el-table-column>

						<el-table-column prop="creatTime" label="创建时间" show-overflow-tooltip />

						<el-table-column label="操作" show-overflow-tooltip width="240">
							<template #default="scope">
								<el-button size="small" text type="primary" icon="Edit" @click="edit(scope.row.ruleId)">修改</el-button>
								<el-button size="small" text type="success" icon="Memo" @click="design(scope.row.ruleId)">设计</el-button>
								<el-button size="small" text icon="DocumentChecked" @click="simulator(scope.row.ruleId)">测试</el-button>
								<el-button size="small" text type="danger" icon="Delete" @click="onTabelRowDel(scope.row)">删除</el-button>
							</template>
						</el-table-column>
					</el-table>

					<el-pagination
						@size-change="onHandleSizeChange"
						@current-change="onHandleCurrentChange"
						class="rule-page__pagination"
						:pager-count="5"
						:page-sizes="[10, 20, 30]"
						v-model:current-page="state.tableData.param.pageNum"
						background
						v-model:page-size="state.tableData.param.pageSize"
						layout="total, sizes, prev, pager, next, jumper"
						:total="state.tableData.total"
					/>
				</div>
			</ConsolePageShell>
		</template>

		<addflow ref="addformRef" @close="close" @submit="submit" />
		<flowdesigner v-if="state.viewstate === 'designer'" :ruleId="state.currentruleId" @close="ondesignerclose"></flowdesigner>
		<flowsimulator v-if="state.viewstate === 'simulator'" :ruleId="state.currentruleId" @close="onsimulatorclose"></flowsimulator>
	</div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { NIL as NIL_UUID } from 'uuid';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import addflow from './addflow.vue';
import flowdesigner from './flowdesigner.vue';
import flowsimulator from './flowsimulator.vue';
import { ruleApi } from '/@/api/flows';
import type { appmessage } from '/@/api/iapiresult';

interface TableDataRow {
	ruleId?: string;
	creatTime?: string;
	createId?: string;
	mountType?: string;
	name?: string;
	parentRuleId?: string;
	ruleDesc?: string;
	creator?: string;
	ruleType?: string;
	flows?: Array<SubTableDataRow>;
}

interface SubTableDataRow {
	flowId?: string;
	createDate?: string;
	flowType?: string;
	nodeProcessClass?: string;
	nodeProcessScript?: string;
	nodeProcessScriptType?: string;
	nodeProcessType?: string;
	conditionexpression?: string;
}

interface TableDataState {
	currentruleId: string;
	viewstate: string;
	tableData: {
		rows: Array<TableDataRow>;
		total: number;
		loading: boolean;
		param: {
			pageNum: number;
			pageSize: number;
		};
	};
}

const mountTypes = new Map([
	['None', { text: 'None', color: '#b1b3b8' }],
	['RAW', { text: '原始数据', color: '#d3adf7' }],
	['Telemetry', { text: '遥测', color: '#79bbff' }],
	['Attribute', { text: '属性', color: '#b3e19d' }],
	['RPC', { text: '远程控制', color: '#1890ff' }],
	['Connected', { text: '在线', color: '#79bbff' }],
	['Disconnected', { text: '离线', color: '#ffa940' }],
	['TelemetryArray', { text: '遥测数组', color: '#2f54eb' }],
	['Alarm', { text: '告警', color: '#f56c6c' }],
	['DeleteDevice', { text: '删除设备', color: '#fa541c' }],
	['CreateDevice', { text: '创建设备', color: '#08979c' }],
	['Activity', { text: '活动事件', color: '#7cb305' }],
	['Inactivity', { text: '非活跃状态', color: '#ffa940' }],
]);

const addformRef = ref();
const state = reactive<TableDataState>({
	currentruleId: '',
	viewstate: 'list',
	tableData: {
		rows: [],
		total: 0,
		loading: false,
		param: {
			pageNum: 1,
			pageSize: 10,
		},
	},
});

const query = reactive({
	name: '',
});

const currentPageMountTypes = computed(() => {
	const entries = state.tableData.rows.map((item) => item.mountType).filter(Boolean) as string[];
	return [...new Set(entries)];
});

const expandedFlowCount = computed(() =>
	state.tableData.rows.reduce((total, row) => total + (row.flows?.length || 0), 0)
);

const listBadges = computed(() => [
	`第 ${state.tableData.param.pageNum} 页`,
	`每页 ${state.tableData.param.pageSize} 条`,
	query.name ? `筛选 ${query.name}` : '筛选 全部',
]);

const listMetrics = computed(() => [
	{
		label: '规则总数',
		value: state.tableData.total,
		hint: '当前工作区已创建的规则数量。',
		tone: 'primary' as const,
	},
	{
		label: '当前页规则',
		value: state.tableData.rows.length,
		hint: '用于快速浏览本页待处理规则。',
		tone: 'accent' as const,
	},
	{
		label: '挂载类型',
		value: currentPageMountTypes.value.length,
		hint: currentPageMountTypes.value.length
			? `覆盖 ${currentPageMountTypes.value.map((item) => mountTypes.get(item)?.text || item).join(' / ')}`
			: '当前列表尚未加载挂载类型。',
		tone: 'success' as const,
	},
	{
		label: '已展开节点',
		value: expandedFlowCount.value,
		hint: '展开规则后可在子表查看节点明细。',
		tone: 'warning' as const,
	},
]);

const close = () => {
	getData();
};

const submit = () => {
	getData();
};

const ondesignerclose = () => {
	state.viewstate = 'list';
};

const onsimulatorclose = () => {
	state.viewstate = 'list';
};

const onTabelRowDel = (row: TableDataRow) => {
	ElMessageBox.confirm(`此操作将永久删除规则“${row.name}”，是否继续？`, '提示', {
		confirmButtonText: '删除',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(() => ruleApi().deleterule(row.ruleId!))
		.then((res: appmessage<boolean>) => {
			if (res.code === 10000 && res.data) {
				ElMessage.success('删除成功');
				getData();
			} else {
				ElMessage.warning(`删除失败：${res.msg}`);
			}
		})
		.catch(() => undefined);
};

const design = (id: string) => {
	state.viewstate = 'designer';
	state.currentruleId = id;
};

const simulator = (id: string) => {
	state.currentruleId = id;
	state.viewstate = 'simulator';
};

const edit = (id: string) => {
	addformRef.value.openDialog(id);
};

const create = () => {
	addformRef.value.openDialog(NIL_UUID);
};

const onHandleSizeChange = (val: number) => {
	state.tableData.param.pageSize = val;
	state.tableData.param.pageNum = 1;
	getData();
};

const onHandleCurrentChange = (val: number) => {
	state.tableData.param.pageNum = val;
	getData();
};

const handleSearch = () => {
	state.tableData.param.pageNum = 1;
	getData();
};

const getData = async () => {
	state.tableData.loading = true;

	try {
		const res = await ruleApi().ruleList({
			offset: state.tableData.param.pageNum - 1,
			limit: state.tableData.param.pageSize,
			name: query.name,
		});
		state.tableData.rows = res.data.rows;
		state.tableData.total = res.data.total;
	} finally {
		state.tableData.loading = false;
	}
};

const expandchange = async (row: TableDataRow, expanded: Array<TableDataRow>) => {
	if (expanded.length > 0 && row.ruleId) {
		const result = await ruleApi().getFlows(row.ruleId);
		row.flows = result.data;
	}
};

onMounted(() => {
	getData();
});
</script>

<style lang="scss" scoped>
.rule-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.rule-page__card {
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.rule-page__card-eyebrow {
	margin-bottom: 10px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.rule-page__card-head h3 {
	margin: 0;
	color: #123b6d;
	font-size: 22px;
	letter-spacing: -0.04em;
}

.rule-page__card-head p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.75;
}

.rule-page__filters {
	display: flex;
	align-items: center;
	flex-wrap: wrap;
	gap: 12px;
	margin: 18px 0;
}

.rule-page__filters .el-input {
	max-width: 280px;
}

.rule-page__subtable {
	--el-fill-color-light: #f8fbff;
}

.rule-page__pagination {
	margin-top: 18px;
}

:deep(.rule-page__card .el-table) {
	border-radius: 20px;
	overflow: hidden;
}

:deep(.rule-page__card .el-table th.el-table__cell) {
	background: #f8fbff;
}

@media (max-width: 767px) {
	.rule-page__card {
		padding: 18px;
		border-radius: 22px;
	}

	.rule-page__filters {
		flex-direction: column;
		align-items: stretch;
	}

	.rule-page__filters .el-input {
		max-width: none;
	}
}
</style>
