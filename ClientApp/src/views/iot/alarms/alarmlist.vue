<template>
  <component :is="wrapper">
    <el-form size="default" label-width="100px" class="mt-10px">
      <el-row :gutter="35">
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="警告创建时间">
            <el-date-picker
                v-model="query.AckDateTime"
                type="daterange"
                start-placeholder="开始时间"
                end-placeholder="结束时间"
            />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="清除时间">
            <el-date-picker
                v-model="query.ClearDateTime"
                type="daterange"
                start-placeholder="开始时间"
                end-placeholder="结束时间"
            />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="持续开始时间">
            <el-date-picker
                v-model="query.StartDateTime"
                type="daterange"
                start-placeholder="开始时间"
                end-placeholder="结束时间"
            />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="35">
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="警告结束时间">
            <el-date-picker
                v-model="query.EndDateTime"
                type="daterange"
                start-placeholder="开始时间"
                end-placeholder="结束时间"
            />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="告警类型">
            <el-input v-model="query.AlarmType" placeholder="请输入告警类型"/>
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="告警状态">
            <el-select v-model="query.alarmStatus" placeholder="请选择告警状态">
              <el-option
                  v-for="item in alarmStatusOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
              />
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="35">
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="告警级别">
            <el-select v-model="query.serverity" placeholder="请选择告警级别">
              <el-option
                  v-for="item in serverityOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
              />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8">
          <el-form-item label="起因来源">
            <el-select v-model="query.originatorType" placeholder="请选择起因来源">
              <el-option
                  v-for="item in originatorTypeOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
              />
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>
      <el-form-item>
        <el-button size="default" type="primary" @click="getData()">
          <el-icon>
            <ele-Search/>
          </el-icon>
          <span>查询</span>
        </el-button>
      </el-form-item>

    </el-form>
    <el-row>

    </el-row>


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
              :color="alarmStatusTAG.get(scope.row.alarmStatus)?.color"
              disable-transitions
          >{{ alarmStatusTAG.get(scope.row.alarmStatus)?.text }}
          </el-tag
          >
        </template>
      </el-table-column>

      <el-table-column prop="serverity" label="严重程度" show-overflow-tooltip>
        <template #default="scope">
          <el-tag
              effect="dark"
              size="small"
              :color="serverityBadge.get(scope.row.serverity)?.color"
              disable-transitions
          >{{ serverityBadge.get(scope.row.serverity)?.text }}
          </el-tag
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
          >{{ originatorTypeTAG.get(scope.row.originatorType)?.text }}
          </el-tag
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
          >确认告警
          </el-button
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
          >清除告警
          </el-button
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
  </component>
</template>

<script lang="ts" setup>

import { getAlarmList, clear, acquire } from "/@/api/alarm";
import { appmessage } from "/@/api/iapiresult";
import { alarmStatusOptions, serverityOptions, originatorTypeOptions, serverityBadge, alarmStatusTAG, originatorTypeTAG } from "/@/views/iot/alarms/alarmSearchOptions";
import type {TableDataRow, TableDataState} from "/@/views/iot/alarms/model";
// 定义接口来定义对象的类型

const props = defineProps({
  OriginatorId: {
    type: String,
    default: "00000000-0000-0000-0000-000000000000",
  },
  wrapper: {
    type: String,
    default: "el-card",
  }
})
const tableData = reactive<TableDataState>({
  rows: [],
  total: 0,
  loading: false,
  param: {
    pageNum: 1,
    pageSize: 10,
  },
})

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
  tableData.param.pageSize = val;

  getData();
};
// 分页改变
const onHandleCurrentChange = (val: number) => {
  tableData.param.pageNum = val;
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
  getAlarmList({
    offset: tableData.param.pageNum - 1,
    limit: tableData.param.pageSize,
    alarmStatus: query.alarmStatus,
    OriginatorId: props.OriginatorId,
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
    tableData.rows = res.data.rows;
    tableData.total = res.data.total;
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
