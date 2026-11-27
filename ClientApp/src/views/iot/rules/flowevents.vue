<template>
  <div class="system-list-container" :class="{ 'console-list-page': standaloneMode }">
    <component :is="wrapper" :class="{ 'console-list-panel flow-events-card': standaloneMode }">
      <div class="system-dept-search" :class="{ 'flow-events-search': standaloneMode }">
        <el-form :size="standaloneMode ? 'small' : 'default'" :label-width="standaloneMode ? 'auto' : '100px'">
          <el-row :gutter="standaloneMode ? 0 : 35">
            <el-form-item label="创建对象">
              <el-select v-model="query.Creator" filterable remote reserve-keyword placeholder="请输入创建对象名称"
                         :remote-method="getCreators" :loading="state.creatorloading">
                <el-option v-for="item in state.creators" :key="item.value" :label="item.label" :value="item.value" />
              </el-select>
            </el-form-item>
            <el-form-item label="规则">
              <el-select v-model="query.RuleId" placeholder="请选择规则">
                <el-option v-for="item in state.rules" :key="item.value" :label="item.label" :value="item.value"
                           :disabled="item.disabled" />
              </el-select>
            </el-form-item>
            <el-form-item label="事件名称">
              <el-input v-model="query.Name" placeholder="请输入事件名称" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="getData()">
                <el-icon>
                  <ele-Search />
                </el-icon>
                查询
              </el-button>
              <el-button @click="expandAll()">
                {{ state.expandAll ? '收缩全部' : '展开全部'}}
              </el-button>
            </el-form-item>
          </el-row>
        </el-form>

      </div>

      <div :class="{ 'console-list-table': standaloneMode }" :role="standaloneMode ? 'region' : undefined"
           :aria-label="standaloneMode ? '规则事件列表' : undefined" :tabindex="standaloneMode ? 0 : undefined">
        <el-table :data="state.tableData.rows" style="width: 100%"
                  :size="standaloneMode ? 'small' : 'default'"
                  row-key="eventId"
                  table-layout="auto"
                  :expand-row-keys="state.expandRowKeys"
                  v-loading="loading">
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
      </div>
      <el-pagination @size-change="onHandleSizeChange" @current-change="onHandleCurrentChange"
                     :class="standaloneMode ? 'console-list-pagination' : 'mt15'" :size="standaloneMode ? 'small' : 'default'"
                     :pager-count="5" :page-sizes="[10, 20, 30,50,100]" v-model:current-page="state.tableData.param.pageNum" background
                     v-model:page-size="state.tableData.param.pageSize" layout="total, sizes, prev, pager, next, jumper"
                     :total="state.tableData.total">
      </el-pagination>
    </component>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref, toRefs, reactive, onMounted, defineComponent } from "vue";
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

import { Session } from "/@/utils/storage";
import { ruleApi } from "/@/api/flows";
import { deviceApi } from "/@/api/devices";
// 定义接口来定义对象的类型

const props = defineProps({
  creator: {
    type: String,
    default: "",
  },

  creatorname: {
    type: String,
    default: "",
  },
  wrapper: {
    type: String,
    default: "el-card",
  }
})
const standaloneMode = computed(() => !props.creator && props.wrapper === "el-card");
const loading = ref(false)

function expandAll() {
  state.expandAll = !state.expandAll
  if (state.expandAll) {
    state.expandRowKeys = state.tableData.rows.map((item: any) => item.eventId)
  } else {
    state.expandRowKeys = []
  }
}
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
  expandStatus: boolean;
}

interface TableDataState {
  expandAll: boolean // 是否展开全部状态
  expandRowKeys: Array<any>; // 已展开行的 id 合集
  creatorloading: boolean;
  creators: Array<any>;
  rules: Array<any>
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


const state = reactive<TableDataState>({
  creatorloading: false,
  expandAll: false,
  expandRowKeys: [],
  creators: [],
  rules: [],
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
  Creator: props.creator ?? '',
  Name: "",
  RuleId: "",
});



const getRules = () => {

  state.creatorloading=true;
  ruleApi()
      .ruleList({
        limit: 100, offset: 0
      })
      .then((res) => {
        state.creatorloading=false;
        state.rules = [...res.data.rows.map(c => { return { value: c.ruleId, label: c.name } })]
      })
}


const getCreators = async (creator: string) => {
  if (creator) {
    const res = await deviceApi().devcieList({
      offset: 0,
      limit: 20,
      onlyActive: false,
      customerId: userInfos.customer.id,
      name: creator
    });
    state.creators = [...res.data?.rows?.map(x => {
      return {
        label: x.name, value: x.id
      }
    })]
  }



}
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

const getData = async () => {

  const params: {
    offset: number
    limit: number
    Name: string
    RuleId?: string
    Creator?: string
  } = {
    offset: state.tableData.param.pageNum - 1,
    limit: state.tableData.param.pageSize,
    Name: query.Name,
  }

  // 未选择筛选项时省略可空 Guid 字段，避免把空字符串交给 ASP.NET Core 绑定器。
  if (query.RuleId) {
    params.RuleId = query.RuleId
  }

  if (props.creator) {
    params.Creator = props.creator
  } else if (query.Creator) {
    params.Creator = query.Creator
  }
  try {
    loading.value = true
    const res = await ruleApi().floweventslist(params)
    state.tableData.rows = res.data.rows.map((item: any) => {
      item.expandStatus = false
      return item
    });
    state.tableData.total = res.data.total;
    loading.value = false
  } catch (e) {
    loading.value = false
  }

};
// 初始化表格数据
const initTableData = () => {
  getData();
};
onMounted(() => {
  initTableData();
  getRules();
});


</script>

<style lang="scss" scoped>
.flow-events-card :deep(.el-card__body) {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  gap: 8px;
  min-height: 0;
  padding: 10px 16px;
}

.flow-events-search {
  flex-shrink: 0;
}

.flow-events-search :deep(.el-row) {
  gap: 8px 14px;
}

.flow-events-search :deep(.el-form-item) {
  margin-bottom: 0;
}

.flow-events-search :deep(.el-input),
.flow-events-search :deep(.el-select) {
  width: 180px;
  max-width: 100%;
}
</style>
