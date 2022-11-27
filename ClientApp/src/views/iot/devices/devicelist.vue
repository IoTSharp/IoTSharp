<template>
  <el-card>
    <div class="z-crud">
      <fs-crud ref="crudRef" v-bind="crudBinding"/>
    </div>
  </el-card>

</template>

<script lang="ts">
import { defineComponent, ref, onMounted } from "vue";
import { useCrud } from "@fast-crud/fast-crud";
import { useExpose } from "@fast-crud/fast-crud";
import _ from 'lodash-es'
import {deviceApi} from "/@/api/devices";
import {Session} from "/@/utils/storage";
const userInfos = Session.get("userInfo");
import { dict, compute } from "@fast-crud/fast-crud";
//此处为crudOptions配置
const createCrudOptions = function ({ expose }) {
  let records = []
  const FsButton = {
    link: true
  }
  const customSwitchComponent = {
    activeColor: 'var(--el-color-primary)',
    inactiveColor: 'var(el-switch-of-color)'
  }
  const pageRequest = async (query) => {
    const params = reactive({
      offset: query.page.currentPage - 1,
      limit: query.page.pageSize,
      onlyActive: false,
      customerId: userInfos.customer.id,
      name: "",
    });

    const res = await deviceApi().devcieList(params)
    records = res.data.rows
    return {
      records,
      currentPage:1,
      pageSize:20,
      total:res.data.total
    }
  };
  const editRequest = async ({ form, row }) => {
    const newItem = _.clone(form)
    newItem.id = row.id
    const target = _.find(records,item=>{return row.id === item.id})
    try {
      const res = await deviceApi().putdevcie(newItem)
      _.merge(target,form)
      return target;
    } catch (e) {
      ElMessage.error(e.response.msg)
    }
  };
  const delRequest = async ({ row }) => {
    try {
      const res = await deviceApi().deletedevcie(row.id)
      _.remove(records, item => {
        return item.id === row.id
      })
    } catch (e) {
      ElMessage.error(e.response.msg)
    }
  };

  const addRequest = async ({ form }) => {
    const res = await deviceApi().postdevcie(form)
    records.push(form)
    return form
  };
  return {
    crudOptions: {
      request: {
        pageRequest,
        addRequest,
        editRequest,
        delRequest
      },
      table: {
        border: false,
      },
      form: {
        labelWidth: "130px", //
      },
      rowHandle: {
        width: 360,
        dropdown: {
          more: {
            //更多按钮配置
            text: "属性",
            ...FsButton,
            icon: 'operation'
          }
        },

        buttons: {

          view:{
            icon:"view", //图标
            ...FsButton,
             // FsButton的配置，可以修改文本、颜色，也可以修改成图标按钮、纯文本按钮等
            order:1, //排序号，越小则排前面，默认值1
            dropdown:false,//是否折叠此按钮，当配置为true，将会收起到dropdown中
            //点击事件,点击此按钮会触发此方法
          },
          edit:{
            icon:"editPen",
            ...FsButton,
            order:1}, //编辑按钮
          remove:{
            icon:"Delete",
            ...FsButton,
            order:5},//删除按钮
          attrNew: {
            size: "small",
            dropdown: true,
            icon:"plus", //图标
            ...FsButton,
            text:"新增属性",//按钮文字
          },
          attrEdit: {
            size: "small",
            dropdown: true,
            icon:"editPen", //图标
            ...FsButton,
            text:"属性修改",//按钮文字
          },
          token: {
            ...FsButton,
            type: 'primary',
            text:compute(({row})=>{
              if (row.identityType === 'AccessToken') return '获取token'
              else if (row.identityType === 'X509Certificate') return '下载证书 '

            }),//按钮文字
          }
        }
      },
      columns: {
        name: {
          title: "设备名称",
          // type: "text",
          type: "button",
          search: {show: true},
          column: {
           component: {
             ...FsButton,
             type: 'primary',
             on:{
               onClick({row}){
                 console.log('点击事件',row)
               }
             }
           }
          }
        },
        deviceType: {
          title: "设备类型",
          type: "dict-select",
          search: {show: false},
          dict: dict({
            data: [
              { value: 'Gateway', label: "网关", },
              { value: 'Device', label: "设备", color: "warning" }
            ]
          })
        },
        active: {
          title: "在线状态",
          type: "dict-switch",
          search: {show: true},
          dict: dict({
            data: [
              { value: true, label: "在线",  },
              { value: false, label: "离线", color: "danger" }
            ]
          }),
          column: {
          },
          viewForm: {
            show: true,
            component: customSwitchComponent
          },
          addForm: {
            show: false,
            component: customSwitchComponent
          },
          editForm: {
            show: false,
            component: customSwitchComponent
          }
        },
        lastActivityDateTime: {
          title: "最后活动时间",
          type: "text",
          search: {show: false},
          addForm: {
            show: false,
          },
          editForm: {
            show: false,
          }
        },
        identityType: {
          title: "认证方式",
          type: "dict-select",
          search: {show: false},
          dict: dict({
            data: [
              { value: 'AccessToken', label: "AccessToken" },
              { value: 'X509Certificate', label: "X509Certificate" }
            ]
          })
        },
        timeout: {
          column: {
            show: false
          },
          title: "超时",
          type: "text",
          search: {show: false},
        }
      }
    }
  };
}

//此处为组件定义
export default defineComponent({
  name: "HelloWorld",
  setup() {
    // crud组件的ref
    const crudRef = ref();
    // crud 配置的ref
    const crudBinding = ref();
    // 暴露的方法
    const { expose } = useExpose({ crudRef, crudBinding });
    // 你的crud配置
    const { crudOptions } = createCrudOptions({ expose });
    // 初始化crud配置
    // eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
    const { resetCrudOptions } = useCrud({ expose, crudOptions });
    // 你可以调用此方法，重新初始化crud配置
    // resetCrudOptions(options)

    // 页面打开后获取列表数据
    onMounted(() => {
      expose.doRefresh();
    });

    return {
      crudBinding,
      crudRef
    };
  }
});
</script>
<style lang="scss"  scoped>

.z-crud {
  height: calc(100vh - 160px);
}

</style>
