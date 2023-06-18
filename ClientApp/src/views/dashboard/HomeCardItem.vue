<template>
  <el-col
      :xs="24"
      :sm="12"
      :md="12"
      :lg="6"
      :xl="6"

      :class="{ 'home-media home-media-lg': index > 1, 'home-media-sm': index === 1 }"
  >
    <div class="home-card-item flex">
      <div class="flex-margin flex w100" :class="` home-one-animation${index}`">
        <div class="home-card-item-icon flex" :style="{ background: item.iconBackgroundColor }">
          <component  :is="{...item.icon}"
                      class="flex-margin font32" :class="item.icon" :style="{ color: 'white' }"
          ></component>
        </div>
        <div class="flex-auto pl-14px">
          <div class="mt10">{{ item.zKey }}</div>
          <span class="font30 font-black">{{ item.zValue }}</span>
        </div>
      </div>
    </div>
  </el-col>
</template>

<script lang="ts" setup>
import {PropType} from "vue";

interface CardItem {
  zValue: string;
  zKey: string;
  icon: any;
  iconBackgroundColor: string;
}
defineProps({
  item: {
    type: Object as PropType<CardItem>,
    required: true
  },
  index: {
    type: [Number,String],
    required: false
  }
})
</script>

<style scoped lang="scss">
$homeNavLengh: 8;
.home-container {
  overflow: hidden;
  .home-card-one,
  .home-card-two,
  .home-card-three {
    .home-card-item {
      width: 100%;
      height: 130px;
      border-radius: 4px;
      transition: all ease 0.3s;
      padding: 20px;
      overflow: hidden;
      background: var(--el-color-white);
      color: var(--el-text-color-primary);
      border: 1px solid var(--next-border-color-light);
      &:hover {
        box-shadow: 0 2px 12px var(--next-color-dark-hover);
        transition: all ease 0.3s;
      }
      &-icon {
        width: 70px;
        height: 70px;
        border-radius: 8px;
        flex-shrink: 1;
        i {
          color: var(--el-text-color-placeholder);
        }
      }
      &-title {
        font-size: 15px;
        font-weight: bold;
        height: 30px;
      }
    }
  }
  .home-card-one {
    @for $i from 0 through 3 {
      .home-one-animation#{$i} {
        opacity: 0;
        animation-name: error-num;
        animation-duration: 0.5s;
        animation-fill-mode: forwards;
        animation-delay: calc($i/10) + s;
      }
    }
  }
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
          @for $i from 0 through $homeNavLengh {
            .home-animation#{$i} {
              opacity: 0;
              animation-name: error-num;
              animation-duration: 0.5s;
              animation-fill-mode: forwards;
              animation-delay: calc($i/10) + s;
            }
          }
        }
      }
    }
  }
}

</style>
