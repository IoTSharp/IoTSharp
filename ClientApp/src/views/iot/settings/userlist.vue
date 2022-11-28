<template>
	<div class="system-list-container">
		<el-card shadow="hover">
			<div class="system-dept-search mb15">
				<el-input size="default" v-model="name" placeholder="请输入用户名称" style="max-width: 180px"> </el-input>
				<el-button size="default" type="primary" class="ml10" @click="handleSearch">
					<el-icon>
						<ele-Search />
					</el-icon>
					查询
				</el-button>
			</div>
			<el-table :data="tableData.rows" style="width: 100%" row-key="id">
				<el-table-column prop="id" label="Id" show-overflow-tooltip></el-table-column>
				<el-table-column prop="userName" label="邮件" show-overflow-tooltip></el-table-column>
				<el-table-column prop="email" label="邮件" show-overflow-tooltip></el-table-column>
				<el-table-column prop="phoneNumber" label="电话" show-overflow-tooltip></el-table-column>
				<el-table-column prop="accessFailedCount" label="登录失败次数" show-overflow-tooltip></el-table-column>
				<el-table-column prop="lockoutEnabled" label="锁定" show-overflow-tooltip></el-table-column>
				<el-table-column prop="lockoutEnd" label="锁定时间" show-overflow-tooltip></el-table-column>

				<el-table-column label="操作" show-overflow-tooltip width="140">
					<template #default="scope">
						<el-button size="small" text type="primary" @click="create(scope.row.id)">修改</el-button>
						<el-button size="small" text type="primary" @click="onTabelRowDel(scope.row)">锁定/禁用</el-button>
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
	</div>
</template>

<script lang="ts">
import { ElMessageBox, ElMessage } from 'element-plus';
import { ref, toRefs, reactive, onMounted, defineComponent } from 'vue';
import { accountApi } from '/@/api/user';
import { useUserInfo } from '/@/stores/userInfo';
import { storeToRefs } from 'pinia';
import { CustomerQueryParam } from '/@/api/user';
import { appmessage } from '/@/api/iapiresult';

// 定义接口来定义对象的类型
interface TableDataRow {
    email?: string;
	id?: string;
	userName?: string;
    phoneNumber?: string;
    accessFailedCount?: string;
    lockoutEnabled?: string;
    lockoutEnd?: string;
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
	name: 'userlist',
	setup: function () {
		const stores = useUserInfo();
		const { userInfos } = storeToRefs(stores);
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
		const handleSearch = () => {
			state.tableData.param.pageNum = 1;
			state.tableData.param.pageSize = 10;
			getData();
		};
		// 初始化表格数据
		const initTableData = () => {
			getData();
		};

		// 删除当前行
		const onTabelRowDel = (row: TableDataRow) => {
			ElMessageBox.confirm(`此操作将永久删除用户：${row.userName}, 是否继续?`, '提示', {
				confirmButtonText: '删除',
				cancelButtonText: '取消',
				type: 'warning',
			})
				.then(() => {
					accountApi()
						.deleteAccount(row.id as string)
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
            let params: CustomerQueryParam = {
                customerId: userInfos.value.customer.id,
                offset: state.tableData.param.pageNum - 1,
                limit: state.tableData.param.pageSize,
            };
			if (name) {
				params.name = name.value;
			}
			accountApi()
				.accountList(params)
				.then((res) => {
					state.tableData.rows = res.data.rows;
					state.tableData.total = res.data.total;
				});
		};
		// 页面加载时
		onMounted(() => {
			initTableData();
		});
		return { name, initTableData, handleSearch, onHandleSizeChange, onHandleCurrentChange, onTabelRowDel, ...toRefs(state) };
	},
});
</script>
