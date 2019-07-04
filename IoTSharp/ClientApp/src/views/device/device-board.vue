<template>
  <div>
    <el-row>
      <el-col :xs="{span: 6}" :sm="{span: 6}" :md="{span:6}" :lg="{span: 6}" :xl="{span: 6}" style="padding-left:10px;margin-bottom:30px;">
        <el-card class="box-card" style="margin-top:40px;">
          <div slot="header" class="clearfix">
            <svg-icon icon-class="international" />
            <span style="margin-left:10px;">设备列表</span>
          </div>
          <div>
            <el-table :data="devList" style="width: 100%;hight:50%;padding-top: 15px;" @row-click="openDeviceDetails">
              <el-table-column v-if="false" label="ID">
                <template slot-scope="scope">
                  {{ scope.row.id }}
                </template>
              </el-table-column>
              <el-table-column label="name" width="75">
                <template slot-scope="scope">
                  {{ scope.row.name }}
                </template>
              </el-table-column>
              <el-table-column label="type" width="60" align="center">
                <template slot-scope="scope">
                  {{ scope.row.deviceType | typeFilter }}
                </template>
              </el-table-column>
              <el-table-column label="Status" width="65" align="center">
                <template slot-scope="{row}">
                  <el-tag :type="row.status | statusFilter">
                    {{ row.status }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column label="Action" width="130" align="center">
                <el-button style="margin-left: 10px;" type="primary" icon="el-icon-edit" @click="handleCreate">
                  增加
                </el-button>
              </el-table-column>
            </el-table>
          </div>
        </el-card>
      </el-col>
      <el-col :span="18" style="padding-left:20px;margin-bottom:30px;margin-top:8px">
        <div style="display:inline">
          <div style="float:left">
            <svg-icon icon-class="list" />
            <span style="margin-left:10px;">设备属性:</span>
          </div>
          <div style="float:right;margin-right:10px;">
            <span style="margin-left:5px;"> Access Token:</span>
            <span style="margin-left:10px; color:#F00">{{ curSelectedDeviceToken }}</span>
            <el-button @click="getDevicToken"><svg-icon icon-class="password" /></el-button>
          </div>
        </div>
        <el-table
          :key="tableKey"
          v-loading="listLoading"
          :data="devAttrList"
          border
          fit
          highlight-current-row
          style="width: 100%;"
          @sort-change="sortChange"
        >
          <el-table-column label="数据ID" prop="id" sortable="custom" align="center" min-width="100%" show-overflow-tooltip>
            <template slot-scope="scope">
              <span>{{ scope.row.id }}</span>
            </template>
          </el-table-column>
          <el-table-column label="键名" width="100%" align="center">
            <template slot-scope="scope">
              <span>{{ scope.row.name }}</span>
            </template>
          </el-table-column>
          <el-table-column label="类别" width="100%">
            <template slot-scope="{row}">
              <span class="link-type">{{ row.deviceType }}</span>
              <span>{{ row.catalog | typeFilter }}</span>
            </template>
          </el-table-column>
          <el-table-column label="数据类型">
            <template slot-scope="{row}">
              <span :type="row.type">
                {{ row.type }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="上传时间" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.dateTime">
                {{ row.dateTime }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="布尔值">
            <template slot-scope="{row}">
              <span :type="row.status">
                {{ row.value_Boolean }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="字符">
            <template slot-scope="{row}">
              <span :type="row.status">
                {{ row.value_String }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="长度">
            <template slot-scope="{row}">
              <span :type="row.status">
                {{ row.value_Long }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="Json值" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.value_Json }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="xml值" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.value_XML }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="二进制" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.value_Binary }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="时间属性值" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.dateTime }}
              </span>
            </template>
          </el-table-column>
        </el-table>
      </el-col>
      <el-col :span="18" :offset="6" style="padding-left:20px;margin-bottom:30px;margin-top:8px">
        <div>
          <svg-icon icon-class="list" />
          <span style="margin-left:10px;">设备数据:</span>
        </div>
        <el-table
          :key="tableKey"
          v-loading="listLoading"
          :data="devMeterList"
          border
          fit
          highlight-current-row
          style="width: 100%;"
          @sort-change="sortChange"
        >
          <el-table-column label="遥测数据ID" prop="id" sortable="custom" align="center" min-width="100%" show-overflow-tooltip>
            <template slot-scope="scope">
              <span>{{ scope.row.id }}</span>
            </template>
          </el-table-column>
          <el-table-column label="遥测键值" width="100%" align="center">
            <template slot-scope="scope">
              <span>{{ scope.row.keyName }}</span>
            </template>
          </el-table-column>
          <el-table-column label="遥测数据" width="100%">
            <template slot-scope="{row}">
              <span class="link-type">{{ row.deviceType }}</span>
              <span>{{ row.catalog | typeFilter }}</span>
            </template>
          </el-table-column>
          <el-table-column label="上传时间" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.dateTime">
                {{ row.dateTime }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="布尔值">
            <template slot-scope="{row}">
              <span :type="row.type">
                {{ row.value_Boolean }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="字符串值" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.dateTime">
                {{ row.value_String }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="长整形">
            <template slot-scope="{row}">
              <span :type="row.status">
                {{ row.value_Long }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="双精度值">
            <template slot-scope="{row}">
              <span :type="row.status">
                {{ row.value_Double }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="Json值" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.value_Json }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="xml值" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.value_XML }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="二进制" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.value_Binary }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="时间属性值" show-overflow-tooltip>
            <template slot-scope="{row}">
              <span :type="row.status | statusFilter">
                {{ row.dateTime }}
              </span>
            </template>
          </el-table-column>
          <el-table-column align="center" label="关联">
            <template slot-scope="scope">
              <el-button @click="handleChartConnection(scope)">
                <i class="el-icon-circle-plus-outline" />
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-col>
    </el-row>
    <el-dialog :visible.sync="dialogVisible" :title="dialogType==='edit'?'Edit Chart':'New Chart'">
      <el-form :model="teleMeter" label-width="80px" label-position="left">
        <el-form-item label="id">
          <el-input v-model="teleMeter.id" />
        </el-form-item>
        <el-form-item label="name">
          <el-input v-model="curSelectedDevIdAndName.name" />
        </el-form-item>
        <el-form-item label="keyName">
          <el-input
            v-model="teleMeter.keyName"
            :autosize="{ minRows: 2, maxRows: 4}"
            type="textarea"
            placeholder="Role Description"
          />
        </el-form-item>
        <el-form-item label="图表">
          <el-tree
            ref="tree"
            :check-strictly="checkStrictly"
            :data="avialableChatList"
            show-checkbox
            node-key="id"
            class="permission-tree"
          />
        </el-form-item>
      </el-form>
      <div style="text-align:right;">
        <el-button type="danger" @click="dialogVisible=false">Cancel</el-button>
        <el-button type="primary" @click="confirmEditAndNofityVisionBoard">Confirm</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
// import Pagination from '@/components/Pagination' // secondary package based on el-pagination
import { getDevices, postDevice, getDeviceAccessToken, getDeviceAttributes, getDeviceTelemetryLatest } from '@/api/device'
import store from '../../store'
import { deepClone } from '@/utils'
import { bus } from './utils.js'

// const DEV_COMPONENT_STORAGE_KEY = 'devcomponents'
const DEV_CHART_BINDING_INFO = 'datachartbinginfo'
const calendarTypeOptions = [
  { key: '0', display_name: '设备' },
  { key: '1', display_name: '网关' }
]
const calendarTypeKeyValue = calendarTypeOptions.reduce((acc, cur) => {
  acc[cur.key] = cur.display_name
  return acc
}, {})

const defaulTeleMeter = {
  id: '',
  keyName: '',
  catalog: '',
  dateTime: '',
  value_Boolean: '',
  value_String: '',
  value_Long: '',
  value_Double: '',
  value_Json: '',
  value_XML: '',
  value_value_Binary: ''
}
const defaulSelectionDevIdAndName = {
  id: '',
  name: ''
}
/*
const avialableChatList = {
  id: '',
  name: '',
  type: '',
  from: '', // system or customer custermized
}
*/

const devChartBindingDataSendDefault = {
  id: '',
  deviceId: '',
  devName: '',
  devBindedAttrId: '',
  devBindedAttr: '',
  devBingedChart: ''
}

export default {
  name: 'DeviceBoard',
  filters: {
    statusFilter(status) {
      const statusMap = {
        Online: 'success',
        Offline: 'danger',
        Created: 'info'
      }
      return statusMap[status]
    },
    typeFilter(type) {
      return calendarTypeKeyValue[type]
    }
  },
  data() {
    return {
      tableKey: 0,
      curSelectedDeviceToken: 0,
      curSelectedDevIdAndName: Object.assign({}, defaulSelectionDevIdAndName),
      devList: null,
      devListTotal: 0,
      devAttrList: null,
      devAttrListTotal: 0,
      devMeterList: null,
      devMeterListTotal: 0,
      listLoading: true,
      teleMeter: Object.assign({}, defaulTeleMeter),
      routes: [], // for edit devMeter to visual component
      avialableChatList: [], // 获取所有的图表控件
      devChartBindingDataSend: Object.assign({}, devChartBindingDataSendDefault),
      dialogVisible: false,
      dialogType: 'new',
      checkStrictly: false, // end
      devlistQuery: {
        page: 1,
        limit: 10,
        importance: undefined,
        name: undefined,
        type: undefined,
        sort: '+id'
      },
      devAttrListQuery: {
        page: 1,
        limit: 10,
        importance: undefined,
        name: undefined,
        type: undefined,
        sort: '+id'
      },
      calendarTypeOptions,
      sortOptions: [{ label: '升序', key: '+id' }, { label: '降序', key: '-id' }],
      statusOptions: ['Online', 'Offline', 'Created'],
      temp: {
        deviceName: undefined,
        deviceType: ''
      },
      dialogFormVisible: false,
      dialogStatus: '',
      textMap: {
        update: 'Edit',
        create: 'Create'
      },
      dialogPvVisible: false,
      pvData: [],
      rules: {
        name: [{ required: true, message: '名称必须填写', trigger: 'change' }],
        type: [{ required: true, message: '请选择类型', trigger: 'blur' }]
      },
      downloadLoading: false
    }
  },
  created() {
    this.getDevList()
    window.localStorage.removeItem(DEV_CHART_BINDING_INFO)
  },
  methods: {
    getDevList() {
      this.listLoading = true
      console.log('getdevicelist start, cid :')
      console.log(store.getters.customerid)
      getDevices(store.getters.customerid).then(response => {
        console.log('deviceList:')
        console.log(response)
        this.devList = response
        this.devListTotal = response.length
        // this.total = response.data.length
        console.log('list:')
        console.log(this.list)
        console.log(this.total)
        // Just to simulate the time of the request
        setTimeout(() => {
          this.listLoading = false
        }, 1.5 * 1000)
      })
    },

    getDevicToken() {
      if (this.curSelectedDevIdAndName.id === '') {
        this.$message({
          message: '请选择一个设备！',
          type: 'error'
        })
        return
      }
      return new Promise((resolve, reject) => {
        getDeviceAccessToken(this.curSelectedDevIdAndName.id).then(response => {
          console.log('dev access token response:')
          console.log(response)
          this.curSelectedDeviceToken = response.id
        }).catch(() => {
          this.$message({
            message: '获取 DeviceId 失败',
            type: 'error'
          })
        }).catch(err => {
          console.log(err)
          reject(false)
        })
      })
    },
    openDeviceDetails(row) {
      console.log('Start get token:')
      console.log(row.id)
      console.log('start get attr, passed value :')
      console.log(row.id)
      this.curSelectedDevIdAndName.id = row.id
      this.curSelectedDevIdAndName.name = row.name
      getDeviceAttributes(row.id).then(response => {
        console.log('start get attr response:')
        console.log(response)
        this.devAttrList = response
      }).catch(() => {
        this.$message({
          message: '获取Device Attr失败',
          type: 'error'
        })
      })
      getDeviceTelemetryLatest(row.id).then(response => {
        console.log('start get devMeterData response:')
        console.log(response)
        this.devMeterList = response
      }).catch(() => {
        this.$message({
          message: '获取Device Telemeter失败',
          type: 'error'
        })
      })
    },
    handleEdit(scope) {
      console.log(scope)
    },
    handleChartConnection(scope) {
      this.dialogType = 'New'
      this.dialogVisible = true
      this.checkStrictly = true
      this.teleMeter = deepClone(scope.row)
      this.devChartBindingDataSend.devBindedAttrId = scope.row.id
      console.log('start handle Chart, row info:')
      console.log(this.teleMeter)
      console.log('devcomponents info:')
      console.log(window.localStorage.getItem('devcomponents'))

      // avialableChatList = JSON.parse(window.localStorage.getItem('devcomponents'))
      // console.log('avialableChatList:')
      var chart = JSON.parse(window.localStorage.getItem('devcomponents')) // or you can get from sql db
      this.avialableChatList = []
      for (let i = 0; i < chart.length; i++) {
        var tempChartInfo = { id: chart[i].id, label: chart[i].vChartType, from: chart[i].from }
        console.log(tempChartInfo)
        this.avialableChatList.push(tempChartInfo)
      }
    },
    confirmEditAndNofityVisionBoard() {
      console.log('edit emit')
      // const checkedKeys = this.$refs.tree.getCheckedKeys()
      const checkednodes = this.$refs.tree.getCheckedNodes() // only one
      console.log(checkednodes)
      if (checkednodes.length === 0) {
        this.$message({
          message: '操作失败，请选择图表类型',
          type: 'failure'
        })
        return
      }
      this.devChartBindingDataSend.deviceId = this.curSelectedDevIdAndName.id
      this.devChartBindingDataSend.name = this.curSelectedDevIdAndName.name
      this.devChartBindingDataSend.devName = this.curSelectedDevIdAndName.name
      this.devChartBindingDataSend.devBingedChart = checkednodes[0].label
      console.log(checkednodes[0].label)
      console.log('devChartBindingDataSend')
      console.log(this.devChartBindingDataSend)
      bus.$emit('devbindingdata', this.devChartBindingDataSend)
      window.localStorage.setItem(DEV_CHART_BINDING_INFO, JSON.stringify(this.devChartBindingDataSend))
      this.$notify({
        title: 'Success',
        dangerouslyUseHTMLString: true,
        message: `
            <div>deviceName: ${this.devChartBindingDataSend.name}</div>
            <div>deviceMeter: ${this.devChartBindingDataSend.devBindedAttrId}</div>
            <div>devChart: ${this.devChartBindingDataSend.devBingedChart}</div>
          `,
        type: 'success'
      })
      this.dialogVisible = false
    },
    handleFilter() {
      this.listQuery.page = 1
      this.getDevList()
    },
    handleDeviceDelete(row) {
      this.$message({
        message: '操作Success',
        type: 'success'
      })
    },
    handleDeviceRpc(row) {
      this.$message({
        message: '操作Success',
        type: 'success'
      })
      // update row attribute here
    },
    resetTemp() {
      this.temp = {
        deviceName: undefined,
        deviceType: ''
      }
    },
    handleCreate() {
      this.resetTemp()
      this.dialogStatus = 'create'
      this.dialogFormVisible = true
      this.$nextTick(() => {
        this.$refs['dataForm'].clearValidate()
      })
    },
    sortChange(data) {
      const { prop, order } = data
      if (prop === 'id') {
        this.sortByID(order)
      }
    },
    sortByID(order) {
      if (order === 'ascending') {
        this.listQuery.sort = '+id'
      } else {
        this.listQuery.sort = '-id'
      }
      this.handleFilter()
    },
    createData() {
      this.$refs['dataForm'].validate((valid) => {
        if (valid) {
          const { deviceName, deviceType } = this.temp
          console.log(deviceName)
          console.log(deviceType)
          postDevice({ name: deviceName.trim(), deviceType: deviceType }).then(() => {
            this.list.unshift(this.temp)
            this.dialogFormVisible = false
            this.$notify({
              title: 'Success',
              message: 'Created Successfully',
              type: 'success',
              duration: 2000
            })
          })
        }
      })
    }
  }
}
</script>

<style scoped>
.box-card {
  max-width: 100%;
  margin: 20px auto;
}
.item-btn{
  margin-bottom: 15px;
  margin-left: 0px;
}
.block {
  padding: 25px;
}
</style>
