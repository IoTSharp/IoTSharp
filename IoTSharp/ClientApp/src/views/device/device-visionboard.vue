<template>
  <div>
    <div class="mixin-components-container">
      <el-row v-for="(page, index) in pages" :key="index" :gutter="8" style="margin-top:10px;">
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
import { bus } from './utils.js'
// https://codesandbox.io/s/z69myovqzx
// https://segmentfault.com/q/1010000012529833

const cardDefaultInfo = {
  id: '123',
  name: 'Testname',
  title: 'chart',
  vChartType: 'type',
  chartData: null,
  settings: null
}

/*
const devChartBindingData = {
  id: '',
  deviceId: '',
  devName: '',
  devBindedAttrId: '',
  devBindedAttr: '',
  devBingedChart: ''
}
*/

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
      metrics: 'value',
      dataName: {
        '温度': '温度'
      }
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
      pages: [],
      refreshTimer: null,
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
        title: '温度',
        columns: ['type', 'a', 'b', 'value'],
        rows: [
          { type: '温度', value: 20, a: 1, b: 2 }
        ]
      },
      chartGaugeData: {
        title: '温度',
        columns: ['type', 'a', 'b', 'value'],
        rows: [
          { type: '温度', value: 80, a: 1, b: 2 }
        ]
      },
      devChartBindingDataReceived: null
    }
  },
  computed() {
    // this.refreshPages()
    /*
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
    */
  },
  created() {
    var chartDataInfo = JSON.parse(window.localStorage.getItem('datachartbinginfo'))
    console.log('from local storage：')
    console.log(chartDataInfo)
    const tempcardInfo = deepClone(this.cardInfo)
    tempcardInfo.vChartType = chartDataInfo.devBingedChart
    tempcardInfo.name = chartDataInfo.devName
    if (tempcardInfo.vChartType === 've-gauge') {
      tempcardInfo.chartData = this.chartGaugeData
      tempcardInfo.settings = this.chartGaugeSettings
    }
    if (tempcardInfo.vChartType === 've-line') {
      tempcardInfo.chartData = this.chartLineData
    }
    this.allChartList.push(tempcardInfo)
    // start
    this.refreshTimer = setInterval(() => {
      for (let i = 0; i < this.allChartList.length; i++) {
        // todo, only show how to
        if (this.allChartList[i].vChartType === 've-gauge') {
          var rdmValue = Math.floor(Math.random() * 10) + 25
          this.allChartList[i].chartData.rows = [{ type: '温度', value: rdmValue, a: 1, b: 2 }]
        }
      }
    }, 2000)
    console.log('created called all chart list:')
    console.log(this.allChartList)
    this.refreshPages()
    // this.$forceUpdate()
    bus.$on('devbindingdata', (data) => {
      console.log('Received data:')
      // this.refreshPages()
      // this.$forceUpdate()
    })
  },
  mounted() {
  },
  beforeDestroy() {
    clearInterval(this.refreshTimer)
  },
  methods: { // 这里用于定义方法
    change() {
      console.log('change gauge data:')
      this.chartGaugeData = this.chartDefaultGaugeData
    },
    refreshPages() {
      console.log('calculate paegs!')
      this.allChartList.forEach((item, index) => {
        const page = Math.floor(index / 4) // 4代表4条为一行，随意更改
        if (!this.pages[page]) {
          this.pages[page] = []
        }
        this.pages[page].push(item)
      })
      console.log(this.pages)
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
