<template>
  <ElCard style="width:500px !important" shadow="never">
    <div class="text-3xl py-20px">个人中心</div>
    <el-tabs v-model="activeName" class="demo-tabs">
      <el-tab-pane label="基础信息" name="basic">
        <FormCreate
            ref="formRef"
            v-model:api="fApi"
            :rule="rules"
            :option="options"
            @submit="onSubmit"
        ></FormCreate>
      </el-tab-pane>
      <el-tab-pane label="密码修改" name="password">
        <FormCreate
            ref="passwordRef"
            v-model:api="passwordFApi"
            :rule="passwordRules"
            :option="options"
            @submit="onResetPassword"
        ></FormCreate>
      </el-tab-pane>
    </el-tabs>
    <div>

    </div>
  </ElCard>
</template>
<script lang="ts" setup>

import {rule} from './profile_form_rules.ts'
import {passwordFormRules} from './password_form_rules.ts'
import {option} from './profile_form_option.ts'
import formCreate, {Api} from "@form-create/element-ui";
import {useLoginApi} from "/@/api/login";
import {ElMessage} from "element-plus";
import {Ref} from "vue";
import {sleep} from "/@/utils/other";
import {ModifyMyInfo, ModifyMyPassword} from "/@/api/account";

const FormCreate = formCreate.$form()

const activeName = ref('password')
const formRef = ref(null)
const passwordRef = ref(null)


interface profile {
  name: string;
  email: string;
  phoneNumber: string;
}


const rules = ref(rule)
const validatePassCheck = (rule:any, value:any, callback:any) => {
  if (value === '') {
    callback(new Error('请再次输入密码'));
  } else if (value !== passwordFApi.value!.form.passnew) {
    callback(new Error('两次输入的密码不一致!'));
  } else {
    callback();
  }
};
passwordFormRules[2].validate.push({
  required: true,
  trigger: "change",
  validator: validatePassCheck
})
// debugger
const passwordRules = ref(passwordFormRules)
const options = ref(option)
//实例对象
const fApi: Ref<Api | null> = ref(null)
const passwordFApi: Ref<Api | null> = ref(null)


onMounted(async ()=>{
  try {
    const res = await useLoginApi().GetUserInfo({})
    const {email, name, phoneNumber} = res.data as profile
    await sleep(1000)
    fApi.value!.setValue({email, name, phoneNumber})
  } catch (e) {
  }
})
function onSubmit(data:any){
  try {
    fApi.value!.validate(async (valid) => {
      if (valid) {
        try {
          await ModifyMyInfo(data)
          ElMessage.success('修改成功')
        } catch (e) {
          ElMessage.error('修改失败')
        }
      } else {
        ElMessage.error('请正确填写信息')
      }
    })
  } catch (e) {
    ElMessage.error('请正确填写信息')
  }
}

function onResetPassword(data:any) {
  try {
    passwordFApi.value!.validate(async (valid) => {
      if (valid) {
        try {
          await ModifyMyPassword(data)
          ElMessage.success('修改成功')
        } catch (e) {
          ElMessage.error('修改失败')
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
