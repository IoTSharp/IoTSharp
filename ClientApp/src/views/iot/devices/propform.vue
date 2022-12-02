<template>
  <div>
    <el-drawer v-model="drawer" :title="dialogtitle" size="75%"
      ><fs-page>
        <div class="m-5">
          <el-row :gutter="10">
            <el-col>
              <el-card header="服务侧属性">
                <fs-form ref="formserverRef" v-bind="formServerOptions" />
                <div style="margin-top: 10px">
                  <el-button @click="formSubmit">提交表单</el-button>
                  <el-button @click="formReset">重置表单</el-button>
                </div>
              </el-card>


            </el-col>
          </el-row>
        </div>
      </fs-page>
    </el-drawer>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from "vue";
import { ElMessage } from "element-plus";
import { useCrud, useExpose, useColumns } from "@fast-crud/fast-crud";
import monaco from "/@/components/monaco/monaco.vue";

export default defineComponent({
  name: "propform",
  setup() {
    const state = reactive({ drawer: false, dialogtitle: "" });

    const createServerFormOptions = () => {
      // 自定义表单配置
      const { buildFormOptions } = useColumns();
      //使用crudOptions结构来构建自定义表单配置
      return buildFormOptions({
        columns: {
         
          groupField: {
            title: "分组字段",
            type: "text",
            form: {
              
              component: {
                //局部引用子表格，要用shallowRef包裹
                name: shallowRef(monaco),
                vModel: "modelValue",
                on: {
                  //处理自定义事件
                  change(context) {
                    console.log("自定义事件", context);
                  },
                },
              },
              rules: [{ required: true, message: "此项必填" }],
            },
          },
        },
        form: {
          labelWidth: "120px",
          group: {
            groups: {
              testGroupName: {
                header: "分组测试",
                columns: ["groupField"],
              },
            },
          },

          doSubmit({ form }) {
            console.log("form submit:", form);
            ElMessage.info("自定义表单提交:" + JSON.stringify(form));
            ElMessage.success("保存成功");
          },
        },
      });
    };

 


    const useFormDirect = () => {
      const formserverRef = ref();
      const formServerOptions = ref();
      formServerOptions.value = createServerFormOptions();
      formServerOptions.value.initialForm = { customField: "初始值" };
 
      function formSubmit() {
        formserverRef.value.submit();
      }
      function formReset() {
        formserverRef.value.reset();
      }
      return {
        formServerOptions,
        formserverRef,
        formSubmit,
        formReset,
      };
    };

    const openDialog = () => {
      state.drawer = true;
    };
    return {
      ...toRefs(state),
      openDialog,
      ...useFormDirect(),
    };
  },
});
</script>
