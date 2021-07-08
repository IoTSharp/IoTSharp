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
    <userform @register="registerDrawer" @success="handleSuccess" />
  </div>
</template>
<script lang="ts">
  import { defineComponent, reactive } from 'vue';
  import { BasicTable, ColumnChangeParam, useTable, TableAction } from '/@/components/Table';
  import { useDrawer } from '/@/components/Drawer';
  import userform from './userform.vue';
  import { useRouter } from 'vue-router';
  import { getBasicColumns, UserListApi, Get } from '../../../api/iotsharp/user';
  export default defineComponent({
    components: { BasicTable, TableAction, userform },

    setup() {
      const router = useRouter();
      const searchInfo = reactive<Recordable>({});
      if (router.currentRoute.value.query.customerid) {
        searchInfo.customerId = router.currentRoute.value.query.customerid;
      } else if (router.currentRoute.value.query.tenantid) {
        searchInfo.tenantid = router.currentRoute.value.query.tenantid;
      }

      const [registerDrawer, { openDrawer }] = useDrawer();
      function onChange() {
        console.log('onChange', arguments);
      }
      const [registerTable, { setColumns, reload, clearSelectedRowKeys }] = useTable({
        canResize: false,
        title: '人员管理',
        titleHelpMessage: '人员管理',
        api: UserListApi,
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
      function Delete(record: Recordable): void {
        console.log('点击了删除', record);
      }
      function Open(record: Recordable) {
        openDrawer(true, {
          item: { customer: searchInfo.customerId },
          isUpdate: false,
        });
      }
      function Edit(record: Recordable) {
        Get(record.id).then((x) => {
          openDrawer(true, {
            x,
            isUpdate: true,
          });
        });
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
        registerDrawer,
        searchInfo,
      };
    },
  });
</script>
