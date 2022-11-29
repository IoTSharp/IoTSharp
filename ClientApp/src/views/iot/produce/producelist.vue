<template>
  <div class="system-list-container">
    <el-card shadow="hover">
      <div class="system-dept-search mb15">
        <el-form size="default" label-width="100px" class="mt35 mb35">
          <el-row :gutter="35">
            <el-col :xs="24" :sm="12" :md="8" :lg="8" :xl="8" class="mb20">
              <el-form-item label="产品名称">
                <el-input v-model="name" placeholder="产品名称" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>

        <el-button size="default" type="primary" class="ml10" @click="getData()">
          <el-icon>
            <ele-Search />
          </el-icon>
          查询
        </el-button>
      </div>
      <el-table :data="tableData.rows" style="width: 100%" row-key="id">
        <el-table-column
          v-if="false"
          prop="id"
          label="id"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column
          prop="name"
          label="产品名称"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column
          prop="defaultIdentityType"
          label="认证方式"
          show-overflow-tooltip
        ></el-table-column>
        <el-table-column
          prop="defaultTimeout"
          label="超时"
          show-overflow-tooltip
        ></el-table-column>
        <el-table-column
          prop="description"
          label="备注"
          show-overflow-tooltip
        ></el-table-column>
        <el-table-column
          prop="endDateTime"
          label="状态"
          show-overflow-tooltip
        ></el-table-column>

        <el-table-column label="操作" show-overflow-tooltip width="200">
          <template #default="scope">
            <el-button
              size="small"
              text
              type="primary"
              @click.prevent="editprod(scope.row)"
              >修改
            </el-button>
            <el-button
              size="small"
              text
              type="primary"
              @click.prevent="editprop(scope.row)"
              >属性
            </el-button>
            <el-button
              size="small"
              text
              type="primary"
              @click.prevent="editdict(scope.row)"
              >字典
            </el-button>
            <el-button
              size="small"
              text
              type="primary"
              @click.prevent="creatdevice(scope.row)"
              >创建设备
            </el-button>

            <el-button
              size="small"
              text
              type="primary"
              @click.prevent="navtodevice(scope.row)"
              >管理设备
            </el-button>
            <el-button
              size="small"
              text
              type="primary"
              @click.prevent="deleteprod(scope.row)"
              >删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination
        @size-change="onHandleSizeChange"
        @current-change="onHandleCurrentChange"
        class="mt15"
        :pager-count="5"
        :page-sizes="[10, 20, 30]"
        v-model:current-page="tableData.param.pageNum"
        background
        v-model:page-size="tableData.param.pageSize"
        layout="total, sizes, prev, pager, next, jumper"
        :total="tableData.total"
      >
      </el-pagination>
    </el-card>
    <produceform ref="produceformRef" />
    <producedatadictionaryform ref="producedatadictionaryformRef" />
        <producepropform ref="producepropformRef" />
    <!-- <flowdesigner ref="flowdesignerRef" /> -->
  </div>
</template>

<script lang="ts">
import { ref, toRefs, reactive, onMounted, defineComponent } from "vue";
import {
  ElMessageBox,
  ElMessage,
  ElButton,
  ElCard,
  ElIcon,
  ElInput,
  ElPagination,
  ElTable,
  ElTableColumn,
} from "element-plus";
import produceform from "./produceform.vue";
import producepropform from "./producepropform.vue";
import producedatadictionaryform from "./producedatadictionaryform.vue";

import { Session } from "/@/utils/storage";
import { getProduceList } from "/@/api/produce";
import { appmessage } from "/@/api/iapiresult";
// 定义接口来定义对象的类型

interface TableDataRow {
  id?: string;
  name?: string;
  defaultIdentityType?: string;
  defaultTimeout?: string;
  description?: string;
  devices?: Array<SubTableDataRow>;
}
interface SubTableDataRow {
  id?: string;
  name?: string;
  deviceType?: string;
  timeout?: string;
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
  name: "producelist",
  components: { produceform, producedatadictionaryform,producepropform },
  setup() {
    const produceformRef = ref();
    const producedatadictionaryformRef = ref();
    const producepropformRef = ref();
    
    const userInfos = Session.get("userInfo");
    const router = useRouter();
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
      name: "",
    });
    const onHandleSizeChange = (val: number) => {
      state.tableData.param.pageSize = val;
      getData();
    };
    // 分页改变
    const onHandleCurrentChange = (val: number) => {
      state.tableData.param.pageNum = val;
      getData();
    };

    const getData = () => {
      console.log(query);
      getProduceList({
        offset: state.tableData.param.pageNum - 1,
        limit: state.tableData.param.pageSize,
      }).then((res) => {
        state.tableData.rows = res.data.rows;
        state.tableData.total = res.data.total;
      });
    };
    // 初始化表格数据
    const initTableData = () => {
      getData();
    };
    onMounted(() => {
      initTableData();
    });

    const creatprod = () => {
      produceformRef.value.openDialog("0000000-0000-0000-0000-000000000000");
    };
    const editprod = (row: TableDataRow) => {

      produceformRef.value.openDialog(row.id);
    };
    const editprop = (row: TableDataRow) => {
      producepropformRef.value.openDialog(row.id);
      

    };
    const editdict = (row: TableDataRow) => {
      producedatadictionaryformRef.value.openDialog();
    };
    const creatdevice = (row: TableDataRow) => {};
    const navtodevice = (row: TableDataRow) => {};
    const deleteprod = (row: TableDataRow) => {};
    return {
      produceformRef,producedatadictionaryformRef,producepropformRef,
      editprod,
      editprop,
      editdict,
      creatdevice,
      navtodevice,
      deleteprod,
      onHandleSizeChange,
      onHandleCurrentChange,
      getData,
      ...toRefs(state),
      ...toRefs(query),
    };
  },
});
</script>
