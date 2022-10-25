<template>
  <div class="system-list-container">
    <el-card shadow="hover">
      <div class="system-dept-search mb15">
        <el-input size="default" placeholder="请输入设备名称" style="max-width: 180px">
        </el-input>
        <el-button size="default" type="primary" class="ml10">
          <el-icon>
            <ele-Search />
          </el-icon>
          查询
        </el-button>
        <el-button
          size="default"
          type="success"
          @click="create('0000000-0000-0000-0000-000000000000')"
          class="ml10"
        >
          <el-icon>
            <ele-FolderAdd />
          </el-icon>
          新增设备
        </el-button>
      </div>
      <el-table :data="tableData.rows" style="width: 100%" row-key="id">
        <el-table-column prop="name" label="设备名称" show-overflow-tooltip>
        </el-table-column>
        <!-- <el-table-column label="排序" show-overflow-tooltip width="80">
					<template #default="scope">
						{{ scope.$index }}
					</template>
				</el-table-column> -->
        <!-- <el-table-column prop="status" label="设备状态" show-overflow-tooltip>
					<template #default="scope">
						<el-tag type="success" v-if="scope.row.status">启用</el-tag>
						<el-tag type="info" v-else>禁用</el-tag>
					</template>
				</el-table-column> -->
        <el-table-column
          prop="deviceType"
          label="设备类型"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column
          prop="active"
          label="在线状态"
          show-overflow-tooltip
        ></el-table-column>
        <el-table-column
          prop="lastActivityDateTime"
          label="最后活动时间"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column
          prop="active"
          label="认证方式"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column label="操作" show-overflow-tooltip width="140">
          <template #default="scope">
            <el-button size="small" text type="primary" @click="create(scope.row.id)"
              >修改</el-button
            >
            <el-button size="small" text type="primary" @click="onTabelRowDel(scope.row)"
              >删除</el-button
            >
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
    <adddevice ref="addformRef" />
  </div>
</template>

<script lang="ts">
import { ElMessageBox, ElMessage, getPositionDataWithUnit } from "element-plus";
import { ref, toRefs, reactive, onMounted, defineComponent } from "vue";
import adddevice from "./adddevice.vue";
import { deviceApi } from "/@/api/devices";
import { Session } from "/@/utils/storage";
import { treeEmits } from "element-plus/es/components/tree-v2/src/virtual-tree";
import { appmessage } from "/@/api/iapiresult";

// 定义接口来定义对象的类型
interface TableDataRow {
  active?: boolean;
  customerId?: string;
  deviceType?: string;
  id?: string;
  identityId?: string;
  identityType?: string;
  identityValue?: string;
  lastActivityDateTime?: string;
  name?: string;
  owner?: string;
  tenantId?: string;
  tenantName?: string;
  timeout?: number;
  children?: TableDataRow[];
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
  name: "devicelist",
  components: { adddevice },
  setup() {
    const addformRef = ref();
    const userInfos = Session.get("userInfo");
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

    // 删除当前行
    const onTabelRowDel = (row: TableDataRow) => {
      ElMessageBox.confirm(`此操作将永久删除设备：${row.name}, 是否继续?`, "提示", {
        confirmButtonText: "删除",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          return deviceApi().deletedevcie(row.id!);
        })
        .then((res:appmessage<boolean>) => {
     
          if (res.code === 10000&&res.data) {
            ElMessage.success("删除成功");
            getData();
          } else {
            ElMessage.warning("删除失败:"+res.msg);
          }
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
      deviceApi()
        .devcieList({
          offset: state.tableData.param.pageNum - 1,
          limit: state.tableData.param.pageSize,
          onlyActive: false,
          customerId: userInfos.customerId.id,
          name: "",
        })
        .then((res) => {
          console.log(res);
          state.tableData.rows = res.data.rows;
          state.tableData.total = res.data.total;
        });
    };
    // 页面加载时
    onMounted(() => {
      initTableData();
    });
    return {
      addformRef,
      create,
      onHandleSizeChange,
      onHandleCurrentChange,
      onTabelRowDel,
      ...toRefs(state),
    };
  },
});
</script>
