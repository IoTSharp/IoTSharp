<template>
  <div>
    <div class="mixin-components-container">
      <el-row v-for="(page, index) of pages" :key="index" :gutter="8" style="margin-top:10px;">
        <el-col v-for="(item, innerindex) of page" :key="item.id" :offset="innerindex > 0 ? 2 : 1" :span="6">
          <el-card class="box-card">
            <div slot="header" class="clearfix">
              <span>{{ item.name }}</span>
            </div>
            <div v-if="item.vChartType === 've-line'">
              <ve-line :data="item.chartData" />
            </div>
            <div v-if="item.vChartType === 've-gauge'">
              <ve-gauge :data="item.chartData" :settings="item.settings" />
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>
<script>

// import PanThumb from '@/components/PanThumb'
// import MdInput from '@/components/MDinput'
import VeLine from 'v-charts/lib/line.common'
import VeGauge from 'v-charts/lib/gauge.common'
import { deepClone } from '@/utils'
// https://codesandbox.io/s/z69myovqzx
// https://segmentfault.com/q/1010000012529833

const DEV_COMPONENT_STORAGE_KEY = 'devcomponents'
const cardDefaultInfo = {
  id: '123',
  name: 'Testname',
  title: 'chart',
  vChartType: 'type',
  chartData: null,
  settings: null
}
export default {
  name: 'ComponentLib',
  components: {
    VeLine,
    VeGauge
  },
  data() {
    const validate = (rule, value, callback) => {
      if (value.length !== 6) {
        callback(new Error('请输入六个字符'))
      } else {
        callback()
      }
    }
    this.chartGaugeSettings = {
      dimension: 'type',
      metrics: 'value'
    }
    return {
      demo: {
        title: ''
      },
      demoRules: {
        title: [{ required: true, trigger: 'change', validator: validate }]
      },
      cardInfo: Object.assign({}, cardDefaultInfo),
      allChartList: [],
      chartLineData: {
        columns: ['日期', 'Meter1', 'Meter2', 'Meter3'],
        rows: [
          { 日期: '1/1', Meter1: 1393, Meter2: 1093, Meter3: 0.32 },
          { 日期: '1/2', Meter1: 3530, Meter2: 3230, Meter3: 0.26 },
          { 日期: '1/3', Meter1: 2923, Meter2: 2623, Meter3: 0.76 },
          { 日期: '1/4', Meter1: 1723, Meter2: 1423, Meter3: 0.49 },
          { 日期: '1/5', Meter1: 3792, Meter2: 3492, Meter3: 0.323 },
          { 日期: '1/6', Meter1: 4593, Meter2: 4293, Meter3: 0.78 }
        ]
      },
      chartDefaultGaugeData: {
        columns: ['type', 'a', 'b', 'value'],
        rows: [
          { type: '温度', value: 20, a: 1, b: 2 }
        ]
      },
      chartGaugeData: {
        columns: ['type', 'a', 'b', 'value'],
        rows: [
          { type: '温度', value: 80, a: 1, b: 2 }
        ]
      }
    }
  },
  computed: {
    pages() {
      const pages = []
      this.allChartList.forEach((item, index) => {
        const page = Math.floor(index / 4) // 4代表4条为一行，随意更改
        if (!pages[page]) {
          pages[page] = []
        }
        pages[page].push(item)
      })
      return pages
    }
  },
  created() {
    const tempcardInfo = deepClone(this.cardInfo)
    tempcardInfo.chartData = this.chartLineData
    tempcardInfo.name = 'threeMeterLineChart'
    tempcardInfo.vChartType = 've-line'
    this.allChartList.push(tempcardInfo)
    const tempcardInfo2 = deepClone(this.cardInfo)
    tempcardInfo2.id = '456'
    tempcardInfo2.name = 'oneValueGaugeChart'
    tempcardInfo2.vChartType = 've-gauge'
    tempcardInfo2.chartData = this.chartGaugeData
    tempcardInfo2.settings = this.chartGaugeSettings
    this.allChartList.push(tempcardInfo2)
    window.localStorage.setItem(DEV_COMPONENT_STORAGE_KEY, JSON.stringify(this.allChartList))
    console.log('all chart list:')
    console.log(this.allChartList)
  },
  methods: { // 这里用于定义方法
    change() {
      console.log('change gauge data:')
      this.chartGaugeData = this.chartDefaultGaugeData
    }
  }
}
</script>

<style scoped>
.mixin-components-container {
  background-color: #f0f2f5;
  padding: 10px;
  min-height: calc(100vh - 84px);
}
.component-item{
  min-height: 50px;
}
</style>
