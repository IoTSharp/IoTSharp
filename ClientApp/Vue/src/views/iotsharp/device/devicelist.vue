<template>
  <div class="p-4">
    <div class="mb-4">
      <a-button class="mr-2" @click="Open"> 新增 </a-button>
      <a-button class="mr-2" @click="reloadTable">刷新 </a-button>
      <a-button class="mr-2" @click="graphtest">X6 Test </a-button>
    </div>

    <BasicTable @register="registerTable" :searchInfo="searchInfo" @expand="handleexpand">
      <template #expandedRowRender="{ record }">
        <BasicTitle helpMessage="属性数据">属性数据</BasicTitle>
        <table style="width: 100%">
          <tbody>
            <tr>
              <td>属性名称</td>
              <td>属性值</td>
              <td>类型</td>
              <td>修改时间</td>
            </tr>
            <tr v-for="(_item, index) in record.Attributes">
              <td>{{ _item.keyName }}</td>
              <td>{{ _item.value }}</td>
              <td>{{ _item.dataSide }}</td>
              <td>{{ _item.dateTime }}</td>
            </tr>
          </tbody>
        </table>
        <BasicTitle helpMessage="遥测数据">遥测数据</BasicTitle>

        <table style="width: 100%">
          <tbody>
            <tr>
              <td>属性名称</td>
              <td>属性值</td>
              <td>修改时间</td>
            </tr>
            <tr v-for="(_item, index) in record.Telemetrys">
              <td>{{ _item.keyName }}</td>
              <td>{{ _item.value }}</td>
              <td>{{ _item.dateTime }}</td>
            </tr>
          </tbody>
        </table>
      </template>
      <template #action="{ record }">
        <TableAction
          :actions="[
            {
              label: '修改',
              icon: 'clarity:note-edit-line',
              onClick: Edit.bind(null, record),
            },
            {
              label: '属性修改',
              icon: 'ant-design:profile-twotone',
              onClick: PropEdit.bind(null, record),
            },
            {
              label: '删除',
              icon: 'ic:outline-delete-outline',
              onClick: Delete.bind(null, record),
            },
          ]"
          :dropDownActions="[]"
        />
      </template>
    </BasicTable>
    <deviceform @register="registerDrawer" @success="handleSuccess" />
    <propform @register="propDrawer" @success="handleSuccess" />
    <devicegraph @register="graphtestDrawer" @success="handleSuccess" />
  </div>
</template>
<script lang="ts">
  import { defineComponent, reactive } from 'vue';
  import { BasicTable, ColumnChangeParam, useTable, TableAction } from '/@/components/Table';
  import { useDrawer } from '/@/components/Drawer';

  import deviceform from './deviceform.vue';
  import propform from './propform.vue';
  import { useRouter } from 'vue-router';
  import {
    getBasicColumns,
    DeviceListApi,
    Get,
    GetAttributeLatest,
    GetTelemetryLatest,
  } from '../../../api/iotsharp/device';
  export default defineComponent({
    components: { BasicTable, TableAction, deviceform, propform },
    setup() {
      const router = useRouter();
      const searchInfo = reactive<Recordable>({});
      searchInfo.customerId = router.currentRoute.value.query.customerid;
      const [propDrawer, { openDrawer: openPropDrawer }] = useDrawer();
      const [registerDrawer, { openDrawer: openDrawer }] = useDrawer();

      function onChange() {
        console.log('onChange', arguments);
      }
      const [registerTable, { setColumns, reload, clearSelectedRowKeys }] = useTable({
        canResize: false,
        title: '设备管理',
        titleHelpMessage: '设备管理',
        api: DeviceListApi,
        columns: getBasicColumns(),
        rowKey: 'id',
        showTableSetting: true,
        showIndexColumn: false,
        expandRowByClick: false,
        onChange,
        rowSelection: {
          type: 'checkbox',
        },
        onColumnsChange: (data: ColumnChangeParam[]) => {
          console.log('ColumnsChanged', data);
        },
        actionColumn: {
          title: 'Action',
          slots: { customRender: 'action' },
        },
      });
      function Delete(record: Recordable): void {
        console.log('点击了删除', record);
      }
      function Open(record: Recordable) {
        openDrawer(true, {
          isUpdate: false,
        });
      }
      function graphtest(): void {
        opengraphtestDrawer(true, {
          // one of you need transfer data
          data: {},
          title: 'dummy title',
        });
      }
      function PropEdit(record: Recordable) {
        GetAttributeLatest({ id: record.id }).then((x) => {
          let b = [{ keyName: 'id', value: record.id }, ...x];
          openPropDrawer(true, {
            item: b,
            isUpdate: false,
          });
        });
      }

      function Edit(record: Recordable) {
        Get(record.id).then((x) => {
          openDrawer(true, {
            x,
            isUpdate: false,
          });
        });
      }

      async function handleexpand(expanded: boolean, item: Recordable) {
        if (expanded) {
          item.Attributes = await GetAttributeLatest({ id: item.id });
          item.Telemetrys = await GetTelemetryLatest({ id: item.id });
        }
      }

      function reloadTable() {
        //    setColumns(getBasicColumns());

        reload({
          page: 1,
        });
      }
      function clearSelect() {
        clearSelectedRowKeys();
      }

      return {
        registerTable,
        reloadTable,
        clearSelect,
        onChange,
        Delete,
        Open,
        Edit,
        PropEdit,
        handleexpand,
        registerDrawer,
    
        graphtest,
        propDrawer,
        openPropDrawer,
  
        searchInfo,
      };
    },
  });
</script>
