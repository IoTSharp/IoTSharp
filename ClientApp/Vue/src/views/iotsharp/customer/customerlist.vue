<template>
  <div class="p-4">
    <div class="mb-4">
      <a-button class="mr-2" @click="Open"> 新增 </a-button>
      <a-button class="mr-2" @click="reloadTable"> 刷新 </a-button>
    </div>

    <BasicTable @register="registerTable" :searchInfo="searchInfo">
      <template #action="{ record }">
        <TableAction
          :actions="[
            {
              label: '设备管理',
              icon: 'ant-design:group-outlined',
              onClick: DeviceManage.bind(null, record),
            },

            {
              label: '人员管理',
              icon: 'ant-design:user-delete-outlined',
              onClick: UserManage.bind(null, record),
            },
            {
              label: '修改',
              icon: 'clarity:note-edit-line',
              onClick: Edit.bind(null, record),
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
    <tenantform @register="registerDrawer" @success="handleSuccess" />
  </div>
</template>
<script lang="ts">
  import { defineComponent, reactive } from 'vue';
  import { BasicTable, ColumnChangeParam, useTable, TableAction } from '/@/components/Table';
  import { useDrawer } from '/@/components/Drawer';
  import tenantform from './customerform.vue';
  import { useRouter } from 'vue-router';
  import { getBasicColumns, CustomerListApi, Get } from '../../../api/iotsharp/customer';
  export default defineComponent({
    components: { BasicTable, TableAction, tenantform },
    setup() {
      const router = useRouter();
      const searchInfo = reactive<Recordable>({});
      searchInfo.tenantid = router.currentRoute.value.query.tenantid;
      const [registerDrawer, { openDrawer }] = useDrawer();
      function onChange() {
        console.log('onChange', arguments);
      }
      const [registerTable, { setColumns, reload, clearSelectedRowKeys }] = useTable({
        canResize: false,
        title: '客户管理',
        titleHelpMessage: '客户管理',
        api: CustomerListApi,
        columns: getBasicColumns(),
        rowKey: 'id',
        showTableSetting: true,
        showIndexColumn: false,
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
      function Delete(record: Recordable): void {}

      function handleSuccess() {
        reload();
      }

      function Open(record: Recordable) {
        openDrawer(true, {
          item: { tenantID: searchInfo.tenantid },
          isUpdate: false,
        });
      }
      function Edit(record: Recordable) {
        Get(record.id).then((item) => {
          openDrawer(true, {
            item,
            isUpdate: true,
          });
        });
      }
      function UserManage(record: Recordable) {
        router.push('user?customerid=' + record.id);
      }
      function DeviceManage(record: Recordable) {
        router.push('device?customerid=' + record.id);
      }
      function reloadTable() {
        setColumns(getBasicColumns());

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
        UserManage,
        DeviceManage,
        registerDrawer,
        handleSuccess,
        searchInfo,
      };
    },
  });
</script>
