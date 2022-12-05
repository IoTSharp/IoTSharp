<template>
  <div>
    <el-drawer v-model="drawer" :title="dialogtitle" size="100%">
      <el-card shadow="hover" header="表单表格验证">
        <el-form ref="tableRulesRef" :model="tableData" size="default">
          <el-table :data="tableData.data" border class="module-table-uncollected">
            <el-table-column
              v-for="(item, index) in tableData.header"
              :key="index"
              show-overflow-tooltip
              :prop="item.prop"
              :width="item.width"
              :label="item.label"
            >
              <!-- <template v-slot:header>
                  <span v-if="item.isRequired" class="color-danger">*</span>
                  <span class="pl5">{{ item.label }}</span>
                  <el-tooltip
                    v-if="item.isTooltip"
                    effect="dark"
                    content="这是tooltip"
                    placement="top"
                  >
                    <i class="iconfont icon-quanxian" />
                  </el-tooltip>
                </template> -->
              <template v-slot="scope">
                <!-- <el-form-item
                    :prop="`data.${scope.$index}.${item.prop}`"
                    :rules="[
                      {
                        required: item.isRequired,
                        message: '不能为空',
                        trigger: `${item.type}` == 'input' ? 'blur' : 'change',
                      },
                    ]"
                  > -->
                <el-switch v-if="item.type === 'switch'" v-model="scope.row[item.prop]" />

                <el-select
                  v-if="item.type === 'select'"
                  v-model="scope.row[item.prop]"
                  placeholder="请选择"
                >
                  <el-option
                    v-for="sel in item.enmus"
                    :key="sel.id"
                    :label="sel.label"
                    :value="sel.value"
                  />
                </el-select>
                <el-date-picker
                  v-else-if="item.type === 'date'"
                  v-model="scope.row[item.prop]"
                  type="date"
                  placeholder="选择日期"
                  style="width: 100%"
                />
                <el-input
                  v-else-if="item.type === 'input'"
                  v-model="scope.row[item.prop]"
                  placeholder="请输入内容"
                />
                <el-input
                  v-else-if="item.type === 'dialog'"
                  v-model="scope.row[item.prop]"
                  readonly
                  placeholder="请输入内容"
                >
                  <template v-slot:suffix>
                    <i class="iconfont icon-shouye_dongtaihui" />
                  </template>
                </el-input>
                <!-- </el-form-item> -->
              </template>
            </el-table-column>

            <el-table-column>
              <template #default="scope">
                <el-button text type="primary" @click.prevent="deleterow(scope.row)"
                  >删除
                </el-button>
              </template></el-table-column
            >
          </el-table>
        </el-form>
        <el-row class="flex mt15">
          <div class="flex-margin">
            <!-- <el-button size="default" type="success" @click="onValidate">验证</el-button> -->
            <el-button size="default" type="primary" @click="onAddRow">新增</el-button>
            <el-button size="default" type="primary" @click="save">保存</el-button>
          </div>
        </el-row>
      </el-card>
    </el-drawer>
  </div>
</template>

<script lang="ts">
import { defineComponent, toRefs, reactive, ref } from "vue";
import { ElMessage } from "element-plus";

import { editProduceData, getProduceData } from "/@/api/produce";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
interface TableHeader {
  prop: string;
  width: string | number;
  label: string;
  isRequired?: boolean;
  isTooltip?: boolean;
  type: string;
  enmus?: any[];
}
interface TableRulesState {
  deviceid: string;
  drawer: boolean;
  dialogtitle: string;
  tableData: {
    data: dictrowitem[];
    header: TableHeader[];
  };
}

interface dictrowitem {
  id?: string;
  keyName?: string;
  dataSide?: string;
  type?: string;
}
export default defineComponent({
  name: "producepropform",
  setup() {
    const tableRulesRef = ref();
    const state = reactive<TableRulesState>({
      deviceid: "",
      drawer: false,
      dialogtitle: "",
      tableData: {
        data: [],
        header: [
          {
            prop: "keyName",
            width: "",
            label: "字段名称",
            isRequired: false,
            type: "input",
          },
          {
            prop: "type",
            width: "",
            label: "数据类型",
            isRequired: false,
            type: "select",
            enmus: [
              { value: "Boolean", label: "Boolean" },
              { value: "String", label: "String" },
              { value: "Long", label: "Long" },
              { value: "Double", label: "Double" },
              { value: "Json", label: "Json" },
              { value: "XML", label: "XML" },
              { value: "Binary", label: "Binary" },
              { value: "DateTime", label: "DateTime" },
            ],
          },
          {
            prop: "dataSide",
            width: "",
            label: "数据侧",
            isRequired: false,
            type: "select",
            enmus: [
              { value: "AnySide", label: "AnySide" },
              { value: "ServerSide", label: "ServerSide" },
              { value: "ClientSide", label: "ClientSide" },
            ],
          },
        ],
      },
    });

    const openDialog = (deviceid: string) => {
      state.deviceid = deviceid;
      getProduceData(deviceid).then((x) => {
        console.log(x.data);
        state.tableData.data = x.data.map((x) => {
          return { id: uuidv4(), keyName: x.keyName, dataSide: x.dataSide, type: x.type };
        });
      });

      state.drawer = true;
    };
    // 关闭弹窗
    const closeDialog = () => {
      state.drawer = false;
    };

    // 验证
    const onValidate = () => {
      if (state.tableData.data.length <= 0) return ElMessage.warning("请先点击增加一行");
      tableRulesRef.value.validate((valid: any) => {
        if (!valid) return ElMessage.warning("表格项必填未填");
        ElMessage.success("全部验证通过");
      });
    };

    // 新增一行
    const onAddRow = () => {
      state.tableData.data.push({
        id: uuidv4(),
        keyName: "",
        dataSide: "",
        type: "",
      });
    };
    const deleterow = (row: dictrowitem) => {
      state.tableData.data = state.tableData.data.filter((c) => c.id !== row.id);
    };
    const save = () => {
      editProduceData({
        produceId: state.deviceid,
        produceData: state.tableData.data,
      }).then((x) => {});
    };
    return {
      save,
      deleterow,
      onValidate,
      onAddRow,
      openDialog,
      closeDialog,
      tableRulesRef,
      ...toRefs(state),
    };
  },
});
</script>
