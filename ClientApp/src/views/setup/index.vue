<template>
  <div class="z-install">
    <div class="content " :style="{height: storesAppInfo.appInfo.installed ? '240px': '580px'}">
      <div class="login-wrap">
        <div class="logo flex justify-center">
          <AppLogo></AppLogo>

        </div>
        <div v-if="storesAppInfo.appInfo.installed" class="installed">
          系统已初始化， 请直接登录
          <Button type="primary" style="margin-top: 10px" @click="$router.replace({name: 'login'})">跳转至登录页面
          </Button>
        </div>
        <div v-else>
          <div class="form-subtitle"> 欢迎使用, 请填写邮箱, 密码以及账号, 用于初始化系统</div>
          <FormCreate
              v-model:api="fApi"
              :rule="rules"
              :option="options"
              v-model="value"
              @submit="onSubmit"
          ></FormCreate>
        </div>
      </div>
    </div>
    <div class="copyright-wrap">
      <p>系统版本</p>
      <p>{{ storesAppInfo.appInfo.version }}</p>
    </div>
  </div>

</template>
<script setup lang="ts">
import AppLogo from "/@/components/AppLogo.vue";
import setup_form_rules from './setup_form_rules.json'
import setup_form_option from './setup_form_option.json'
import {initSysAdmin} from "/@/api/installer";
import {reactive, ref} from "vue";
import formCreate from "@form-create/element-ui";
import {ElButton as Button, ElMessage} from 'element-plus'
import { useAppInfo } from "/@/stores/appInfo";

import {useRouter} from "vue-router";

const FormCreate = formCreate.$form()
const router = useRouter()
const storesAppInfo = useAppInfo()
//实例对象
const fApi = ref({})
//表单数据
const value = ref({})
// const options = ref(setup_form_option)
const options = reactive(setup_form_option)

const validatePassCheck = (rule:any, value:any, callback:any) => {
  if (value === '') {
    callback(new Error('请再次输入密码'));
  } else if (value !== fApi.value.form.password) {
    callback(new Error('两次输入的密码不一致!'));
  } else {
    callback();
  }
};
setup_form_rules[3].validate.push({
  required: true,
  trigger: "change",
  validator: validatePassCheck
})
JSON.parse(JSON.stringify(setup_form_rules))
const rules = ref(setup_form_rules)

function onSubmit(data) {
  try {
    fApi.value.validate(async (valid) => {
      if (valid) {
        try {
          await initSysAdmin(data)
          ElMessage.success('初始化成功')
          await router.replace({name: 'login'})
        } catch (e) {
          ElMessage.error('初始化失败')
        }
      } else {
        ElMessage.error('请正确填写信息')
      }
    })
  } catch (e) {
    ElMessage.error('请正确填写信息')
  }
}


</script>
<style lang="scss">
.z-install {
  .el-form-item {
    margin-top: 16px;
  }

  .el-form--large.el-form--label-top .el-form-item .el-form-item__label {
    margin-bottom: 4px !important;
  }

  .el-button {
    width: 100%;
    margin-top: 10px;
  }

  .content {
    position: absolute;
    box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);
    left: 50%;
    top: 40%;
    margin-left: -211px;
    margin-top: -290px;
    width: 422px;
    height: 680px;
    background-repeat: no-repeat;
    background-size: 100% 100%;
    overflow: hidden;
    background-color: #fff;
    border-radius: 18px;
  }

  .login-wrap {
    padding: 40px 60px;
    box-sizing: border-box;
    margin: 0 auto;
  }

  .form-title {
    font-size: 24px;
    font-weight: bold;
    text-align: center;
    margin-top: 20px;
  }

  .form-subtitle {
    font-size: 12px;
    color: gray;
    text-align: center;
    margin-bottom: 12px;
    margin-top: 8px;
  }

  .logo {
    width: 100%;
  }

  .copyright-wrap {
    position: absolute;
    bottom: 20px;
    width: 100%;
    text-align: center;
    color: #afafaf;
    font-size: 12px;
  }

  .installed {
    text-align: center;
    font-size: 16px;
    font-weight: bold;
    margin-top: 20px;
    color: var(--el-text-color-regular);
  }
}


</style>
