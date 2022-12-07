<template>
  <div>
    <el-drawer v-model="state.drawer" :title="state.dialogtitle" size="100%">
      <el-card shadow="hover" header="表单表格验证">
        <el-form ref="tableRulesRef" :model="state.tableData" size="default">
          <el-table :data="state.tableData.data" border class="module-table-uncollected">
            <el-table-column
              v-for="(item, index) in state.tableData.header"
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
                    v-for="sel in state.tableData.datatypes"
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

<script lang="ts" setup>
import { reactive, ref } from "vue";
import { v4 as uuidv4, NIL as NIL_UUID } from "uuid";
import { ElMessage } from "element-plus";

import { editProduceDictionary, getProduceDictionary } from "/@/api/produce";
interface TableHeader {
  prop: string;
  width: string | number;
  label: string;
  isRequired?: boolean;
  isTooltip?: boolean;
  type: string;
}
interface TableRulesState {
  deviceid: string;
  drawer: boolean;
  dialogtitle: string;
  tableData: {
    data: dictrowitem[];
    header: TableHeader[];
    datatypes: any[];
  };
}

interface dictrowitem {
  id?: string;
  keyName?: string;
  displayName?: string;
  unit?: string;
  unitExpression?: string;
  unitConvert?: boolean;
  keyDesc?: string;
  defaultValue?: string;
  display?: boolean;
  place0?: string;
  tag?: string;
  dataType?: string;
}

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
        prop: "displayName",
        width: "",
        label: "字段显示名称",
        isRequired: false,
        type: "input",
      },
      {
        prop: "unit",
        width: "",
        label: "单位",
        isRequired: false,
        type: "input",
      },

      {
        prop: "unitExpression",
        width: "",
        label: "单位转换表达式",
        isRequired: false,
        type: "input",
      },
      {
        prop: "unitConvert",
        width: "",
        label: "UnitConvert",
        isRequired: false,
        type: "switch",
      },
      {
        prop: "keyDesc",
        width: "",
        label: "字段备注",
        isRequired: false,
        type: "input",
      },
      {
        prop: "defaultValue",
        width: "",
        label: "默认值",
        isRequired: false,
        type: "input",
      },
      {
        prop: "display",
        width: "",
        label: "是否显示",
        isRequired: false,
        type: "switch",
      },
      {
        prop: "place0",
        width: "",
        label: "位置名称",
        isRequired: false,
        type: "input",
      },
      { prop: "tag", width: "", label: "Tag", isRequired: true, type: "input" },
      {
        prop: "dataType",
        width: "",
        label: "数据类型",
        isRequired: false,
        type: "select",
      },
    ],
    datatypes: [
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
});

const openDialog = (deviceid: string) => {
  state.deviceid = deviceid;
  getProduceDictionary(deviceid).then((x) => {
    state.tableData.data = x.data.map((x) => {
      return {
        id: uuidv4(),
        keyName: x.keyName,
        displayName: x.displayName,
        unit: x.unit,
        unitExpression: x.unitExpression,
        unitConvert: x.unitConvert,
        keyDesc: x.keyDesc,
        defaultValue: x.defaultValue,
        display: x.display,
        place0: x.place0,
        tag: x.tag,
        dataType: x.dataType,
      };
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
    displayName: "",
    unit: "",
    unitExpression: "",
    unitConvert: false,
    keyDesc: "",
    defaultValue: "",
    display: false,
    place0: "",
    tag: "",
    dataType: "",
  });
};
const deleterow = (row: dictrowitem) => {
  state.tableData.data = state.tableData.data.filter((c) => c.id !== row.id);
};
const save = () => {
  editProduceDictionary({
    produceId: state.deviceid,
    produceDictionaryData: state.tableData.data,
  }).then((x) => {
    console.log(x);
  });
};

defineExpose({
  openDialog,
});
</script>
