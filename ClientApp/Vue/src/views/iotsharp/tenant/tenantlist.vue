<template>
  <div class="p-4">
    <div class="mb-4">
      <a-button class="mr-2" @click="reloadTable"> 还原 </a-button>
    </div>

    <BasicTable @register="registerTable">
      <template #action="{ record }">
        <TableAction
          :actions="[
            {
              label: '客户管理',
              icon: 'ic:outline-delete-outline',
              onClick: CustomerManage.bind(null, record),
            },

            {
              label: '人员管理',
              icon: 'ic:outline-delete-outline',
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
  import { getBasicColumns, TenantListApi } from '../../../api/iotsharp/tenant';
  export default defineComponent({
    components: { BasicTable, TableAction, tenantform },
    setup() {
      const [registerDrawer, { openDrawer }] = useDrawer();
      function onChange() {
        console.log('onChange', arguments);
      }
      const [registerTable, { setColumns, reload, clearSelectedRowKeys }] = useTable({
        canResize: false,
        title: 'useTable示例',
        titleHelpMessage: '使用useTable调用表格内方法',
        api: TenantListApi,
        columns: getBasicColumns(),
        rowKey: 'id',
        showTableSetting: true,
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
      function Edit(record: Recordable) {
        openDrawer(true, {
          record,
          isUpdate: false,
        });
      }
      function UserManage(record: Recordable) {
        console.log('点击了启用', record);
      }
      function CustomerManage(record: Recordable) {
        console.log('点击了启用', record);
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
      };
    },
  });
</script>
