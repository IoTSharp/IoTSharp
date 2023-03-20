<template>
  <div class="workflow-drawer-node">
    <el-tabs type="border-card" v-model="state.tabsActive">
      <!-- 扩展表单 -->
      <el-tab-pane label="配置" name="1">
        <el-scrollbar>
          <monaco
            :height="state.configheight"
            :width="state.configwidth"
            theme="vs-dark"
            v-model="state.node.content"
            language="json"
            selectOnLineNumbers="true"
          ></monaco>
            <div style="clear:both;"></div>
            <div class="codeTip">
                <div>当前执行器为：<span style="font-weight:bold;">{{state.node.name}}</span>，对应处理器：<span style="font-weight:bold;">{{state.node.mata}}</span></div>
                <div>配置信息说明（使用上级传出的数据(上级可以使用："用于自定义的告警推送的执行器")，按照以下配置推送）：</div>
                <div v-if="state.node.mata=='IoTSharp.TaskActions.AlarmPullExcutor'">
<pre>
{
   "BaseUrl":"http://xxxxx:xxxxxx/",
   "Url":"/xxxxx/xxxxx",
   "Token":"xxxxxxxxxxx"
}
</pre>
                    <p>如：</p>
<pre>
{
   "BaseUrl":"http://www.baidu.com/",
   "Url":"/Devices/GetDevice", //后端会在此基础上拼接 /DeviceId
   "Token":"e10be78ae02f47b09f8fa2212000d153"
}
</pre>
                </div>
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.PublishAlarmDataTask'">
                    <p>上级节点返回的对象如下</p>
<pre>
if(input.temperature>38){
    output.CreateDateTime="2023-02-20 16:14:00",//可以不传
    output.OriginatorType="Device" //Device或Gateway
    output.OriginatorName="Modbus" //可以为设备名称或者设备Id
    output.AlarmType="发烧"
    output.AlarmDetail="体温超过38度，请注意"
    output.ServerityLevel="Warning" //参考枚举：ServerityLevel
    return output;
}
</pre>
                    <!--output.warnDataId="D9F7DB7A-OB6B-4443-9172-FB58A0 /-->
                    <p>也就是说该节点接收的对象为:</p>
<pre>
{
   "CreateDateTime":"2023-02-20 16:14:00",//可以不传
   "OriginatorType":"Device" //Device或Gateway
   "OriginatorName":"Modbus" //可以为设备名称或者设备Id
   "AlarmType":"发烧" //告警类型
   "AlarmDetail":"体温超过38度，请注意"
   "ServerityLevel":"Warning" //参考枚举：ServerityLevel
}
</pre>
                </div>
                <!--属性发布器-->
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.PublishAttributeDataTask'">
                    <p>发布设备属性信息，数据侧为：客户侧属性</p>
<pre>
[
    {
       "key":"speed",//属性名
       "value":"1000" //属性值
    },{
       "key":"rate",//属性名
       "value":"10.2" //属性值
    }
]
</pre>
                </div>
                <!--遥测数据发布器-->
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.PublishTelemetryDataTask'">
                    <p>发布设备遥测数据，数据侧为：客户侧属性</p>
<pre>
[
    {
       "key":"speed",//属性名
       "value":"1000" //属性值
    },{
       "key":"rate",//属性名
       "value":"10.2" //属性值
    }
]
</pre>
                </div>
                <!--用于自定义的告警推送的执行器-->
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.CustomeAlarmPullExcutor'">
                    <p>将以下信息进行后台组合，产生"告警数据发布器"所能使用的数据，一般作为"告警数据发布器"的上级</p>
                    <pre>
{
   "Serverity":4,
   "AlarmType":"xxxxx",
   "AlarmDetail":"xxxxxxxxxxx"
}
</pre>
                    <p>如：</p>
                    <pre>
{
   "Serverity":7,
   "AlarmType":"发烧",
   "AlarmDetail":"体温超过38度，请注意"
}
</pre>
                    <p>此处特殊说明：Serverity必须为Int数值</p>
                </div>
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.DeviceActionExcutor'">
                    <p>1、该执行器使用上级数据按照下面的配置信息进行post推送，并返回执行结果供下一节点使用</p>
                    <pre>
{
   "BaseUrl":"http://xxxxx:xxxxxx/",
   "Url":"/xxxxx/xxxxx",
   "Token":"xxxxxxxxxxx"
}
</pre>
                    <p>如：</p>
<pre>
{
   "BaseUrl":"http://www.baidu.com/",
   "Url":"/Devices/GetDevice", //后端会在此基础上拼接 /DeviceId
   "Token":"e10be78ae02f47b09f8fa2212000d153"
}
</pre>
                    <p style="margin:4px 0px;"><el-tag type="success">传出</el-tag>向以上地址传参为：</p>
                    <pre>
{
   "sosType":"1",
   "sosContent":"{\"name\":\"张三\",\"age\":\"17\"}"//上级传参
}
</pre>
                    <p>2、上述地址需要按照以下内容格式返回（其中success作为判断依据）：</p>
                    <pre>
{
   "success":true,
   "message":"处理成功",
   "timestamp":"1202121545",
   "result":"处理成功",
   "code":"200"
}
</pre>
                    <p style="margin:4px 0px;">3.1、<el-tag type="warning">传入</el-tag>如果上述success为true，则该节点返回以下格式内容供下一节点使用：</p>
                    <pre>
{
   "ExecutionInfo":"{\"success\":true,\"message\":\"处理成功\"
                    ,\"timestamp\":\"1202121545\",\"result\":\"处理成功\"
                    ,\"code\":\"200\"}",
   "ExecutionStatus":true,
   "DynamicOutput":"{\"name\":\"张三\",\"age\":\"17\"}",
}
</pre>
                    <p>3.2、如果上述success为false，则该节点返回以下格式内容：</p>
                    <pre>
{
   "ExecutionInfo":"{\"success\":false,\"message\":\"处理出现异常，格式不正确\"
                    ,\"timestamp\":\"1202121545\",\"result\":\"处理失败\"
                    ,\"code\":\"500\"}",
   "ExecutionStatus":false,
}
</pre>
                </div>
                <!--用于消息推送的执行器-->
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.MessagePullExecutor'">
                    <p>1、该执行器使用上级数据按照下面的配置信息进行post推送，并返回执行结果供下一节点使用</p>
                    <pre>
{
   "BaseUrl":"http://xxxxx:xxxxxx/",
   "Url":"/xxxxx/xxxxx",
   "Token":"xxxxxxxxxxx"
}
</pre>
                    <p>如：</p>
<pre>
{
   "BaseUrl":"http://www.baidu.com/",
   "Url":"/Devices/GetDevice", //后端会在此基础上拼接 /DeviceId
   "Token":"e10be78ae02f47b09f8fa2212000d153"
}
</pre>
<p>举例：如果流程图中上一节点传入参数为（格式为举例，其他json对象都可以）：</p>
                    <pre>
{
	"UserName": "张三",
	"DeviceId": "123GE3423455",
	"Age": "18",
	"Attr": {
		"Spped": 100,
		"State": "stop",
		"Msg": "运行停止",
		"Time": "20230317"
	}
}
</pre>
                    <p style="margin:4px 0px;"><el-tag type="success">传出</el-tag>后端将对以上Json对象进行转换，变为以下形式后对配置的地址传参，格式为：</p>
                    <pre>
[{
	"keyName": "username",
	"value": "张三"
}, {
	"keyName": "deviceid",
	"value": "123GE3423455"
}, {
	"keyName": "age",
	"value": "18"
}, {
	"keyName": "attr",
	"value": {
		"Spped": 100,
		"State": "stop",
		"Msg": "运行停止",
		"Time": "20230317"
	}
}]
</pre>
                    <p>2、上述地址需要按照以下内容格式返回（其中success作为判断依据）：</p>
                    <pre>
{
   "success":true,
   "message":"处理成功",
   "timestamp":"1202121545",
   "result":"处理成功",
   "code":"200"
}
</pre>
                    <p style="margin:4px 0px;">3.1、<el-tag type="warning">传入</el-tag>如果上述success为true，则该节点返回以下格式内容供下一节点使用：</p>
                    <pre>
{
   "ExecutionInfo":"{\"success\":true,\"message\":\"处理成功\"
                    ,\"timestamp\":\"1202121545\",\"result\":\"处理成功\"
                    ,\"code\":\"200\"}",
   "ExecutionStatus":true,
   "DynamicOutput":"{\"UserName\":\"张三\",\"DeviceId\":\"123GE3423455\",\"Age\":\"18\"
                    ,\"Attr\":{\"Spped\":100,\"State\":\"stop\",\"Msg\":\"运行停止\"
                    ,\"Time\":\"20230317\"}}",
}
</pre>
                    <p>3.2、如果上述success为false，则该节点返回以下格式内容：</p>
                    <pre>
{
   "ExecutionInfo":"{\"success\":false,\"message\":\"处理出现异常，格式不正确\"
                    ,\"timestamp\":\"1202121545\",\"result\":\"处理失败\"
                    ,\"code\":\"500\"}",
   "ExecutionStatus":false,
}
</pre>

                </div>
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.RangerCheckExcutor'">
                    待补充，用于增加范围属性的执行器

                </div>
                <div v-else-if="state.node.mata=='IoTSharp.TaskActions.TelemetryArrayPullExcutor'">
                    待补充，用于遥测数组推送的执行器


                </div>

                <!--mata："IoTSharp.TaskActions.AlarmPullExcutor"
      nodenamespace："bpmn:Task"
      nodetype:"executor"
    name:"用于告警推送的执行器"-->
            </div>
        </el-scrollbar>
      </el-tab-pane>
      <!-- 节点编辑 -->
      <el-tab-pane label="杂项" name="2">
        <el-scrollbar>
          <el-form
            :model="state.node"
            :rules="state.nodeRules"
            ref="nodeFormRef"
            size="default"
            label-width="80px"
            class="pt15 pr15 pb15 pl15"
          >
            <el-form-item label="数据id" prop="id">
              <el-input
                v-model="state.node.id"
                placeholder="请输入数据id"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="节点id" prop="nodeId">
              <el-input
                v-model="state.node.nodeId"
                placeholder="请输入节点id"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="类型" prop="type">
              <el-input
                v-model="state.node.type"
                placeholder="请输入类型"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="left坐标" prop="left">
              <el-input
                v-model="state.node.left"
                placeholder="请输入left坐标"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="top坐标" prop="top">
              <el-input
                v-model="state.node.top"
                placeholder="请输入top坐标"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="icon图标" prop="icon">
              <el-input
                v-model="state.node.icon"
                placeholder="请输入icon图标"
                clearable
              ></el-input>
            </el-form-item>
            <el-form-item label="名称" prop="name">
              <el-input
                v-model="state.node.name"
                placeholder="请输入名称"
                clearable
              ></el-input>
            </el-form-item>
            <el-form-item>
              <el-button class="mb15" @click="onNodeRefresh">
                <SvgIcon name="ele-RefreshRight" />
                重置
              </el-button>
              <el-button type="primary" class="mb15" @click="onNodeSubmit">
                <SvgIcon name="ele-Check" />
                保存
              </el-button>
            </el-form-item>
          </el-form>
        </el-scrollbar>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script lang="ts" setup>
import { reactive, toRefs, ref, nextTick, getCurrentInstance, Ref } from "vue";
import {
  ElButton,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElScrollbar,
  ElTabPane,
  ElTabs,
} from "element-plus";
import monaco from "/@/components/monaco/monaco.vue";
import node from "element-plus/es/components/cascader-panel/src/node";
// 定义接口来定义对象的类型
interface WorkflowDrawerNodeState {
  configwidth: string;
  configheight: string;
  node: nodedata;
  nodeRules: any;
  form: any;
  tabsActive: string;
  loading: {
    extend: boolean;
  };
}

interface nodedata {
  id?: string;
  contextMenuClickId?: number;
  content?: string;
  from?: string;
  icon?: string;
  label?: string;
  left?: string;
  mata?: string;
  name?: string;
  nodeId?: string;
  nodeclass?: string;
  nodenamespace?: string;
  nodetype?: string;
  result?: string;
  top?: string;
  type?: string;
}

const props = defineProps({
  modelValue: {
    type: Object,
    default: {},
  },
});

const emit = defineEmits(["close", "submit"]);
const { proxy } = <any>getCurrentInstance();
const nodeFormRef = ref();
const extendFormRef = ref();
const chartsMonitorRef = ref();
const state = reactive<WorkflowDrawerNodeState>({
  configwidth: "100%",
  configheight: "100%",
  node:props.modelValue,
  nodeRules: {
    id: [{ required: true, message: "请输入数据id", trigger: "blur" }],
    nodeId: [{ required: true, message: "请输入节点id", trigger: "blur" }],
    type: [{ required: true, message: "请输入类型", trigger: "blur" }],
    left: [{ required: true, message: "请输入left坐标", trigger: "blur" }],
    top: [{ required: true, message: "请输入top坐标", trigger: "blur" }],
    icon: [{ required: true, message: "请输入icon图标", trigger: "blur" }],
    name: [{ required: true, message: "请输入名称", trigger: "blur" }],
  },
  form: {
    module: [],
  },
  tabsActive: "1",
  loading: {
    extend: false,
  },
});
// 获取父组件数据
// const getParentData = (data: object) => {
//   state.tabsActive = "1";
//   state.node = data;
// };

onMounted(() => {});
// 节点编辑-重置
const onNodeRefresh = () => {
  state.node.icon = "";
  state.node.name = "";
};
// 节点编辑-保存
const onNodeSubmit = () => {
  nodeFormRef.value.validate((valid: boolean) => {
    if (valid) {
      emit("submit", state.node);
      emit("close");
    } else {
      return false;
    }
  });
};
watch(
  () => props.modelValue,
  () => {
    console.log(state)
    state.node = props.modelValue
  }
);


defineExpose({
  // getParentData,
});
// 图表可视化-初始化
</script>

<style scoped lang="scss">
.workflow-drawer-node {
  :deep {
    .el-tabs {
      box-shadow: unset;
      border: unset;

      .el-tabs__nav {
        display: flex;
        width: 100%;

        .el-tabs__item {
          flex: 1;
          padding: unset;
          text-align: center;

          &:first-of-type.is-active {
            border-left-color: transparent;
          }

          &:last-of-type.is-active {
            border-right-color: transparent;
          }
        }
      }

      .el-tabs__content {
        padding: 0;
        height: calc(100vh - 90px);

        .el-tab-pane {
          height: 100%;
          .codeTip{
            padding:5px 15px;
            border: 1px solid #eaeaea;
            color: #4a4949;
            margin:8px;
            font-size: 12px;
            clear:both;
            pre{
                background: #1e1e1e;
                color: #c6c6c6;
                padding: 4px;
            }
          }
        }
      }
    }
  }
}
</style>
