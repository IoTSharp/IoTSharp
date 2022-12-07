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
        <el-button size="default" type="success" @click="creatprod()" class="ml10">
          <el-icon>
            <ele-FolderAdd />
          </el-icon>
          新增产品
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

        <el-table-column label="操作" show-overflow-tooltip width="300">
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
              @click.prevent="deleteprod(scope.row)"
              >删除
            </el-button>

            &nbsp;
            <el-dropdown
              size="small"
              @command="
                (command) => {
                  dropdownCommand(scope.row, command);
                }
              "
            >
              <el-button type="primary" size="small" text>
                更多<el-icon class="el-icon--right"><arrow-down /></el-icon>
              </el-button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="prop">属性</el-dropdown-item>
                  <el-dropdown-item command="dict">字典</el-dropdown-item>
                  <el-dropdown-item command="createdev">创建设备</el-dropdown-item>
                  <el-dropdown-item command="managedev">管理设备</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
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
    <propform ref="propformRef"></propform>
    <deviceform ref="deviceformRef"></deviceform>
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

import propform from "./../devices/propform.vue";
import { useRouter } from "vue-router";
import produceform from "./produceform.vue";
import deviceform from "./deviceform.vue";
import producepropform from "./producepropform.vue";
import producedatadictionaryform from "./producedatadictionaryform.vue";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
import { Session } from "/@/utils/storage";
import { getProduceList ,deleteProduce} from "/@/api/produce";
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
  components: {
    produceform,
    producedatadictionaryform,
    producepropform,
    propform,
    deviceform,
  },
  setup() {
    const produceformRef = ref();
    const deviceformRef = ref();
    const producedatadictionaryformRef = ref();
    const producepropformRef = ref();
    const propformRef = ref();
    const userInfos = Session.get("userInfo");
    const router = useRouter();
    const state = reactive<TableDataState>({
      tableData: {
        rows: [],
        total: 0,
        loading: false,
        param: {
          pageNum: 1,
          pageSize: 20,
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
      getProduceList({
        offset: state.tableData.param.pageNum - 1,
        limit: state.tableData.param.pageSize,
        name: "",
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

    const dropdownCommand = (row: any, command: string) => {
      switch (command) {
        case "dict":
          editdict(row);
          break;
        case "createdev":
          creatdevice(row);
          break;
        case "managedev":
          navtodevice(row);
          break;
        case "prop":
          editprop(row);
          break;
      }
    };

    const creatprod = () => {
        //  propformRef.value.openDialog("11c12dfe-8b37-4ddb-805a-00ba802259eb");
    produceformRef.value.openDialog(NIL_UUID);
    };
    const editprod = (row: TableDataRow) => {
      produceformRef.value.openDialog(row.id);
    };
    const editprop = (row: TableDataRow) => {
      producepropformRef.value.openDialog(row.id);
    };
    const editdict = (row: TableDataRow) => {
      producedatadictionaryformRef.value.openDialog(row.id);
    };
    const creatdevice = (row: TableDataRow) => {
      deviceformRef.value.openDialog(row.id);
    };
    const navtodevice = (row: TableDataRow) => {
      router.push({
        path: "/iot/devices/devicelist",
      });
    };
    const deleteprod = (row: TableDataRow) => {

      

    };
    return {
      dropdownCommand,
      deviceformRef,
      produceformRef,
      producedatadictionaryformRef,
      producepropformRef,
      propformRef,
      creatprod,
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
