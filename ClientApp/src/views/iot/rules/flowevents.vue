<template>
  <div class="system-list-container">
    <component :is="wrapper">
      <div class="system-dept-search ">
        <el-form size="default" label-width="100px">
          <el-row :gutter="35">
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
              <el-form-item label="事件名称">
                <el-input v-model="query.CreatorName" placeholder="请输入事件名称"/>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
              <el-form-item label="规则">
                <el-input v-model="query.RuleId" placeholder="请选择规则"/>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
              <el-form-item label="创建对象">
                <el-input v-model="query.Name" placeholder="请输入创建对象"/>
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item>

            <el-button size="default" type="primary" @click="getData()">
              <el-icon>
                <ele-Search/>
              </el-icon>
              查询
            </el-button>
          </el-form-item>
        </el-form>

      </div>


      <el-table :data="state.tableData.rows" style="width: 100%" row-key="eventId" table-layout="auto">
        <el-table-column type="expand">
          <template #default="props">
            <el-card class="box-card" style="margin: 3px;">
              <template #header>
                <div class="card-header">
                  <span>描述</span>
                </div>
              </template>
              <p m="t-0 b-2">{{ props.row.eventDesc }}</p>
            </el-card>
            <el-card class="box-card" style="margin: 3px;">
              <template #header>
                <div class="card-header">
                  <span>输入值</span>
                </div>
              </template>
              <p m="t-0 b-2"> {{ props.row.mataData }}</p>
            </el-card>
          </template>
        </el-table-column>
        <el-table-column prop="eventName" label="事件名称" show-overflow-tooltip></el-table-column>
        <el-table-column prop="type" label="类型" show-overflow-tooltip></el-table-column>
        <el-table-column prop="name" label="触发规则" show-overflow-tooltip></el-table-column>
        <el-table-column prop="createrDateTime" label="创建时间" show-overflow-tooltip></el-table-column>
        <el-table-column label="操作" show-overflow-tooltip>
          <template #default="scope">
            <el-button size="small" text type="primary" v-if="
            scope.row.alarmStatus === 'Active_UnAck' ||
            scope.row.alarmStatus === 'Cleared_UnAck'
          " @click="replay(scope.row)">回放
            </el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination @size-change="onHandleSizeChange" @current-change="onHandleCurrentChange" class="mt15"
                     :pager-count="5" :page-sizes="[10, 20, 30]" v-model:current-page="state.tableData.param.pageNum"
                     background
                     v-model:page-size="state.tableData.param.pageSize" layout="total, sizes, prev, pager, next, jumper"
                     :total="state.tableData.total">
      </el-pagination>

    </component>
  </div>
</template>

<script lang="ts" setup>
import {ref, toRefs, reactive, onMounted, defineComponent} from "vue";
import {
  ElMessageBox,
  ElMessage,
  ElButton,
  ElCard,
  ElIcon,
  ElInput,
  ElPagination,
  ElTable,
  ElTableColumn,
} from "element-plus";

import {Session} from "/@/utils/storage";
import {ruleApi} from "/@/api/flows";
// 定义接口来定义对象的类型

const props = defineProps({
  creator: {
    type: String,
    default: "",
  },
  wrapper: {
    type: String,
    default: "el-card",
  }
})

interface TableDataRow {
  bizid?: string;
  createrDateTime?: string;
  creator?: string;
  creatorName?: string;
  eventDesc?: string;
  eventId?: string;
  eventName?: string;
  eventStaus?: string;
  mataData?: string;
  name?: string;
  ruleId?: string;
  type?: string;
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
  CreatorName: "",
  Name: "",
  RuleId: "",
});

const replay = (val: TableDataRow) => {
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

  const params:{
    offset: number
    limit: number
    Name: string
    RuleId: string
    CreatorName: string
    Creator?: string
  } = {
    offset: state.tableData.param.pageNum - 1,
    limit: state.tableData.param.pageSize,
    Name: query.Name,
    RuleId: query.RuleId,
    CreatorName: query.CreatorName,
  }
  if (props.creator) {
    params.Creator = props.creator
  }
  ruleApi().floweventslist(params).then((res) => {
    state.tableData.rows = res.data.rows;
    state.tableData.total = res.data.total;
  });
};
// 初始化表格数据
const initTableData = () => {
  getData();
};
onMounted(() => {
  initTableData();
});


</script>
