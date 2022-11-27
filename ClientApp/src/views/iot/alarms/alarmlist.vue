<template>
  <div class="system-list-container">
    <el-card shadow="hover">
      <div class="system-dept-search mb15">
        <el-form size="default" label-width="100px" class="mt35 mb35">
          <el-row :gutter="35">
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="警告创建时间">
                <el-date-picker
                  v-model="AckDateTime"
                  type="daterange"
                  start-placeholder="开始时间"
                  end-placeholder="结束时间"
                />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="清除时间">
                <el-date-picker
                  v-model="ClearDateTime"
                  type="daterange"
                  start-placeholder="开始时间"
                  end-placeholder="结束时间"
                />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="警告持续开始时间">
                <el-date-picker
                  v-model="StartDateTime"
                  type="daterange"
                  start-placeholder="开始时间"
                  end-placeholder="结束时间"
                />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="35">
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="警告结束时间">
                <el-date-picker
                  v-model="EndDateTime"
                  type="daterange"
                  start-placeholder="开始时间"
                  end-placeholder="结束时间"
                />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="告警类型">
                <el-input v-model="AlarmType" placeholder="请输入告警类型" />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="告警状态">
                <el-select v-model="alarmStatus" placeholder="请选择告警状态">
                  <el-option
                    v-for="item in alarmStatusoptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="35">
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="告警级别">
                <el-select v-model="serverity" placeholder="请选择告警级别">
                  <el-option
                    v-for="item in serverityoptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="起因来源">
                <el-select v-model="originatorType" placeholder="请选择起因来源">
                  <el-option
                    v-for="item in originatorTypeoptions"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>

        <el-button size="default" type="primary" class="ml10" @click="getData()">
          <el-icon>
            <ele-Search />
          </el-icon>
          查询
        </el-button>
      </div>
      <el-table :data="tableData.rows" style="width: 100%" row-key="id">
        <el-table-column
          prop="alarmType"
          label="告警类型"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column
          prop="ackDateTime"
          label="创建时间"
          show-overflow-tooltip
        ></el-table-column>
        <el-table-column
          prop="startDateTime"
          label="警告持续的开始时间"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column
          prop="endDateTime"
          label="警告持续的结束时间"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column
          prop="清除时间"
          label="警告持续的结束时间"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column prop="alarmStatus" label="告警状态" show-overflow-tooltip>
          <template #default="scope">
            <el-tag
              effect="dark"
              size="small"
              :color="alarmstatusTAG.get(scope.row.alarmStatus)?.color"
              disable-transitions
              >{{ alarmstatusTAG.get(scope.row.alarmStatus)?.text }}</el-tag
            >
          </template>
        </el-table-column>

        <el-table-column prop="serverity" label="严重程度" show-overflow-tooltip>
          <template #default="scope">
            <el-tag
              effect="dark"
              size="small"
              :color="serveritybadge.get(scope.row.serverity)?.color"
              disable-transitions
              >{{ serveritybadge.get(scope.row.serverity)?.text }}</el-tag
            >
          </template>
        </el-table-column>

        <el-table-column prop="originatorType" label="设备类型" show-overflow-tooltip>
          <template #default="scope">
            <el-tag
              effect="dark"
              size="small"
              :color="originatorTypeTAG.get(scope.row.originatorType)?.color"
              disable-transitions
              >{{ originatorTypeTAG.get(scope.row.originatorType)?.text }}</el-tag
            >
          </template>
        </el-table-column>

        <el-table-column label="操作" show-overflow-tooltip width="200">
          <template #default="scope">
            <el-button
              size="small"
              text
              type="primary"
              v-if="
                scope.row.alarmStatus === 'Active_UnAck' ||
                scope.row.alarmStatus === 'Cleared_UnAck'
              "
              @click="acquireAlarm(scope.row)"
              >确认告警</el-button
            >
            <el-button
              size="small"
              text
              type="primary"
              v-if="
                scope.row.alarmStatus === 'Active_Ack' ||
                scope.row.alarmStatus === 'Active_UnAck'
              "
              @click="clearAlarm(scope.row)"
              >清除告警</el-button
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

    <!-- <flowdesigner ref="flowdesignerRef" /> -->
  </div>
</template>

<script lang="ts">
import { ref, toRefs, reactive, onMounted, defineComponent } from "vue";
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
import { create } from "domain";
import { Session } from "/@/utils/storage";
import { getAlarmList, clear, acquire } from "/@/api/alarm";
import { appmessage } from "/@/api/iapiresult";
// 定义接口来定义对象的类型

interface TableDataRow {
  id?: string;
  ackDateTime?: string;
  alarmDetail?: string;
  alarmStatus?: string;
  alarmType?: string;
  clearDateTime?: string;
  endDateTime?: string;
  originator?: string;
  originatorId?: string;
  originatorType?: string;
  propagate?: string;
  serverity?: string;
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
  name: "addDevice",
  components: {},
  setup() {
    const alarmStatusoptions = [
      {
        value: "-1",
        label: "全部",
      },
      {
        value: "0",
        label: "激活未应答",
      },
      {
        value: "1",
        label: "激活已应答",
      },
      {
        value: "2",
        label: "清除未应答",
      },
      {
        value: "3",
        label: "清除已应答",
      },
    ];

    const serverityoptions = [
      {
        value: "-1",
        label: "全部",
      },
      {
        value: "0",
        label: "不确定",
      },
      {
        value: "1",
        label: "警告",
      },
      {
        value: "2",
        label: "次要",
      },
      {
        value: "3",
        label: "重要",
      },
      {
        value: "4",
        label: "错误",
      },
    ];

    const originatorTypeoptions = [
      {
        value: "-1",
        label: "全部",
      },
      {
        value: "0",
        label: "未知",
      },
      {
        value: "1",
        label: "设备",
      },
      {
        value: "2",
        label: "网关",
      },
      {
        value: "3",
        label: "资产",
      },
    ];

    const serveritybadge = new Map([
      ["Indeterminate", { text: "不确定", color: "#8c8c8c" }],
      ["Warning", { text: "警告", color: "#faad14" }],
      ["Minor", { text: "次要", color: "#bae637" }],
      ["Major", { text: "主要", color: "#1890ff" }],
      ["Critical", { text: "错误", color: "#f5222d" }],
    ]);

    const alarmstatusTAG = new Map([
      ["Active_UnAck", { text: "激活未应答", color: "#ffa39e" }],
      ["Active_Ack", { text: "激活已应答", color: "#f759ab" }],
      ["Cleared_UnAck", { text: "清除未应答", color: "#87e8de" }],
      ["Cleared_Act", { text: "清除已应答", color: "#7cb305" }],
    ]);

    const originatorTypeTAG = new Map([
      ["Unknow", { text: "未知", color: "#ffa39e" }],
      ["Device", { text: "设备", color: "#f759ab" }],
      ["Gateway", { text: "网关", color: "#87e8de" }],
      ["Asset", { text: "资产", color: "#d3f261" }],
    ]);

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
      AckDateTime: "",
      ClearDateTime: "",
      StartDateTime: "",
      EndDateTime: "",
      AlarmType: "",
      alarmStatus: "-1",
      serverity: "-1",
      originatorType: "-1",
      Originator: "",
    });
    const onHandleSizeChange = (val: number) => {
      state.tableData.param.pageSize = val;

      getData();
    };
    // 分页改变
    const onHandleCurrentChange = (val: number) => {
      state.tableData.param.pageNum = val;
      getData();
    };

    const clearAlarm = (row: TableDataRow) => {
      clear(row.id!).then((x:appmessage<boolean>) => {
        if (x && x.data) {
          ElMessage.success("清除成功");
          getData()
        } else {
          ElMessage.warning("清除失败:"+x.msg);
        }
      });
    };
    const acquireAlarm = (row: TableDataRow) => {
      acquire(row.id!).then((x:appmessage<boolean>) => {
       if (x && x.data) {
          ElMessage.success("确认成功");
          getData()
        } else {
          ElMessage.warning("确认失败"+x.msg);
        }
      });
    };
    const getData = () => {
      console.log(query);
      getAlarmList({
        offset: state.tableData.param.pageNum - 1,
        limit: state.tableData.param.pageSize,
        alarmStatus: query.alarmStatus,
        OriginatorId: "00000000-0000-0000-0000-000000000000",
        ClearDateTime: query.ClearDateTime,
        AckDateTime: query.AckDateTime,
        StartDateTime: query.StartDateTime,
        originatorType: query.originatorType,
        AlarmType: query.AlarmType,
        Name: "",
        EndDateTime: query.EndDateTime,
        OriginatorName: "",
        serverity: "-1",
      }).then((res) => {
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
    return {
      onHandleSizeChange,
      onHandleCurrentChange,
      getData,
      serveritybadge,
      alarmstatusTAG,
      originatorTypeTAG,
      alarmStatusoptions,
      originatorTypeoptions,
      serverityoptions,
      acquireAlarm,
      clearAlarm,
      ...toRefs(state),
      ...toRefs(query),
    };
  },
});
</script>
