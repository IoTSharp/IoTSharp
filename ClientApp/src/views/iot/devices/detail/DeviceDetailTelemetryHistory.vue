<template>
  <div>
    <div class="search">
      <el-form ref="formRef" :model="queryForm" label-width="120px" size="small">
        <el-form-item prop="keys" label="遥测属性" class="z-search-keys">
          <div class="z-checkbox-group">
            <el-checkbox-group v-model="queryForm.keys">
              <el-checkbox v-for="key in state.telemetryKeys" :label="key" :key="key"/>
            </el-checkbox-group>
          </div>
<!--          <el-button  type="primary" @click="backToRealtime">-->
<!--            <el-icon><ArrowLeft /></el-icon>返回实时遥测</el-button>-->
        </el-form-item>
        <el-form-item prop="datetimeRange" label="时间区间">
          <div style="width:100px">
            <el-date-picker
                v-model="queryForm.datetimeRange"
                type="datetimerange"
                :shortcuts="shortcuts"
                range-separator="To"
                start-placeholder="Start date"
                end-placeholder="End date"
            />
          </div>
        </el-form-item>
        <el-form-item prop="every" label="时间间隔">
          <el-time-picker v-model="queryForm.every" placeholder="时间间隔" value-format="HH:mm:ss"/>
        </el-form-item>
        <el-form-item prop="aggregate" label="取值方式">
          <el-radio-group v-model="queryForm.aggregate">
            <el-radio-button label="None">所有值</el-radio-button>
            <el-radio-button label="Mean">平均值</el-radio-button>
            <el-radio-button label="Median">中值</el-radio-button>
            <el-radio-button label="Last">末值</el-radio-button>
            <el-radio-button label="First">首值</el-radio-button>
            <el-radio-button label="Max">最大值</el-radio-button>
            <el-radio-button label="Min">最小值</el-radio-button>
            <el-radio-button label="Sum">合计</el-radio-button>
          </el-radio-group>
        </el-form-item>
        <el-form-item class="z-search-button-area">
          <div>
            <el-button type="primary" @click="search">查询</el-button>
            <el-button @click="resetForm(formRef)">重置</el-button>
          </div>
          <el-radio-group v-model="dataDisplayStatus" class="ml-12px">
            <el-radio-button label="chart">图表</el-radio-button>
            <el-radio-button label="dataTable">数据</el-radio-button>
          </el-radio-group>

        </el-form-item>
      </el-form>
    </div>
    <!--    表格数据 -->
    <div v-show="dataDisplayStatus === 'dataTable'" class="z-table">
      <el-table :data="tableData" style="width: 100%" size="small" v-loading="loading">
        <el-table-column prop="keyName" label="名称"></el-table-column>
        <el-table-column prop="value" label="值"></el-table-column>
        <el-table-column prop="dataType" label="类型"></el-table-column>
        <el-table-column prop="dateTime" label="时间" :formatter="formatColumnDataTime"></el-table-column>
      </el-table>
    </div>
    <div v-show="dataDisplayStatus === 'chart'">
      <div style="height: 330px" ref="messageChartRef" v-loading="loading"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {dateUtil, ElDateTimePickerShortcuts, formatToDateTime} from "/@/utils/dateUtil";
import {deviceApi} from "/@/api/devices";
import { getCurrentInstance, ref } from "vue";
import { EChartsOption } from "echarts";
import * as echarts from "echarts";
import _ from 'lodash-es';
import { telemetryHistoryChartOptions } from "/@/views/iot/devices/detail/telemetryHistoryChartOptions";
const {proxy} = <any>getCurrentInstance();
const formatColumnDataTime = (row, column, cellValue, index) => {
  return formatToDateTime(cellValue)
}
const formRef = ref()
const loading = ref(false)
// const dataDisplayStatus = ref('dataTable')
const dataDisplayStatus = ref('chart')
const messageChartRef = ref();
let historyChart:any = null

interface IQueryForm {
  pi: number;
  ps: number;
  deviceId: string;
  keys: string | any;
  end: string | Date;
  every: string;
  begin: string | Date;
  sorter: string;
  aggregate: string;
  status: any;
  datetimeRange: Date | number | string | Array<Date>
}


const shortcuts = ElDateTimePickerShortcuts

const props = defineProps({
  deviceId: {
    type: String,
    default: ''
  },
})

const queryForm: IQueryForm = reactive({
  pi: 0,
  deviceId: props.deviceId,
  ps: 10,
  keys: [],
  every: '01:00:00',
  aggregate: 'Mean',
  begin: dateUtil().subtract(1, 'day').toISOString(),
  end: dateUtil().toISOString(),
  sorter: '',
  status: null,
  datetimeRange: []
})
const tableData = ref([])


const search = async () => {
  if (queryForm.keys.length === 0) {
    ElMessage.warning('请选择遥测属性')
    return
  }
  await getData()
}
const resetForm = (formEl) => {
  if (!formEl) return
  formEl.resetFields()
}
// const backToRealtime = ()=>{
//   proxy.mittBus.emit('updateTelemetryPageSate', 'realtime')
//
// }
const getData = async () => {
  const params = {...queryForm}
  if (params.datetimeRange[0]) params.begin = params.datetimeRange[0]
  if (params.datetimeRange[1]) params.end = params.datetimeRange[1]
  console.log(`%cgetData@DeviceDetailTelemetryHistory:148`, 'color:black;font-size:16px;background:yellow;font-weight: bold;', params.keys)
  params.keys = params.keys.join(',')
  params.every = '0.' + params.every + ':000';
  loading.value = true
  try {
    const res = await deviceApi().getDeviceTelemetryData(props.deviceId, params)
    tableData.value = res.data
    updateChart(res.data)
  } catch (e) { /* empty */ }
  loading.value = false
}
const updateChart = (rawData) => {
  let series = []
  let xAxisData = []
  series = queryForm.keys.map((key)=>{
    return {
      name: key,
      type: 'line',
      smooth: true,
      symbol: 'none',
      data: [],
      seriesLayoutBy: 'row'
    }
  })
  Object.entries(_.groupBy(rawData, 'dateTime')).forEach(([dateTime, values])=>{
    xAxisData.push(dateTime)
    for (const data of values) {
      const seriesItem = series.find((x)=>x.name === data.keyName)
      seriesItem.data.push(data.value)

    }
  })
  telemetryHistoryChartOptions.series = series
  telemetryHistoryChartOptions.xAxis.data = xAxisData
  historyChart.setOption(telemetryHistoryChartOptions, {
    replaceMerge: ["series", "yAxis", "xAxis"],
  });
}
const initChart = (target: any, option: EChartsOption) => {
  historyChart = echarts.init(target.value);
  historyChart.setOption(option);
};
const state = reactive({
  telemetryKeys: []
})
const getTelemetryKeys = async () => {
  const res = await deviceApi().getDeviceLatestTelemetry(props.deviceId);
  state.telemetryKeys = res.data.filter((x) => typeof x.value === 'number').map((c) => c.keyName);
}

onMounted(()=>{
  getTelemetryKeys();
  initChart(messageChartRef, telemetryHistoryChartOptions as EChartsOption);
})
</script>

<style lang="scss" scoped>
.z-table {
  width: 100%;
  height: calc(100vh - 380px);
  overflow: auto;
}

.z-search-button-area {
  padding-right: 20px;

  :deep(.el-form-item__content) {
    display: flex;
    justify-content: space-between;
  }

}
.z-search-keys {
  :deep(.el-form-item__content) {
    display: flex;
    justify-content: space-between;
    flex-wrap: nowrap;
    align-items: start;
    .z-checkbox-group {

    }
  }
}
</style>
