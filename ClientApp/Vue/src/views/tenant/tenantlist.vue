<template>
  <div>
    <div class="table-page-search-wrapper">
      <a-form layout="inline">
        <a-row :gutter="48">
          <a-col :md="8" :sm="24">
            <a-form-item label="租户名称">
              <a-input v-model="queryParam.name" placeholder="" />
            </a-form-item>
          </a-col>
          <a-col :md="(!advanced && 8) || 24" :sm="24">
            <span
              class="table-page-search-submitButtons"
              :style="(advanced && { float: 'right', overflow: 'hidden' }) || {}"
            >
              <a-button type="primary" @click="$refs.table.refresh(true)">查询</a-button>
              <a-button style="margin-left: 8px" @click="() => (queryParam = {})">重置</a-button>
            </span>
          </a-col>
        </a-row>
      </a-form>
    </div>

    <div class="table-operator">
      <a-button type="primary" icon="plus" @click="handleEdit(null)">新建</a-button>
      <a-button type="dashed" @click="tableOption">{{ (optionAlertShow && '关闭') || '开启' }} alert</a-button>
      <a-dropdown v-action:edit v-if="selectedRowKeys.length > 0">
        <a-menu slot="overlay">
          <a-menu-item key="1"><a-icon type="delete" />删除</a-menu-item>
          <!-- lock | unlock -->
          <a-menu-item key="2"><a-icon type="lock" />锁定</a-menu-item>
        </a-menu>
        <a-button style="margin-left: 8px"> 批量操作 <a-icon type="down" /> </a-button>
      </a-dropdown>
    </div>

    <s-table
      ref="table"
      size="default"
      rowKey="key"
      :columns="columns"
      :data="loadData"
      :alert="options.alert"
      :rowSelection="options.rowSelection"
    >
      <span slot="serial" slot-scope="text, record, index">
        {{ index + 1 }}
      </span>
      <span slot="action" slot-scope="text, record">
        <template>
          <a @click="handleEdit(record)">编辑</a>
          <a-divider type="vertical" />
        </template>
        <a-dropdown>
          <a class="ant-dropdown-link"> 更多 <a-icon type="down" /> </a>
          <a-menu slot="overlay">
            <a-menu-item>
              <a href="javascript:;">详情</a>
            </a-menu-item>
            <a-menu-item v-if="$auth('table.disable')">
              <a href="javascript:;">禁用</a>
            </a-menu-item>
            <a-menu-item v-if="$auth('table.delete')">
              <a href="javascript:;">删除</a>
            </a-menu-item>
          </a-menu>
        </a-dropdown>
      </span>
    </s-table>
    <a-drawer :title="title" :width="720" :visible="visible" :body-style="{ paddingBottom: '80px' }" @close="onClose">
      <a-form :form="form" layout="vertical" hide-required-mark>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="Name">
              <a-input
                v-decorator="[
                  'name',
                  {
                    rules: [{ required: true, message: 'Please enter user name' }],
                  },
                ]"
                placeholder="Please enter user name"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="Url">
              <a-input
                v-decorator="[
                  'url',
                  {
                    rules: [{ required: true, message: 'please enter url' }],
                  },
                ]"
                style="width: 100%"
                addon-before="http://"
                addon-after=".com"
                placeholder="please enter url"
              />
            </a-form-item>
          </a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="Owner">
              <a-select
                v-decorator="[
                  'owner',
                  {
                    rules: [{ required: true, message: 'Please select an owner' }],
                  },
                ]"
                placeholder="Please a-s an owner"
              >
                <a-select-option value="xiao"> Xiaoxiao Fu </a-select-option>
                <a-select-option value="mao"> Maomao Zhou </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="Type">
              <a-select
                v-decorator="[
                  'type',
                  {
                    rules: [{ required: true, message: 'Please choose the type' }],
                  },
                ]"
                placeholder="Please choose the type"
              >
                <a-select-option value="private"> Private </a-select-option>
                <a-select-option value="public"> Public </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="Approver">
              <a-select
                v-decorator="[
                  'approver',
                  {
                    rules: [{ required: true, message: 'Please choose the approver' }],
                  },
                ]"
                placeholder="Please choose the approver"
              >
                <a-select-option value="jack"> Jack Ma </a-select-option>
                <a-select-option value="tom"> Tom Liu </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="DateTime">
              <a-date-picker
                v-decorator="[
                  'dateTime',
                  {
                    rules: [{ required: true, message: 'Please choose the dateTime' }],
                  },
                ]"
                style="width: 100%"
                :get-popup-container="(trigger) => trigger.parentNode"
              />
            </a-form-item>
          </a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :span="24">
            <a-form-item label="Description">
              <a-textarea
                v-decorator="[
                  'description',
                  {
                    rules: [{ required: true, message: 'Please enter url description' }],
                  },
                ]"
                :rows="4"
                placeholder="please enter url description"
              />
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
      <div
        :style="{
          position: 'absolute',
          right: 0,
          bottom: 0,
          width: '100%',
          borderTop: '1px solid #e9e9e9',
          padding: '10px 16px',
          background: '#fff',
          textAlign: 'right',
          zIndex: 1,
        }"
      >
        <a-button :style="{ marginRight: '8px' }" @click="onClose"> Cancel </a-button>
        <a-button type="primary" @click="onClose"> Submit </a-button>
      </div>
    </a-drawer>
  </div>
</template>

<script>
import moment from 'moment'
import { STable } from '@/components'
import { getTenantList, getTenant } from '@/api/manage'

export default {
  name: 'TableList',
  components: {
    STable
  },
  data () {
    return {
      form: this.$form.createForm(this),
      title: '',
      visible: false,
      mdl: {},
      // 高级搜索 展开/关闭
      advanced: false,
      // 查询参数
      queryParam: {},
      pagination: {},
      // 表头
      columns: [
        {
          title: '#',
          scopedSlots: { customRender: 'serial' }
        },
        {
          title: '租户Id',
          dataIndex: 'id'
        },
        {
          title: '名称',
          dataIndex: 'name'
        },
        {
          title: '邮箱',
          dataIndex: 'eMail',
          sorter: true,
          needTotal: true,
          customRender: (text) => text
        },
        {
          title: '状态',
          dataIndex: 'phone',
          needTotal: true
        },
        {
          title: '省',
          dataIndex: 'province',
          sorter: true
        },
        {
          title: '操作',
          dataIndex: 'action',
          width: '150px',
          scopedSlots: { customRender: 'action' }
        }
      ],
      // 加载数据方法 必须为 Promise 对象
      loadData: (parameter) => {
        return getTenantList(Object.assign(parameter, this.queryParam)).then((res) => {
          return {
            data: res,
            pageSize: 10,
            pageNo: 0,
            totalPage: 1,
            totalCount: 10
          }
        })
      },
      selectedRowKeys: [],
      selectedRows: [],

      // custom table alert & rowSelection
      options: {
        alert: {
          show: true,
          clear: () => {
            this.selectedRowKeys = []
          }
        },
        rowSelection: {
          selectedRowKeys: this.selectedRowKeys,
          onChange: this.onSelectChange
        }
      },
      optionAlertShow: false
    }
  },
  created () {
    this.tableOption()
  },
  methods: {
    tableOption () {
      if (!this.optionAlertShow) {
        this.options = {
          alert: {
            show: true,
            clear: () => {
              this.selectedRowKeys = []
            }
          },
          rowSelection: {
            selectedRowKeys: this.selectedRowKeys,
            onChange: this.onSelectChange
          }
        }
        this.optionAlertShow = true
      } else {
        this.options = {
          alert: false,
          rowSelection: null
        }
        this.optionAlertShow = false
      }
    },

    handleEdit (record) {
      this.visible = true
      if (record) {
        getTenant(record.id).then((x) => {})
      } else {
      }
    },
    onClose () {},
    handleOk () {},

    onSelectChange (selectedRowKeys, selectedRows) {
      this.selectedRowKeys = selectedRowKeys
      this.selectedRows = selectedRows
    },
    toggleAdvanced () {
      this.advanced = !this.advanced
    },

    resetSearchForm () {
      this.queryParam = {
        date: moment(new Date())
      }
    }
  }
}
</script>
