<template>
	<div class="product-page">
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
				<el-button type="primary" :loading="state.tableData.loading" @click="refreshList">刷新列表</el-button>
				<el-button type="success" @click="createProduct">新增产品</el-button>
			</template>

			<template #aside>
				<div class="product-page__scope">
					<span>产品工作区</span>
					<strong>{{ state.tableData.total }}</strong>
					<small>建模、映射与设备创建入口都已收拢到这里</small>
				</div>
			</template>

			<div class="product-page__filters">
				<el-input v-model="query.name" placeholder="请输入产品名称" clearable @keyup.enter="handleSearch" />
				<el-button type="primary" @click="handleSearch">
					<el-icon><ele-Search /></el-icon>
					查询
				</el-button>
				<el-button @click="resetFilters">重置</el-button>
			</div>

			<el-table :data="state.tableData.rows" row-key="id" v-loading="state.tableData.loading" class="product-page__table">
				<el-table-column type="expand">
					<template #default="props">
						<el-table :data="props.row.devices" class="product-page__subtable">
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
						<div class="product-page__name-cell">
							<strong>{{ scope.row.name }}</strong>
							<el-tag v-if="scope.row.defaultDeviceType === 'Gateway'">网关模型</el-tag>
							<el-tag v-else-if="scope.row.defaultDeviceType === 'Device'" type="warning">设备模板</el-tag>
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
						<el-button size="small" text type="primary" @click.prevent="editProduct(scope.row)">
							<el-icon><Edit /></el-icon>
							修改
						</el-button>

						<el-button size="small" text type="primary" @click.prevent="deleteProductRow(scope.row)">
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
				class="product-page__pagination"
				:pager-count="5"
				:page-sizes="[10, 20, 30]"
				v-model:current-page="state.tableData.param.pageNum"
				background
				v-model:page-size="state.tableData.param.pageSize"
				layout="total, sizes, prev, pager, next, jumper"
				:total="state.tableData.total"
			/>
		</ConsoleCrudWorkspace>

		<productform ref="productformRef" @close="close" @submit="submit" />
		<ProductDataDictionaryForm ref="productDataDictionaryFormRef" @submit="submit" @close="close" />
		<productpropform ref="productpropformRef" @close="close" @submit="submit" />
		<ProductDataMappingDesigner ref="productDataMappingDesignerRef" @close="close" @submit="submit" />
		<deviceform ref="deviceformRef" @close="close" @submit="submit"></deviceform>
	</div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { useRouter } from 'vue-router';
import { NIL as NIL_UUID } from 'uuid';
import ConsoleCrudWorkspace from '/@/components/console/ConsoleCrudWorkspace.vue';
import productform from './productform.vue';
import deviceform from './deviceform.vue';
import productpropform from './productpropform.vue';
import ProductDataDictionaryForm from './productdatadictionaryform.vue';
import ProductDataMappingDesigner from './productdatamappingdesigner.vue';
import { deleteProduct, getProductList } from '/@/api/product';

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

const productformRef = ref();
const deviceformRef = ref();
const productDataDictionaryFormRef = ref();
const productpropformRef = ref();
const productDataMappingDesignerRef = ref();
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
			label: '设备模板',
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

const refreshList = () => {
	getData();
};

const getData = async () => {
	if (state.tableData.loading) return;
	state.tableData.loading = true;

	try {
		const res = await getProductList({
			offset: state.tableData.param.pageNum - 1,
			limit: state.tableData.param.pageSize,
			name: query.name,
		});
		const pageData = res?.data ?? {};
		state.tableData.rows = Array.isArray(pageData.rows) ? pageData.rows : [];
		state.tableData.total = Number(pageData.total ?? 0);
		lastRefresh.value = new Date().toLocaleTimeString('zh-CN', { hour12: false });
	} catch (error) {
		state.tableData.rows = [];
		state.tableData.total = 0;
		ElMessage.error('刷新产品列表失败，请稍后重试');
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

const createProduct = () => {
	productformRef.value.openDialog(NIL_UUID);
};

const editProduct = (row: TableDataRow) => {
	productformRef.value.openDialog(row.id);
};

const editprop = (row: TableDataRow) => {
	productpropformRef.value.openDialog(row.id);
};

const editdict = (row: TableDataRow) => {
	productDataDictionaryFormRef.value.openDialog(row.id);
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
	productDataMappingDesignerRef.value.openDialog(row.id, row.devices ?? []);
};

const deleteProductRow = async (row: TableDataRow) => {
	ElMessageBox.confirm('确定删除该产品？', '警告', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			const result = await deleteProduct(row.id ?? '');
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
.product-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.product-page__scope {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 170px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.product-page__scope span {
	color: #64748b;
	font-size: 12px;
}

.product-page__scope strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 30px;
	line-height: 1;
}

.product-page__scope small {
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
	text-align: right;
}

.product-page__filters {
	display: flex;
	align-items: center;
	flex-wrap: wrap;
	gap: 12px;
	margin-bottom: 18px;
}

.product-page__filters .el-input {
	max-width: 320px;
}

.product-page__name-cell {
	display: flex;
	align-items: center;
	gap: 10px;
}

.product-page__name-cell strong {
	color: #123b6d;
	font-weight: 700;
}

.product-page__subtable {
	--el-fill-color-light: #f8fbff;
}

.product-page__pagination {
	margin-top: 18px;
}

:deep(.product-page__table) {
	border-radius: 20px;
	overflow: hidden;
}

:deep(.product-page__table th.el-table__cell) {
	background: #f8fbff;
}

@media (max-width: 767px) {
	.product-page__scope {
		width: 100%;
		align-items: flex-start;
	}

	.product-page__scope small {
		text-align: left;
	}

	.product-page__filters {
		flex-direction: column;
		align-items: stretch;
	}

	.product-page__filters .el-input {
		max-width: none;
	}

	.product-page__name-cell {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
