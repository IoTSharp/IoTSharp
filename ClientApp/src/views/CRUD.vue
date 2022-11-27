<template>
  <div class="z-crud">
    <fs-crud ref="crudRef" v-bind="crudBinding"/>
<!--    <fs-page height="800px">-->
<!--      -->
<!--    </fs-page>-->
  </div>
</template>

<script>
import { defineComponent, ref, onMounted } from "vue";
import { useCrud } from "@fast-crud/fast-crud";
import { useExpose } from "@fast-crud/fast-crud";
import _ from 'lodash-es'

//此处为crudOptions配置
const createCrudOptions = function ({ expose }) {
  const records = [{id:1,name:'Hello World'}]
  const pageRequest = async (query) => {
    return {
      records, currentPage:1,pageSize:20,total:records.length
    }
  };
  const editRequest = async ({ form, row }) => {
    const target = _.find(records,item=>{return row.id === item.id})
    _.merge(target,form)
    return target;
  };
  const delRequest = async ({ row }) => {
    _.remove(records,item=>{return item.id === row.id})
  };

  const addRequest = async ({ form }) => {
    const maxRecord = _.maxBy(records,item=>{return item.id})
    form.id = (maxRecord?.id||0)+1
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
      columns: {
        name: {
          title: "姓名",
          type: "text",
          search: {show: true},
          form: {
            component: {
              maxlength: 20
            }
          }
        },
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
  height: calc(100vh - 120px);
}

</style>
