<template>
	<div class="produce-page">
		<ConsoleCrudWorkspace
			eyebrow="Product Workspace"
			title="产品与模型管理"
			description="把产品搜索、创建、属性字典、映射设计和设备创建入口统一到新的工作台布局里，减少在多种表格样式之间切换。"
			card-eyebrow="Product Table"
			card-title="产品列表"
			card-description="支持按产品名称筛选，直接进入属性、字典、数据映射与设备创建等常用操作。"
			:badges="badges"
			:metrics="metrics"
		>
			<template #actions>
				<el-button type="primary" @click="getData">刷新列表</el-button>
				<el-button type="success" @click="creatprod">新增产品</el-button>
			</template>

			<template #aside>
				<div class="produce-page__scope">
					<span>产品工作区</span>
					<strong>{{ state.tableData.total }}</strong>
					<small>建模、映射与设备创建入口都已收拢到这里</small>
				</div>
			</template>

			<div class="produce-page__filters">
				<el-input v-model="query.name" placeholder="请输入产品名称" clearable @keyup.enter="handleSearch" />
				<el-button type="primary" @click="handleSearch">
					<el-icon><ele-Search /></el-icon>
					查询
				</el-button>
				<el-button @click="resetFilters">重置</el-button>
			</div>

			<el-table :data="state.tableData.rows" row-key="id" v-loading="state.tableData.loading" class="produce-page__table">
				<el-table-column type="expand">
					<template #default="props">
						<el-table :data="props.row.devices" class="produce-page__subtable">
							<el-table-column label="设备名称" prop="name" />
							<el-table-column label="设备类型" prop="deviceType">
								<template #default="scope">
									<el-tag v-if="scope.row.deviceType === 'Gateway'">网关</el-tag>
									<el-tag v-else-if="scope.row.deviceType === 'Device'" type="warning">设备</el-tag>
									<span v-else>{{ scope.row.deviceType || '--' }}</span>
								</template>
							</el-table-column>
							<el-table-column label="超时" prop="timeout" />
						</el-table>
					</template>
				</el-table-column>

				<el-table-column prop="name" label="产品名称" min-width="220" show-overflow-tooltip>
					<template #default="scope">
						<div class="produce-page__name-cell">
							<strong>{{ scope.row.name }}</strong>
							<el-tag v-if="scope.row.defaultDeviceType === 'Gateway'">网关模型</el-tag>
							<el-tag v-else-if="scope.row.defaultDeviceType === 'Device'" type="warning">设备模型</el-tag>
						</div>
					</template>
				</el-table-column>

				<el-table-column prop="defaultDeviceType" label="默认设备类型" width="140">
					<template #default="scope">
						<el-tag v-if="scope.row.defaultDeviceType === 'Gateway'">网关</el-tag>
						<el-tag v-else-if="scope.row.defaultDeviceType === 'Device'" type="warning">设备</el-tag>
						<span v-else>{{ scope.row.defaultDeviceType || '--' }}</span>
					</template>
				</el-table-column>
				<el-table-column prop="defaultIdentityType" label="认证方式" show-overflow-tooltip />
				<el-table-column prop="defaultTimeout" label="超时" width="100" show-overflow-tooltip />
				<el-table-column prop="description" label="备注" show-overflow-tooltip />

				<el-table-column label="操作" show-overflow-tooltip width="320">
					<template #default="scope">
						<el-button size="small" text type="primary" @click.prevent="editprod(scope.row)">
							<el-icon><Edit /></el-icon>
							修改
						</el-button>

						<el-button size="small" text type="primary" @click.prevent="deleteprod(scope.row)">
							<el-icon><Delete /></el-icon>
							删除
						</el-button>

						<el-dropdown
							size="small"
							@command="
								(command) => {
									dropdownCommand(scope.row, command);
								}
							"
						>
							<el-button type="primary" size="small" text>
								更多
								<el-icon class="el-icon--right"><arrow-down /></el-icon>
							</el-button>
							<template #dropdown>
								<el-dropdown-menu>
									<el-dropdown-item command="prop">
										<el-icon><Operation /></el-icon>
										属性
									</el-dropdown-item>
									<el-dropdown-item command="dict">
										<el-icon><DocumentCopy /></el-icon>
										字典
									</el-dropdown-item>
									<el-dropdown-item command="mapping">
										<el-icon><Share /></el-icon>
										数据映射
									</el-dropdown-item>
									<el-dropdown-item command="createdev">
										<el-icon><Plus /></el-icon>
										创建设备
									</el-dropdown-item>
									<el-dropdown-item command="managedev">管理设备</el-dropdown-item>
								</el-dropdown-menu>
							</template>
						</el-dropdown>
					</template>
				</el-table-column>
			</el-table>

			<el-pagination
				@size-change="onHandleSizeChange"
				@current-change="onHandleCurrentChange"
				class="produce-page__pagination"
				:pager-count="5"
				:page-sizes="[10, 20, 30]"
				v-model:current-page="state.tableData.param.pageNum"
				background
				v-model:page-size="state.tableData.param.pageSize"
				layout="total, sizes, prev, pager, next, jumper"
				:total="state.tableData.total"
			/>
		</ConsoleCrudWorkspace>

		<produceform ref="produceformRef" @close="close" @submit="submit" />
		<producedatadictionaryform ref="producedatadictionaryformRef" @submit="submit" @close="close" />
		<producepropform ref="producepropformRef" @close="close" @submit="submit" />
		<producedatamappingdesigner ref="producedatamappingdesignerRef" @close="close" @submit="submit" />
		<deviceform ref="deviceformRef" @close="close" @submit="submit"></deviceform>
	</div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { useRouter } from 'vue-router';
import { NIL as NIL_UUID } from 'uuid';
import ConsoleCrudWorkspace from '/@/components/console/ConsoleCrudWorkspace.vue';
import produceform from './produceform.vue';
import deviceform from './deviceform.vue';
import producepropform from './producepropform.vue';
import producedatadictionaryform from './producedatadictionaryform.vue';
import producedatamappingdesigner from './producedatamappingdesigner.vue';
import { deleteProduce, getProduceList } from '/@/api/produce';

interface TableDataRow {
	id?: string;
	name?: string;
	defaultDeviceType?: string;
	defaultIdentityType?: string;
	defaultTimeout?: string;
	description?: string;
	devices?: Array<SubTableDataRow>;
}

interface SubTableDataRow {
	id?: string;
	name?: string;
	deviceType?: string;
	timeout?: string;
}

interface TableDataState {
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

const produceformRef = ref();
const deviceformRef = ref();
const producedatadictionaryformRef = ref();
const producepropformRef = ref();
const producedatamappingdesignerRef = ref();
const router = useRouter();

const state = reactive<TableDataState>({
	tableData: {
		rows: [],
		total: 0,
		loading: false,
		param: {
			pageNum: 1,
			pageSize: 20,
		},
	},
});

const query = reactive({
	name: '',
});

const lastRefresh = ref('');

const badges = computed(() => [
	query.name ? `筛选 ${query.name}` : '筛选 全部',
	`当前页 ${state.tableData.rows.length} 个`,
	lastRefresh.value ? `最近同步 ${lastRefresh.value}` : '等待首次同步',
]);

const metrics = computed(() => {
	const gatewayCount = state.tableData.rows.filter((item) => item.defaultDeviceType === 'Gateway').length;
	const deviceCount = state.tableData.rows.filter((item) => item.defaultDeviceType === 'Device').length;
	const describedCount = state.tableData.rows.filter((item) => item.description).length;

	return [
		{
			label: '产品总数',
			value: state.tableData.total,
			hint: '当前工作区内已建模的产品数量。',
			tone: 'primary' as const,
		},
		{
			label: '网关模型',
			value: gatewayCount,
			hint: '当前页默认设备类型为网关的产品数量。',
			tone: 'accent' as const,
		},
		{
			label: '设备模型',
			value: deviceCount,
			hint: '当前页默认设备类型为设备的产品数量。',
			tone: 'success' as const,
		},
		{
			label: '描述完整',
			value: describedCount,
			hint: '当前页已填写备注说明的产品数量。',
			tone: 'warning' as const,
		},
	];
});

const onHandleSizeChange = (val: number) => {
	state.tableData.param.pageSize = val;
	state.tableData.param.pageNum = 1;
	getData();
};

const onHandleCurrentChange = (val: number) => {
	state.tableData.param.pageNum = val;
	getData();
};

const getData = async () => {
	state.tableData.loading = true;

	try {
		const res = await getProduceList({
			offset: state.tableData.param.pageNum - 1,
			limit: state.tableData.param.pageSize,
			name: query.name,
		});
		state.tableData.rows = res.data.rows;
		state.tableData.total = res.data.total;
		lastRefresh.value = new Date().toLocaleTimeString('zh-CN', { hour12: false });
	} finally {
		state.tableData.loading = false;
	}
};

const handleSearch = () => {
	state.tableData.param.pageNum = 1;
	getData();
};

const resetFilters = () => {
	query.name = '';
	state.tableData.param.pageNum = 1;
	getData();
};

onMounted(() => {
	getData();
});

const close = () => {
	getData();
};

const submit = () => {
	getData();
};

const dropdownCommand = (row: any, command: string) => {
	switch (command) {
		case 'dict':
			editdict(row);
			break;
		case 'createdev':
			creatdevice(row);
			break;
		case 'managedev':
			navtodevice(row);
			break;
		case 'prop':
			editprop(row);
			break;
		case 'mapping':
			editMapping(row);
			break;
	}
};

const creatprod = () => {
	produceformRef.value.openDialog(NIL_UUID);
};

const editprod = (row: TableDataRow) => {
	produceformRef.value.openDialog(row.id);
};

const editprop = (row: TableDataRow) => {
	producepropformRef.value.openDialog(row.id);
};

const editdict = (row: TableDataRow) => {
	producedatadictionaryformRef.value.openDialog(row.id);
};

const creatdevice = (row: TableDataRow) => {
	deviceformRef.value.openDialog(row.id);
};

const navtodevice = () => {
	router.push({
		path: '/iot/devices/devicelist',
	});
};

const editMapping = (row: any) => {
	producedatamappingdesignerRef.value.openDialog(row.id, row.devices ?? []);
};

const deleteprod = async (row: TableDataRow) => {
	ElMessageBox.confirm('确定删除该产品？', '警告', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			const result = await deleteProduce(row.id ?? '');
			if (result.code === 10000) {
				ElMessage.success('删除成功');
				getData();
			} else {
				ElMessage.warning(`删除失败：${result.msg}`);
			}
		})
		.catch(() => undefined);
};
</script>

<style lang="scss" scoped>
.produce-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.produce-page__scope {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 170px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.produce-page__scope span {
	color: #64748b;
	font-size: 12px;
}

.produce-page__scope strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 30px;
	line-height: 1;
}

.produce-page__scope small {
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
	text-align: right;
}

.produce-page__filters {
	display: flex;
	align-items: center;
	flex-wrap: wrap;
	gap: 12px;
	margin-bottom: 18px;
}

.produce-page__filters .el-input {
	max-width: 320px;
}

.produce-page__name-cell {
	display: flex;
	align-items: center;
	gap: 10px;
}

.produce-page__name-cell strong {
	color: #123b6d;
	font-weight: 700;
}

.produce-page__subtable {
	--el-fill-color-light: #f8fbff;
}

.produce-page__pagination {
	margin-top: 18px;
}

:deep(.produce-page__table) {
	border-radius: 20px;
	overflow: hidden;
}

:deep(.produce-page__table th.el-table__cell) {
	background: #f8fbff;
}

@media (max-width: 767px) {
	.produce-page__scope {
		width: 100%;
		align-items: flex-start;
	}

	.produce-page__scope small {
		text-align: left;
	}

	.produce-page__filters {
		flex-direction: column;
		align-items: stretch;
	}

	.produce-page__filters .el-input {
		max-width: none;
	}

	.produce-page__name-cell {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
