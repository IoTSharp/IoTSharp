<template>
  <div class="home-container">
    首页
    <el-row :gutter="15" class="home-card-one mb15">
<!--      <el-col-->
<!--          :xs="24"-->
<!--          :sm="12"-->
<!--          :md="12"-->
<!--          :lg="6"-->
<!--          :xl="6"-->
<!--          v-for="(v, k) in homeOne"-->
<!--          :key="k"-->
<!--          :class="{ 'home-media home-media-lg': k > 1, 'home-media-sm': k === 1 }"-->
<!--      >-->
<!--        <div class="home-card-item flex">-->
<!--          <div class="flex-margin flex w100" :class="` home-one-animation${k}`">-->
<!--            <div class="flex-auto">-->
<!--              <span class="font30">{{ v.num1 }}</span>-->
<!--              <span class="ml5 font16" :style="{ color: v.color1 }">{{ v.num2 }}%</span>-->
<!--              <div class="mt10">{{ v.num3 }}</div>-->
<!--            </div>-->
<!--            <div class="home-card-item-icon flex" :style="{ background: `var(${v.color2})` }">-->
<!--              <i class="flex-margin font32" :class="v.num4" :style="{ color: `var(${v.color3})` }"></i>-->
<!--            </div>-->
<!--          </div>-->
<!--        </div>-->
<!--      </el-col>-->
    </el-row>
  </div>

</template>
<script lang="ts" setup>
const temp1 =  [
  {
    num1: '125,12',
    num2: '-12.32',
    num3: '订单统计信息',
    num4: 'fa fa-meetup',
    color1: '#FF6462',
    color2: '--next-color-primary-lighter',
    color3: '--el-color-primary',
  },
  {
    num1: '653,33',
    num2: '+42.32',
    num3: '月度计划信息',
    num4: 'iconfont icon-ditu',
    color1: '#6690F9',
    color2: '--next-color-success-lighter',
    color3: '--el-color-success',
  },
  {
    num1: '125,65',
    num2: '+17.32',
    num3: '年度计划信息',
    num4: 'iconfont icon-zaosheng',
    color1: '#6690F9',
    color2: '--next-color-warning-lighter',
    color3: '--el-color-warning',
  },
  {
    num1: '520,43',
    num2: '-10.01',
    num3: '访问统计信息',
    num4: 'fa fa-github-alt',
    color1: '#FF6462',
    color2: '--next-color-danger-lighter',
    color3: '--el-color-danger',
  },
]
const homeOne = reactive(temp1)
const test = ref('')
</script>
