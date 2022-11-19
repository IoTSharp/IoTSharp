<template>
  <div>
    <el-drawer v-model="drawer" :title="dialogtitle" size="50%">
      <div class="add-form-container">
        <el-form :model="dataForm" size="default" label-width="90px">
          <el-row :gutter="35">
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="规则链名称">
                <el-input
                  v-model="dataForm.name"
                  placeholder="请输入规则链名称"
                  clearable
                ></el-input>
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="事件类型">
                <el-select v-model="dataForm.mountType" placeholder="请选择事件类型">
                  <el-option
                    v-for="item in mountTypes"
                    :key="item.value"
                    :label="item.value"
                    :value="item.value"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
              <el-form-item label="规则链描述">
                <el-input
                  v-model="dataForm.ruleDesc"
                  placeholder="请输入规则链描述信息"
                  clearable
                ></el-input>
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
import { ruleApi } from "/@/api/flows";
import { appmessage } from "/@/api/iapiresult";

interface ruleform {
  drawer: boolean;
  dialogtitle: string;
  dataForm: ruleaddoreditdto;
  mountTypes: Array<any>;
}

export default defineComponent({
  name: 'addflow',
  components: {},
  setup(props) {
    const state = reactive<ruleform>({
      drawer: false,
      dialogtitle: '',
      mountTypes: [
        {
          value: 'Telemetry',
          label: '遥测',
        },
        {
          value: 'Attribute',
          label: '属性',
        },
        {
          value: 'RAW',
          label: 'RAW',
        },
        {
          value: 'RPC',
          label: 'RPC',
        },
        {
          value: 'Online',
          label: 'Online',
        },
        {
          value: 'Offline',
          label: 'Offline',
        },
        {
          value: 'TelemetryArray',
          label: '遥测数组',
        },
        {
          value: 'Alarm',
          label: '告警',
        },
      ],
      dataForm: {
        ruleId: '00000000-0000-0000-0000-000000000000',
        name: '',
        ruleDesc: '',
        mountType: '',
      },
    });

    const openDialog = (ruleid: string) => {
      console.log(ruleid)
      if (ruleid === '00000000-0000-0000-0000-000000000000') {
        state.dataForm = {
          ruleId: '00000000-0000-0000-0000-000000000000',
          name: '',
          ruleDesc: '',
          mountType: '',
        };
        state.dialogtitle = '新增规则链';
      } else {
        state.dialogtitle = '修改规则链';
        ruleApi()
          .getrule(ruleid)
          .then((res) => {
            state.dataForm = res.data;
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
      if (state.dataForm.ruleId === '00000000-0000-0000-0000-000000000000') {
        ruleApi()
          .postrule(state.dataForm)
          .then((res: appmessage<boolean>) => {
            if (res.code === 10000 && res.data) {
              ElMessage.success('新增成功');
            } else {
              ElMessage.warning('新增失败:' + res.msg);
            }
          });
      } else {
        ruleApi()
          .putrule(state.dataForm)
          .then((res: appmessage<boolean>) => {
            if (res.code === 10000 && res.data) {
              ElMessage.success('修改成功');
            } else {
              ElMessage.warning('修改失败:' + res.msg);
            }
          });
      }
    };
    return { ...toRefs(state), onSubmit, openDialog, closeDialog };
  },
});
</script>
