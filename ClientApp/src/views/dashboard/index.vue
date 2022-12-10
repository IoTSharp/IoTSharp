<template>
  <div class="home-container">
    <!-- 看板 -->
    <el-row :gutter="15" class="home-card-one mb15" style="row-gap: 15px">
      <home-card-item :item="item" :index="index" v-for="(item, index) in kanbanData" :key="index"></home-card-item>
    </el-row>
    <!--   图表 + 健康 -->
    <el-row :gutter="15" class="mb15">
      <el-col :span="12">
        <card title="设备在线率" :icon="checkNetwork" style="height: 400px">
          <div style="height: 100%" ref="onlineChartRef"></div>
        </card>
      </el-col>
      <el-col :span="12">
        <card title="设备告警率" :icon="warning" style="height: 400px">
          <div style="height: 100%" ref="warningChartRef"></div>
        </card>
      </el-col>

    </el-row>
    <!--  消息 -->
    <el-row :gutter="15" class="mb15" >
      <el-col :span="16">
        <card title="消息总线" :icon="iconChart" style="height: 400px">
          <div style="height: 310px" ref="messageChartRef"></div>
        </card>
      </el-col>
      <el-col :span="8">
        <card title="健康检查" :icon="monitor">
          <div class="pt-10px">
            <div v-for="(item, index) in healthChecks" class="flex items-center mb-20px text-lg" :key="index">
              <el-icon class="text-2xl mr-8px">
                <CircleCheckFilled class="text-green-500" v-if="item.status === 'Healthy'"/>
                <CircleCloseFilled class="text-red-500" v-else/>
              </el-icon>
              {{ item.name }}
            </div>
          </div>
        </card>
      </el-col>
    </el-row>

  </div>

</template>
<script lang="ts" setup>
import * as echarts from "echarts";
import {EChartsOption} from 'echarts'
import {storeToRefs} from "pinia";
import checkNetwork from "~icons/mdi/check-network"
import warning from "~icons/ic/round-warning"
import message from "~icons/ic/baseline-message"
import monitor from "~icons/ic/round-monitor-heart"
import iconChart from '~icons/mdi/chart-bar-stacked'
import Card from '/@/components/card/index.vue'

let global: any = {
  homeChartOne: null,
  homeChartTwo: null,
  homeCharThree: null,
  dispose: [null, '', undefined],
};
import HomeCardItem from "/@/views/dashboard/HomeCardItem.vue";
import {homeCardItemsConfig} from "/@/views/dashboard/homeCardItems";
import {nextTick, onActivated, onMounted, ref, watch} from "vue";
import {useThemeConfig} from '/@/stores/themeConfig';
import {getHealthChecks, getKanban, getMessageInfo} from "/@/api/dashboard";

const storesThemeConfig = useThemeConfig();
const {themeConfig} = storeToRefs(storesThemeConfig);

const onlineChartRef = ref();
const warningChartRef = ref();
const messageChartRef = ref();
const myCharts = reactive([]) // 页面所有图表保存在这里
const charts = reactive({
  theme: '',
  bgColor: '',
  color: '#303133',
})
const healthChecks = reactive([])
const colorList = [themeConfig.value.primary, '#D8D8D8'];
const onlineChartOption = {
  backgroundColor: charts.bgColor,
  tooltip: {trigger: 'item', formatter: '{b} <br/> {c}'},
  legend: {
    type: 'scroll',
    orient: 'vertical',
    right: '0%',
    left: '65%',
    top: 'center',
    itemWidth: 14,
    itemHeight: 14,
    data: ['在线设备', '离线设备'],
    textStyle: {
      rich: {
        name: {
          fontSize: 14,
          fontWeight: 400,
          width: 200,
          height: 35,
          padding: [0, 0, 0, 60],
          color: charts.color,
        },
        rate: {
          fontSize: 15,
          fontWeight: 500,
          height: 35,
          width: 40,
          padding: [0, 0, 0, 30],
          color: charts.color,
        },
      },
    },
  },
  series: [
    {
      type: 'pie',
      radius: ['60', themeConfig.value.isIsDark ? '50' : '102'],
      center: ['32%', '50%'],
      itemStyle: {
        color: function (params: any) {
          return colorList[params.dataIndex];
        },
      },
      label: {show: false},
      labelLine: {show: false},
      data: [{
        name: '在线设备', value: 99
      }, {name: '离线设备', value: 10}],
    },
  ],
};
const warningChartOption = {
  backgroundColor: charts.bgColor,
  tooltip: {trigger: 'item', formatter: '{b} <br/> {c}'},
  legend: {
    type: 'scroll',
    orient: 'vertical',
    right: '0%',
    left: '65%',
    top: 'center',
    itemWidth: 14,
    itemHeight: 14,
    data: ['正常', '普通告警', '重要告警', '紧急告警'],
    textStyle: {
      rich: {
        name: {
          fontSize: 14,
          fontWeight: 400,
          width: 200,
          height: 35,
          padding: [0, 0, 0, 60],
          color: charts.color,
        },
        rate: {
          fontSize: 15,
          fontWeight: 500,
          height: 35,
          width: 40,
          padding: [0, 0, 0, 30],
          color: charts.color,
        },
      },
    },
  },
  series: [
    {
      type: 'pie',
      radius: ['60', themeConfig.value.isIsDark ? '50' : '102'],
      center: ['32%', '50%'],
      // itemStyle: {
      //   color: function (params: any) {
      //     return colorList[params.dataIndex];
      //   },
      // },
      color: [themeConfig.value.primary, '#F6BD16', '#FF8728', '#E23711'],
      label: {show: false},
      labelLine: {show: false},
      data: [
        {name: '正常', value: '99'},
        {name: '普通告警', value: '10'},
        {name: '重要告警', value: '10'},
        {name: '紧急告警', value: '10'}
      ],
    },
  ],
};
const messageChartOption = {
  tooltip: {
    trigger: 'axis'
  },
  legend: {
    data: ['publishFailed', 'publishSuccessed', 'subscribeFailed', 'subscribeSuccessed']
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '3%',
    containLabel: true
  },
  xAxis: {
    type: 'category',
    boundaryGap: false,
    data: []
  },
  yAxis: {
    type: 'value',
    boundaryGap: [0, '100%']
  },
  dataZoom: [
    {
      type: 'inside',
      start: 0,
      end: 10
    },
    {
      start: 0,
      end: 10,
      height: 10,//这里可以设置dataZoom的尺寸
      bottom: 0,
    }
  ],
  series: [
    {
      name: 'publishFailed',
      type: 'line',
      smooth: true,
      seriesLayoutBy: 'row',
      stack: 'Total',
      data: []
    },
    {
      name: 'publishSuccessed',
      type: 'line',
      smooth: true,
      seriesLayoutBy: 'row',
      stack: 'Total',
      data: []
    },
    {
      name: 'subscribeFailed',
      type: 'line',
      smooth: true,
      seriesLayoutBy: 'row',
      stack: 'Total',
      data: []
    },
    {
      name: 'subscribeSuccessed',
      type: 'line',
      smooth: true,
      seriesLayoutBy: 'row',
      stack: 'Total',
      data: []
    },
  ]
};
const kanbanData = reactive(homeCardItemsConfig)
/**
 *
 * @param target 在哪里显示, 是一个 Dom 元素，通过 <div ref="xxx"> 赋值到变量中
 * @param option:EChartsOption 图表参数
 * @param name:string initEchartsResizeFun 需要页面上所有图表的列表， 所以用 name 区分保存
 */
const initPieChart = (target: any, option: EChartsOption, name: string,) => {
  // if (!global.dispose.some((b: any) => b === global.homeChartTwo)) global.homeChartTwo.dispose();
  global[name] = <any>echarts.init(target.value, charts.theme);
  (<any>global[name]).setOption(option);
  (<any>myCharts).push(global[name]);
};


// 批量设置 echarts resize
const initEchartsResizeFun = () => {
  nextTick(() => {
    for (let i = 0; i < myCharts.length; i++) {
      setTimeout(() => {
        (<any>myCharts[i]).resize();
      }, i * 1000);
    }
  });
};
// 批量设置 echarts resize
const initEchartsResize = () => {
  window.addEventListener('resize', initEchartsResizeFun);
};
// 页面加载时
onMounted(async () => {
  await getData()
  initEchartsResize();

});
// 由于页面缓存原因，keep-alive
onActivated(() => {
  initEchartsResizeFun();
});

async function getData(){
  // 卡片数据
    const res = await getKanban()
    const {
        eventCount,
        onlineDeviceCount,
        telemetryDataCount,
        deviceCount,
        alarmsCount,
        userCount,
        produceCount,
        rulesCount
    } = res.data
    kanbanData[3].zValue = eventCount
    kanbanData[1].zValue = onlineDeviceCount
    kanbanData[0].zValue = deviceCount
    kanbanData[2].zValue = telemetryDataCount
    kanbanData[4].zValue = alarmsCount
    kanbanData[5].zValue = userCount
    kanbanData[6].zValue = produceCount
    kanbanData[7].zValue = rulesCount

  // * 配置在线设备图表数据
  onlineChartOption.series[0].data[0].value = onlineDeviceCount
  onlineChartOption.series[0].data[1].value = deviceCount - onlineDeviceCount

  const messageInfo = await getMessageInfo()
  const {
    dayHour,
    publishFailed,
    publishSuccessed,
    subscribeSuccessed,
    subscribeFailed,
  } = messageInfo.data;
  messageChartOption.xAxis.data = dayHour
  messageChartOption.series[0].data = publishFailed
  messageChartOption.series[1].data = publishSuccessed
  messageChartOption.series[2].data = subscribeFailed
  messageChartOption.series[3].data = subscribeSuccessed
  // 健康数据
  const healthRes: any = await getHealthChecks()
  Object.assign(healthChecks, healthRes[0].entries)
}

watch(
    () => themeConfig.value.isIsDark,
    (isIsDark) => {
      nextTick(() => {
        charts.theme = isIsDark ? 'dark' : '';
        charts.bgColor = isIsDark ? 'transparent' : '';
        charts.color = isIsDark ? '#dadada' : '#303133';
        setTimeout(() => {
          // initLineChart();
        }, 500);
        setTimeout(() => {
          initPieChart(onlineChartRef, onlineChartOption as EChartsOption, 'onlineChart');
          initPieChart(warningChartRef, warningChartOption as EChartsOption, 'warningChart');
          initPieChart(messageChartRef, messageChartOption as EChartsOption, 'messageChart');
        }, 700);
        setTimeout(() => {
          // initBarChart();
        }, 1000);
      });
    },
    {
      deep: true,
      immediate: true,
    }
);


</script>
<style scoped lang="scss">
.home-container {
  overflow: hidden;

  .home-card-two,
  .home-card-three {
    .home-card-item {
      height: 400px;
      width: 100%;
      overflow: hidden;

      .home-monitor {
        height: 100%;

        .flex-warp-item {
          width: 25%;
          height: 111px;
          display: flex;

          .flex-warp-item-box {
            margin: auto;
            text-align: center;
            color: var(--el-text-color-primary);
            display: flex;
            border-radius: 5px;
            background: var(--next-bg-color);
            cursor: pointer;
            transition: all 0.3s ease;

            &:hover {
              background: var(--el-color-primary-light-9);
              transition: all 0.3s ease;
            }
          }
        }
      }
    }
  }
}
</style>
