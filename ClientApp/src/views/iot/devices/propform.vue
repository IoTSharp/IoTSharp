<template>
    <div class="system-list-container">
    </div>
  </template>
  
  <script lang="ts">
  import { ref, toRefs, reactive, onMounted, defineComponent } from "vue";
  import { Session } from "/@/utils/storage";
  import { useRouter } from "vue-router";
  // 定义接口来定义对象的类型
  interface TableDataRow {
    ruleId?: string;
   
  }
  interface TableDataState {
    tableData: {
      rows: Array<TableDataRow>;
      total: number;
      loading: boolean;
      param: {
        pageNum: number;
        pageSize: number;
      };
    };
  }
  
  export default defineComponent({
    name: "flowlist",
    components: { },
    setup() {

      const userInfos = Session.get("userInfo");
      const router=useRouter();
      const state = reactive<TableDataState>({
        tableData: {
          rows: [],
          total: 0,
          loading: false,
          param: {
            pageNum: 1,
            pageSize: 10,
          },
        },
      });
  
      const query = reactive({
       
      });
  
   
      const initTableData = () => {

      };
  
      onMounted(() => {
        initTableData();
      });
      return {
        ...toRefs(state),
        ...toRefs(query),
   
      };
    },
  });
  </script>
  