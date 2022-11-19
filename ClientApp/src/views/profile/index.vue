<template>
  <ElCard style="width:500px !important" shadow="never">
    <div class="text-3xl py-20px">个人中心</div>
    <div>
      <FormCreate
          ref="formRef"
          v-model:api="fApi"
          :rule="rules"
          :option="options"
          @submit="onSubmit"
      ></FormCreate>
    </div>
  </ElCard>
</template>
<script lang="ts" setup>

import {rule} from './profile_form_rules.ts'
import {option} from './profile_form_option.ts'
import formCreate, {Api} from "@form-create/element-ui";
import {useLoginApi} from "/@/api/login";
import {ElMessage} from "element-plus";
import {Ref} from "vue";
import {sleep} from "/@/utils/other";
import {ModifyMyInfo} from "/@/api/account";
const formRef = ref(null)
interface profile {
  name: string;
  email: string;
  phoneNumber: string;
}
const rules = ref(rule)
const options = ref(option)
//实例对象
const fApi: Ref<Api | null> = ref(null)
//表单数据
const FormCreate = formCreate.$form()

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


</script>
