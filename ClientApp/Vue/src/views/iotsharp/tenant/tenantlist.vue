<template>
  <div class="p-4">
    <div class="mb-4">
      <a-button class="mr-2" @click="Open"> 新增 </a-button>
      <a-button class="mr-2" @click="reloadTable"> 刷新 </a-button>
    </div>

    <BasicTable @register="registerTable">
      <template #action="{ record }">
        <TableAction
          :actions="[
            {
              label: '客户管理',
              icon: 'ant-design:gold-outlined',
              onClick: CustomerManage.bind(null, record),
            },

            {
              label: '人员管理',
              icon: ' ant-design:user-delete-outlined',
              onClick: UserManage.bind(null, record),
            },
            {
              label: '修改',
              icon: 'ic:outline-delete-outline',
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
  import { defineComponent } from 'vue';
  import { BasicTable, ColumnChangeParam, useTable, TableAction } from '/@/components/Table';
  import { useDrawer } from '/@/components/Drawer';
  import tenantform from './tenantform.vue';
  import { useRouter } from 'vue-router';
  import { getBasicColumns, TenantListApi, Get } from '../../../api/iotsharp/tenant';
  export default defineComponent({
    components: { BasicTable, TableAction, tenantform },
    setup() {
      const router = useRouter();
      const [registerDrawer, { openDrawer }] = useDrawer();
      function onChange() {
        console.log('onChange', arguments);
      }
      const [registerTable, { setColumns, reload, clearSelectedRowKeys }] = useTable({
        canResize: false,
        title: '租户管理',
        titleHelpMessage: '租户管理',
        api: TenantListApi,
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
        router.push('user?tenantid='+record.id);
      }
      function CustomerManage(record: Recordable) {
        router.push('customer?tenantid='+record.id);
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
        CustomerManage,
        registerDrawer,
        handleSuccess,
      };
    },
  });
</script>
