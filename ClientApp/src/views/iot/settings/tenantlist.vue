<template>
	<div class="system-list-container">
		<el-card shadow="hover">
			<div class="system-dept-search mb15">
				<el-input size="default" placeholder="请输入租户名称" style="max-width: 180px" v-model="name"> </el-input>
				<el-button size="default" type="primary" class="ml10" @click="handleSearch">
					<el-icon>
						<ele-Search />
					</el-icon>
					查询
				</el-button>
				<el-button size="default" type="success" @click="create('00000000-0000-0000-0000-000000000000')" class="ml10">
					<el-icon>
						<ele-FolderAdd />
					</el-icon>
					新增租户
				</el-button>
			</div>
			<el-table :data="tableData.rows" style="width: 100%" row-key="id">
				<el-table-column prop="name" label="名称" show-overflow-tooltip> </el-table-column>

				<el-table-column prop="eMail" label="邮件" show-overflow-tooltip></el-table-column>
				<el-table-column prop="phone" label="电话" show-overflow-tooltip></el-table-column>
				<el-table-column prop="country" label="国家" show-overflow-tooltip></el-table-column>
				<el-table-column prop="province" label="省" show-overflow-tooltip></el-table-column>

				<el-table-column prop="city" label="市" show-overflow-tooltip></el-table-column>

				<el-table-column prop="street" label="街道" show-overflow-tooltip></el-table-column>
				<el-table-column prop="address" label="地址" show-overflow-tooltip></el-table-column>
				<el-table-column prop="zipCode" label="邮编" show-overflow-tooltip></el-table-column>
				<el-table-column label="操作" show-overflow-tooltip width="140">
					<template #default="scope">
						<el-button size="small" text type="primary" @click="create(scope.row.id)">修改</el-button>
						<el-button size="small" text type="primary" @click="onTabelRowDel(scope.row)">删除</el-button>
					</template>
				</el-table-column>
			</el-table>
			<el-pagination
				@size-change="onHandleSizeChange"
				@current-change="onHandleCurrentChange"
				class="mt15"
				:pager-count="5"
				:page-sizes="[10, 20, 30]"
				v-model:current-page="tableData.param.pageNum"
				background
				v-model:page-size="tableData.param.pageSize"
				layout="total, sizes, prev, pager, next, jumper"
				:total="tableData.total"
			>
			</el-pagination>
		</el-card>
		<addtenant ref="addformRef" @getData="initTableData" />
	</div>
</template>

<script lang="ts">
import { ElMessageBox, ElMessage, getPositionDataWithUnit } from 'element-plus';
import { ref, toRefs, reactive, onMounted, defineComponent } from 'vue';
import addtenant from './addtenant.vue';
import { deviceApi } from '/@/api/devices';
import { QueryParam, tenantApi } from '/@/api/tenants';
import { Session } from '/@/utils/storage';
import { appmessage } from '/@/api/iapiresult';

// 定义接口来定义对象的类型
interface TableDataRow {
	address?: string;
	city?: string;
	deviceType?: string;
	country?: string;
	eMail?: string;
	id?: string;
	name?: string;
	phone?: string;
	province?: string;
	street?: string;
	zipCode?: string;
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

export default defineComponent({
	name: 'tenantlist',
	components: { addtenant },
	setup() {
		const addformRef = ref();
		const userInfos = Session.get('userInfo');
		const name = ref<string>('');
		const state = reactive<TableDataState>({
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
		// 初始化表格数据
		const initTableData = () => {
			getData();
		};

		const handleSearch = () => {
			state.tableData.param.pageNum = 1;
			state.tableData.param.pageSize = 10;
			getData();
		};

		// 删除当前行
		const onTabelRowDel = (row: TableDataRow) => {
			ElMessageBox.confirm(`此操作将永久删除租户：${row.name}, 是否继续?`, '提示', {
				confirmButtonText: '删除',
				cancelButtonText: '取消',
				type: 'warning',
			})
				.then(() => {
					tenantApi()
						.deletetenant(row.id as string)
						.then((res: appmessage<boolean>) => {
							if (res.code === 10000 && res.data) {
								ElMessage.success('删除成功');
								initTableData();
							} else {
								ElMessage.warning('删除失败:' + res.msg);
							}
						});
					ElMessage.success('删除成功');
				})
				.catch(() => {});
		};

		const create = (id: string) => {
			addformRef.value.openDialog(id);
		};

		const onHandleSizeChange = (val: number) => {
			state.tableData.param.pageSize = val;
			getData();
		};
		// 分页改变
		const onHandleCurrentChange = (val: number) => {
			state.tableData.param.pageNum = val;
			getData();
		};

		const getData = () => {
			let params: QueryParam = {
				offset: state.tableData.param.pageNum - 1,
				limit: state.tableData.param.pageSize,
			};
			if (name) {
				params.name = name.value;
			}
			tenantApi()
				.tenantList(params)
				.then((res) => {
					state.tableData.rows = res.data.rows;
					state.tableData.total = res.data.total;
				});
		};
		// 页面加载时
		onMounted(() => {
			initTableData();
		});
		return { name, addformRef, handleSearch, create, initTableData, onHandleSizeChange, onHandleCurrentChange, onTabelRowDel, ...toRefs(state) };
	},
});
</script>
