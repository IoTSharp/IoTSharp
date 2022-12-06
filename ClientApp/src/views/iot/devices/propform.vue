<template>
  <div>
    <el-drawer v-model="drawer" :title="dialogtitle" size="75%"
      ><fs-page class="fs-page-prop">
        <div class="m-5">
          <el-row :gutter="10">
            <el-col>
              <el-card header="服务侧属性">
                <fs-form ref="formserverRef" v-bind="formserverOptions" />
              </el-card>

              <el-card header="客户侧属性">
                <fs-form ref="formclientRef" v-bind="formclientOptions" />
              </el-card>

              <el-card header="任意侧属性">
                <fs-form ref="formanyRef" v-bind="formanyOptions" />
              </el-card>

              <div style="margin-top: 10px">
                <el-button @click="formSubmit">提交表单</el-button>
                <el-button @click="formReset">重置表单</el-button>
              </div>
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
import { useCrud, useExpose, useColumns, dict } from "@fast-crud/fast-crud";
import monaco from "/@/components/monaco/monaco.vue";
import { deviceApi } from "/@/api/devices";
import { appmessage } from "/@/api/iapiresult";
import dayjs from "dayjs";

interface propformstate{
  drawer:boolean;
  dialogtitle:string;
  devid:string;
}


export default defineComponent({
  name: "propform",
  setup() {
    const state = reactive<propformstate>({ drawer: false, dialogtitle: "", devid: "" });
    const formserverRef = ref();
    const formserverOptions = ref();
    const formclientRef = ref();
    const formclientOptions = ref();
    const formanyRef = ref();
    const formanyOptions = ref();
    const buidForm = async () => {
      console.log(state)
      var result = await deviceApi().getDeviceAttributes(
       state.devid
      );

      var serveropt = {
        form: {
          labelWidth: "120px",
          display: "flex",
        },
        columns: {},
      };

      var clientopt = {
        form: {
          labelWidth: "120px",
          display: "flex",
        },
        columns: {},
      };

      var anyopt = {
        form: {
          labelWidth: "120px",
          display: "flex",
        },
        columns: {},
      };

      var serverval = {};
      var anyval = {};
      var clientval = {};

      for (var item of result.data) {
        switch (item.dataSide) {
          case "AnySide":
            anyval[item.keyName] = item.value;
            anyopt.columns[item.keyName] = buildWegits(item, {});
            break;
          case "ServerSide":
            serverval[item.keyName] = item.value;
            serveropt.columns[item.keyName] = buildWegits(item, {});
            break;
          case "ClientSide":
            clientval[item.keyName] = item.value;
            clientopt.columns[item.keyName] = buildWegits(item, {});
            break;
        }
      }

      const { buildFormOptions } = useColumns();
      formserverOptions.value = buildFormOptions(serveropt);
      formclientOptions.value = buildFormOptions(clientopt);
      formanyOptions.value = buildFormOptions(anyopt);
      formserverRef.value.setFormData(serverval)
      formclientRef.value.setFormData(clientval)
      formanyRef.value.setFormData(anyval)

      
    
    };

    const buildWegits = (data: any, cfg?: any) => {
      switch (data.dataType) {
        case "Boolean":
          return {
            title: data.keyName,
            type: "dict-switch",
            form: {
              col: { span: 24 },
              dict: dict({
                data: [
                  { value: true, label: "开启" },
                  { value: false, label: "关闭" },
                ],
              }),
            },
          };
          break;
        case "DateTime":
          return {
            title: data.keyName,
            type: "datetime",
            search: {
              show: true,
              width: 185,
              component: {},
            },
            valueBuilder({ value, row, key }) {
              if (value != null) {
                row[key] = dayjs(value);
              }
            },
            valueResolve({ value, row, key }) {
              if (value != null) {
                row[key] = value.valueOf();
              }
            },
          };
          break;
        case "Long":
          return {
            title: data.keyName,
            search: { show: true },
            type: "number",
          };
          break;
        case "String":
          return {
            title: data.keyName,
            type: "text", //虽然不写也能正确显示组件，但不建议省略它
            search: { show: true },
            form: {
              component: {
                maxlength: 20,
              },
            },
          };

          break;
        case "Double":
          return {
            title: data.keyName,
            type: "number",
            form: {
              component: {
                step: 0.1,
              },
            },
          };
          break;
        case "Json":
          return {
            title: data.keyName,
            form: {
              col: { span: 24 },
              component: {
                name: shallowRef(monaco),
                vModel: "modelValue",
                on: {
          
                  change(context) {},
                },
              },
              rules: [{ required: true, message: "此项必填" }],
            },
          };
          break;
        case "XML":
          return {
            title: data.keyName,
            form: {
              col: { span: 24 },
              component: {
                name: shallowRef(monaco),
                vModel: "modelValue",
                on: {
                  change(context) {},
                },
              },
              rules: [{ required: true, message: "此项必填" }],
            },
          };
          break;
        case "Binary":
          return {
            title: data.keyName,
            form: {
              col: { span: 24 },
              component: {
         
                name: shallowRef(monaco),
                vModel: "modelValue",
                on: {
                  change(context) {},
                },
              },
              rules: [{ required: true, message: "此项必填" }],
            },
          };
          break;

        default:
          return {
            title: data.keyName,
            type: "text", //虽然不写也能正确显示组件，但不建议省略它
            search: { show: true },
            form: {
              component: {
                maxlength: 1000,
              },
            },
          };
      }
    };

    //const createServerFormOptions = async () => {
    // 自定义表单配置

    // console.log(await buidForm());
    //  return buildFormOptions(buidForm());
    //使用crudOptions结构来构建自定义表单配置
    // return buildFormOptions({
    //   form: {
    //     display: "flex",
    //   },
    //   columns: {
    //     json: {
    //       title: "json",
    //       form: {
    //         col: { span: 24 },
    //         component: {
    //           //局部引用子表格，要用shallowRef包裹
    //           name: shallowRef(monaco),
    //           vModel: "modelValue",
    //           on: {
    //             //处理自定义事件
    //             change(context) {
    //               console.log("自定义事件", context);
    //             },
    //           },
    //         },
    //         rules: [{ required: true, message: "此项必填" }],
    //       },
    //     },
    //
    //     xml: {
    //       title: "xml",
    //       type: "text",
    //       form: {
    //         col: { span: 24 },
    //         component: {
    //           //局部引用子表格，要用shallowRef包裹
    //           name: shallowRef(monaco),
    //           vModel: "modelValue",
    //           on: {
    //             //处理自定义事件
    //             change(context) {
    //               console.log("自定义事件", context);
    //             },
    //           },
    //         },
    //
    //         rules: [{ required: true, message: "此项必填" }],
    //       },
    //     },
    //
    //     text: {
    //       title: "text",
    //       type: "text",
    //       form: {
    //         col: { span: 24 },
    //         // component: {
    //         //   //局部引用子表格，要用shallowRef包裹
    //         //   name: shallowRef(monaco),
    //         //   vModel: "modelValue",
    //         //   on: {
    //         //     //处理自定义事件
    //         //     change(context) {
    //         //       console.log("自定义事件", context);
    //         //     },
    //         //   },
    //         // },
    //
    //         rules: [{ required: true, message: "此项必填" }],
    //       },
    //     },
    //     bin: {
    //       title: "bin",
    //       form: {
    //         col: { span: 24 },
    //         component: {
    //           //局部引用子表格，要用shallowRef包裹
    //           name: shallowRef(monaco),
    //           vModel: "modelValue",
    //           on: {
    //             //处理自定义事件
    //             change(context) {
    //               console.log("自定义事件", context);
    //             },
    //           },
    //         },
    //       },
    //     },
    //   },
    // });
    //};

    const formSubmit = async () => {
      console.log(formserverRef.value.getFormData());
      console.log(formclientRef.value.getFormData());
      console.log(formanyRef.value.getFormData());

      var data={
        anyside:formanyRef.value.getFormData(),
        clientside:formclientRef.value.getFormData(),
        serverside:formserverRef.value.getFormData(),
      }

      var result = await deviceApi().editDeviceAttributes(
        state.devid,data
      );

      if(result.data){


      }

    };
    const formReset = () => {
      formserverRef.value.reset();
      formclientRef.value.reset();
      formanyRef.value.reset();
    };
    const openDialog = (devid: string) => {
      state.devid = devid;
      state.drawer = true;
      console.log(devid)
      console.log(state)
      buidForm();
    };
    return {
      formserverOptions,
      formserverRef,
      formclientOptions,
      formclientRef,
      formanyRef,
      formanyOptions,

      formSubmit,
      formReset,
      ...toRefs(state),
      openDialog,
    };
  },
});
</script>
<style lang="scss" scoped>
.fs-page-prop {
  margin-top: 3rem;
}
</style>
