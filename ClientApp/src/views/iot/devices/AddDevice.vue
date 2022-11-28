<template>
  <div >
    <el-drawer v-model="drawer" :title="dialogtitle" size="50%">
      <div class="add-form-container">
        <el-form :model="dataForm" size="default" label-width="90px">
          <el-row :gutter="35">
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="设备名称">
                <el-input
                  v-model="dataForm.name"
                  placeholder="请输入设备名称"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="设备类型">
                <el-select v-model="dataForm.deviceType" placeholder="请选择设备类型">
                  <el-option
                    v-for="item in deviceTypes"
                    :key="item"
                    :label="item"
                    :value="item"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="超时">
                <el-input
                  v-model="dataForm.timeout"
                  placeholder="请输入超时"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="认证方式">
                <el-select v-model="dataForm.identityType" placeholder="请选择认证方式">
                  <el-option
                    v-for="item in identityTypes"
                    :key="item"
                    :label="item"
                    :value="item"
                  />
                </el-select>
              </el-form-item>
            
            </el-col>


            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
             
              <el-form-item>
                <el-button type="primary" @click="onSubmit">保存</el-button>
                <el-button @click="closeDialog">取消</el-button>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </div>
    </el-drawer>
  </div>
</template>

<script lang="ts">
import { ref, toRefs, reactive, onMounted, defineComponent, watchEffect } from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { deviceApi } from "/@/api/devices";
import { appmessage } from "/@/api/iapiresult";

interface deviceform {
  drawer: boolean;
  dialogtitle: string;
  dataForm: deviceaddoreditdto;
  identityTypes: Array<string>;
  deviceTypes: Array<string>;
}

export default defineComponent({
  name: "addDevice",
  components: {},
  setup(props) {
    const state = reactive<deviceform>({
      drawer: false,
      dialogtitle: '',
      identityTypes: ['AccessToken','DevicePassword','X509Certificate'],
      deviceTypes: ['Device','Gateway'],
      dataForm: {
        id: '0000000-0000-0000-0000-000000000000',
        name: '',
        timeout: 300,
        identityType: '',
        deviceType: '',
      },
    });

    const openDialog = (deviceid: string) => {
      if (deviceid === '0000000-0000-0000-0000-000000000000') {
        state.dataForm={
        id: '0000000-0000-0000-0000-000000000000',
        name: '',
        timeout: 300,
        identityType: '',
        deviceType: '',
      }
        state.dialogtitle = '新增设备';
      } else {
        state.dialogtitle = '修改设备';
        deviceApi()
          .getdevcie(deviceid)
          .then((res) => {
            state.dataForm=res.data
          });
      }
      state.drawer = true;
    };
    // 关闭弹窗
    const closeDialog = () => {
      state.drawer = false;
    };
    
    watchEffect(() => {});

    onMounted(() => {});
    const onSubmit = () => {
      if(state.dataForm.id==='0000000-0000-0000-0000-000000000000'){
        deviceApi().postdevcie(state.dataForm).then((res:appmessage<boolean>)=>{
          if (res.code === 10000&&res.data) {
            ElMessage.success("新增成功");
          } else {
            ElMessage.warning("新增失败:"+res.msg);
          }

        });
      }else{
        deviceApi().putdevcie(state.dataForm).then((res:appmessage<boolean>)=>{
          if (res.code === 10000&&res.data) {
            ElMessage.success("修改成功");
          } else {
            ElMessage.warning("修改失败:"+res.msg);
          }
        });
      }

    };
    return { ...toRefs(state), onSubmit, openDialog, closeDialog };
  },
});
</script>
