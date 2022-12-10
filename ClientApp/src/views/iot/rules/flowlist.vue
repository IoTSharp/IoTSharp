<template>
  <div class="system-list-container">
    <el-card shadow="hover">
      <div class="system-dept-search mb15">
        <el-input size="default" placeholder="请输入规则名称" style="max-width: 180px" v-model="query.name">
        </el-input>

        <el-button size="default" type="primary" class="ml10" @click="getData()">
          <el-icon>
            <ele-Search />
          </el-icon>
          查询
        </el-button>
        <el-button size="default" type="success" @click="create()" class="ml10">
          <el-icon>
            <ele-FolderAdd />
          </el-icon>
          新增规则
        </el-button>
      </div>
      <el-table :data="state.tableData.rows" style="width: 100%" row-key="ruleId" table-layout="auto" @expand-change="expandchange">


        <el-table-column type="expand">
          <template #default="props">
            <el-table :data="props.row.flows">
              <el-table-column label="节点名称" prop="flowname" />
              <el-table-column label="类型" prop="flowType" />
              <el-table-column label="执行器" prop="nodeProcessClass" />
              <el-table-column label="脚本类型" prop="nodeProcessScriptType" />
           
            </el-table>
          </template>
        </el-table-column>

        <el-table-column prop="name" label="规则名称" show-overflow-tooltip>
        </el-table-column>

        <el-table-column prop="mountType" label="挂载类型" show-overflow-tooltip>
          <template #default="scope">
            <el-tag effect="dark" size="small" type="info" class="mx-1"
              :color="mountTypes.get(scope.row.mountType)?.color" disable-transitions>{{
                  mountTypes.get(scope.row.mountType)?.text
              }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="creatTime" label="创建时间" show-overflow-tooltip></el-table-column>



        <el-table-column label="操作" show-overflow-tooltip >
          <template #default="scope">
            <el-button size="small" text type="primary" icon="Edit" @click="edit(scope.row.ruleId)"> 修改</el-button>
            <el-button size="small" text type="success" icon="Memo" @click="design(scope.row.ruleId)">设计</el-button>
            <el-button size="small" text icon="DocumentChecked" @click="simulator(scope.row.ruleId)">测试</el-button>
            <el-button size="small" text type="danger" icon="Delete" @click="onTabelRowDel(scope.row)">删除</el-button>
       
          </template>
        </el-table-column>
      </el-table>
      <el-pagination @size-change="onHandleSizeChange" @current-change="onHandleCurrentChange" class="mt15"
        :pager-count="5" :page-sizes="[10, 20, 30]" v-model:current-page="state.tableData.param.pageNum" background
        v-model:page-size="state.tableData.param.pageSize" layout="total, sizes, prev, pager, next, jumper"
        :total="state.tableData.total">
      </el-pagination>
    </el-card>
    <addflow ref="addformRef" @close="close" @submit="submit" />
  </div>
</template>

<script lang="ts" setup>
import { ElMessageBox, ElMessage, getPositionDataWithUnit } from "element-plus";
import { ref, reactive, onMounted, defineComponent } from "vue";
import addflow from "./addflow.vue";
import { ruleApi } from "/@/api/flows";
import { Session } from "/@/utils/storage";
import { appmessage } from "/@/api/iapiresult";
import { useRouter } from "vue-router";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
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
  flows?: Array<SubTableDataRow>

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
  ["None", { text: "None", color: "#b1b3b8" }],
  ["RAW", { text: "原始数据", color: "#d3adf7" }],
  ["Telemetry", { text: "遥测", color: "#79bbff" }],
  ["Attribute", { text: "属性", color: "#b3e19d" }],
  ["RPC", { text: "远程控制", color: "#1890ff" }],
  ["Connected", { text: "在线", color: "#79bbff" }],
  ["Disconnected", { text: "离线", color: "#ffa940" }],
  ["TelemetryArray", { text: "遥测数组", color: "#2f54eb" }],
  ["Alarm", { text: "告警", color: "#F56C6C" }],
  ["DeleteDevice", { text: "删除设备", color: "#fa541c" }],
  ["CreateDevice", { text: "创建设备", color: "#08979c" }],
  ["Activity", { text: "活动事件", color: "#7cb305" }],
  ["Inactivity", { text: "非活跃状态", color: "#ffa940" }],
]);
const addformRef = ref();
const userInfos = Session.get("userInfo");
const router = useRouter();
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
  name: "",
});

// 初始化表格数据
const initTableData = () => {
  getData();
};

const close = () => {
  getData();
};
const submit = () => {
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
    .catch(() => { });
};

const design = (id: string) => {
  router.push({
    path: "/iot/rules/flowdesigner",
    query: {
      id: id,
    },
  });
};

const simulator = (id: string) => {
  router.push({
    path: "/iot/rules/flowsimulator",
    query: {
      id: id,
    },
  });
};

const edit = (id: string) => {
  addformRef.value.openDialog(id);
};
const create = () => {
  addformRef.value.openDialog(NIL_UUID);
};

const onHandleSizeChange = (val: number) => {
  query.limit = val;
  getData();
};
// 分页改变
const onHandleCurrentChange = (val: number) => {
  query.offset= val-1;
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



const expandchange = async (row: TableDataRow, expanded: Array<TableDataRow>

) => {

  if (expanded.length > 0 && row.ruleId) {
    var result = await ruleApi().getFlows(row.ruleId)
    row.flows = result.data;
  }

}
// 页面加载时
onMounted(() => {
  initTableData();
});
</script>
