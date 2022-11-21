<template>
  <el-form size="large" class="login-content-form">
    <el-form-item class="login-animation1">
      <el-input
        type="text"
        placeholder="请输入用户名"
        v-model="ruleForm.userName"
        clearable
        autocomplete="off"
      >
        <template #prefix>
          <el-icon class="el-input__icon"><ele-User /></el-icon>
        </template>
      </el-input>
    </el-form-item>
    <el-form-item class="login-animation2">
      <el-input
        :type="isShowPassword ? 'text' : 'password'"
        placeholder="请输入密码"
        v-model="ruleForm.password"
        autocomplete="off"
      >
        <template #prefix>
          <el-icon class="el-input__icon"><ele-Unlock /></el-icon>
        </template>
        <template #suffix>
          <i
            class="iconfont el-input__icon login-content-password"
            :class="isShowPassword ? 'icon-yincangmima' : 'icon-xianshimima'"
            @click="isShowPassword = !isShowPassword"
          >
          </i>
        </template>
      </el-input>
    </el-form-item>

    <el-form-item class="login-animation4">
      <el-button
        type="primary"
        class="login-content-submit"
        size="large"
        @click="onSignIn"
        :loading="loading.signIn"
      >
        <span>{{ $t("message.account.accountBtnText") }}</span>
      </el-button>
    </el-form-item>


    <div class="mt10-40px login-animation5 text-center">
      没有账号?
      <router-link to="/signup"> <el-link type="primary" :underline="false" >立即注册 </el-link></router-link>
    </div>
  </el-form>




  <el-dialog v-model="dialogVisible" title="" width="400px"  >
    <slide-verify
      ref="block"
      :imgs="imgs"
      slider-text="向右滑动->"
      :accuracy="accuracy"
      @again="onAgain"
      @success="onSuccess"
      @fail="onFail"
      @refresh="onRefresh"
    ></slide-verify>
  </el-dialog>
</template>

<script lang="ts">
import { toRefs, reactive, defineComponent, computed ,ref} from "vue";
import { useRoute, useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import { useI18n } from "vue-i18n";
import Cookies from "js-cookie";
import { storeToRefs } from "pinia";
import { useThemeConfig } from "/@/stores/themeConfig";
import { initFrontEndControlRoutes } from "/@/router/frontEnd";
import { initBackEndControlRoutes } from "/@/router/backEnd";
import { Session } from "/@/utils/storage";
import { formatAxis } from "/@/utils/formatTime";
import { NextLoading } from "/@/utils/loading";
import { useLoginApi } from "/@/api/login";
import SlideVerify, { SlideVerifyInstance } from "vue3-slide-verify";
import "vue3-slide-verify/dist/style.css";
export default defineComponent({
  components: { SlideVerify },
  name: "loginAccount",
  setup() {
    const block = ref<SlideVerifyInstance>();
    const { t } = useI18n();
    const storesThemeConfig = useThemeConfig();
    const { themeConfig } = storeToRefs(storesThemeConfig);
    const route = useRoute();
    const router = useRouter();
    const state = reactive({
      dialogVisible: false,
      isShowPassword: false,
      accuracy: 3,
      msg: "",
      imgs: [import.meta.env.VITE_API_URL+'/api/Captcha/Imgs'],
      ruleForm: {
        userName: "iotmaster@iotsharp.net",
        password: "",
        code: "1234",
      },
      loading: {
        signIn: false,
      },
    });
    // 时间获取
    const currentTime = computed(() => {
      return formatAxis(new Date());
    });
    // 登录
    const onSignIn = async () => {
      state.dialogVisible = true;
      // 存储 token 到浏览器缓存
    };
    const onAgain = () => {
      state.msg = "检测到非人为操作的哦！ try again";
      // 刷新
      block.value?.refresh();
    };

    const onSuccess = (times: number) => {
      state.msg = `login success, 耗时${(times / 1000).toFixed(1)}s`;
      state.loading.signIn = true;
      useLoginApi()
        .signIn({ password: state.ruleForm.password, userName: state.ruleForm.userName })
        .then(async (res: any) => {
          if (res && res.code === 10000) {
            Session.set("token", res.data.token.access_token);
            // 模拟数据，对接接口时，记得删除多余代码及对应依赖的引入。用于 `/src/stores/userInfo.ts` 中不同用户登录判断（模拟数据）
            Cookies.set("userName", state.ruleForm.userName);
            if (!themeConfig.value.isRequestRoutes) {
              // 前端控制路由，2、请注意执行顺序
              await initFrontEndControlRoutes();
              signInSuccess();
            } else {
              // 模拟后端控制路由，isRequestRoutes 为 true，则开启后端控制路由
              // 添加完动态路由，再进行 router 跳转，否则可能报错 No match found for location with path "/"
              await initBackEndControlRoutes();
              // 执行完 initBackEndControlRoutes，再执行 signInSuccess
              signInSuccess();
            }
          } else {
            state.loading.signIn = false;
            ElMessage.success(`用户名不存在或者密码错误`);
          }
        })
        .finally(() => {});
    };

    const onFail = () => {
      state.msg = "验证不通过";
    };

    const onRefresh = () => {
      state.msg = "点击了刷新小图标";
    };

    const handleClick = () => {
      // 刷新
      block.value?.refresh();
      state.msg = "";
    };
    // 登录成功后的跳转
    const signInSuccess = () => {
      // 初始化登录成功时间问候语
      let currentTimeInfo = currentTime.value;
      // 登录成功，跳到转首页
      // 如果是复制粘贴的路径，非首页/登录页，那么登录成功后重定向到对应的路径中
      if (route.query?.redirect) {
        router.push({
          path: <string>route.query?.redirect,
          query:
            Object.keys(<string>route.query?.params).length > 0
              ? JSON.parse(<string>route.query?.params)
              : "",
        });
      } else {
        router.push("/");
      }
      // 登录成功提示
      // 关闭 loading
      state.loading.signIn = true;
      const signInText = t("message.signInText");
      ElMessage.success(`${currentTimeInfo}，${signInText}`);
      // 添加 loading，防止第一次进入界面时出现短暂空白
      NextLoading.start();
    };
    return {
      onSignIn,
      onAgain,
      onSuccess,
      onFail,
      onRefresh,
      ...toRefs(state),
    };
  },
});
</script>

<style scoped lang="scss">
.login-content-form {
  margin-top: 20px;
  @for $i from 1 through 5 {
    .login-animation#{$i} {
      opacity: 0;
      animation-name: error-num;
      animation-duration: 0.5s;
      animation-fill-mode: forwards;
      animation-delay: calc($i/10) + s;
    }
  }
  .login-content-password {
    display: inline-block;
    width: 20px;
    cursor: pointer;
    &:hover {
      color: #909399;
    }
  }
  .login-content-code {
    width: 100%;
    padding: 0;
    font-weight: bold;
    letter-spacing: 5px;
  }
  .login-content-submit {
    width: 100%;
    letter-spacing: 2px;
    margin-top: 15px;
  }
}
</style>
