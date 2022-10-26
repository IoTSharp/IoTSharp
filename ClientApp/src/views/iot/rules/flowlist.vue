<template>
  <div class="system-list-container">
    <el-card shadow="hover">
      <div class="system-dept-search mb15">
        <el-input
          size="default"
          placeholder="请输入规则名称"
          style="max-width: 180px"
          v-model="name"
        >
        </el-input>

        <el-button size="default" type="primary" class="ml10" @click="getData()">
          <el-icon>
            <ele-Search />
          </el-icon>
          查询
        </el-button>
        <el-button
          size="default"
          type="success"
          @click="create('00000000-0000-0000-0000-000000000000')"
          class="ml10"
        >
          <el-icon>
            <ele-FolderAdd />
          </el-icon>
          新增规则
        </el-button>
      </div>
      <el-table :data="tableData.rows" style="width: 100%" row-key="id">
        <el-table-column prop="name" label="规则名称" show-overflow-tooltip>
        </el-table-column>

        <el-table-column
          prop="deviceType"
          label="规则类型"
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
            <el-button size="small" text type="primary" @click="create(scope.row.ruleId)"
              >修改</el-button
            >

            <el-button size="small" text type="primary" @click="design(scope.row.ruleId)"
              >设计</el-button
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
    <addflow ref="addformRef" />
    <!-- <flowdesigner ref="flowdesignerRef" /> -->
  </div>
</template>

<script lang="ts">
import { ElMessageBox, ElMessage, getPositionDataWithUnit } from "element-plus";
import { ref, toRefs, reactive, onMounted, defineComponent } from "vue";
import addflow from "./addflow.vue";
import flowdesigner from "./flowdesigner.vue";
import { ruleApi } from "/@/api/flows";
import { Session } from "/@/utils/storage";
import { treeEmits } from "element-plus/es/components/tree-v2/src/virtual-tree";
import { appmessage } from "/@/api/iapiresult";
import { useRouter } from "vue-router";

// 定义接口来定义对象的类型
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
  name: "flowlist",
  components: { addflow, flowdesigner },
  setup() {
    const addformRef = ref();   
    const userInfos = Session.get("userInfo");
    const router=useRouter();
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

    const query = reactive({
      offset: state.tableData.param.pageNum - 1,
      limit: state.tableData.param.pageSize,
      onlyActive: false,
      customerId: userInfos.customerId.id,
      name: "",
    });

    // 初始化表格数据
    const initTableData = () => {
      getData();
    };

    // 删除当前行
    const onTabelRowDel = (row: TableDataRow) => {
      ElMessageBox.confirm(`此操作将永久删除规则：${row.name}, 是否继续?`, "提示", {
        confirmButtonText: "删除",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          return ruleApi().deleterule(row.ruleId!);
        })
        .then((res: appmessage<boolean>) => {
          if (res.code === 10000 && res.data) {
            ElMessage.success("删除成功");
            getData();
          } else {
            ElMessage.warning("删除失败:" + res.msg);
          }
        })
        .catch(() => {});
    };

    
   
    const design = (id: string) => {
      router.push({

        path:'/iot/rules/flowdesigner',
        query:{
          id:id
        }
      })
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
      ruleApi()
        .ruleList(query)
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
      design,
      onHandleSizeChange,
      onHandleCurrentChange,
      onTabelRowDel,
      ...toRefs(state),
      ...toRefs(query),
      getData,
    };
  },
});
</script>
