import {tenantApi} from '/@/api/tenants';
import _ from 'lodash-es';
import { useRouter } from 'vue-router';
import {TableDataRow} from '../model/tenantListModel';
import {ElMessage} from "element-plus";
// eslint-disable-next-line no-unused-vars
export const createTenantListCrudOptions = function ({expose}) {
  let records: any[] = [];
  const router = useRouter();
  const FsButton = {
    link: true,
  };
  const customSwitchComponent = {
    activeColor: 'var(--el-color-primary)',
    inactiveColor: 'var(el-switch-of-color)',
  };
  const pageRequest = async (query) => {
    let { form: {
      name,
    }, page: {
      currentPage,
      pageSize: limit
    }} = query;
    currentPage = currentPage === 1 ? 0 : currentPage - 1
    const res = await tenantApi().tenantList({name, limit: limit, offset: currentPage});
    return {
      records: res.data.rows,
      currentPage: 1,
      pageSize: 20,
      total: res.data.total,
    };
  };
  const editRequest = async ({form, row}) => {
    form.id = row.id;
    try {
      await tenantApi().puttenant(form);
      return form;
    } catch (e) {
      ElMessage.error(e.response.msg);
    }
  };
  const delRequest = async ({row}) => {
    try {
      await tenantApi().deletetenant(row.id);
      _.remove(records, (item: TableDataRow) => {
        return item.id === row.id;
      });
    } catch (e) {
      ElMessage.error(e.response.msg);
    }
  };

  const addRequest = async ({form}) => {
    try {
      await tenantApi().posttenant(form);
      records.push(form);
      return form;
    } catch (e) {
      ElMessage.error(e.response.msg);
    }
  };
  return {
    crudOptions: {
      request: {
        pageRequest,
        addRequest,
        delRequest,
        editRequest,
      },
      table: {
        border: false,
      },
      form: {
        labelWidth: '80px', //
      },
      search: {
        show: true,
      },
      rowHandle: {
        width: 250,
        scope: 'id',
        buttons: {
          view: {
            icon: 'View',
            show: false,
            ...FsButton,
          },
          edit: {
            icon: 'Edit',
            ...FsButton,
            order: 2,
          },
          custom: {
            text: '客户管理',
            title: '客户管理',
            icon: 'User',
            order: 1,
            type: 'info',
            ...FsButton,
            click: (e) => {
              router.push({
                path: '/iot/settings/customerlist',
                query: {
                  id: e.row.id
                }
              })
            }
          },
          remove: {
            icon: 'Delete',
            order: 3,
            ...FsButton,
          }, //删除按钮
        },
      },
      columns: {
        name: {
          title: '名称',
          type: 'text',
          column: {width: 200},
          search: {show: true}, //显示查询
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        email: {
          title: '邮件',
          type: 'text',
          column: { width: 180 },
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        phone: {
          title: '电话',
          column: { width: 150 },
          type: 'text',
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        country: {
          title: '国家',
          column: { width: 80 },
          type: 'text',
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        province: {
          title: '省',
          type: 'text',
          column: { width: 80 },
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        city: {
          title: '市',
          type: 'text',
          column: { width: 100 },
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        street: {
          title: '街道',
          column: { width: 180 },
          type: 'text',
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        address: {
          title: '地址',
          column: { width: 180 },
          type: 'text',
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
        zipCode: {
          title: '邮编',
          column: { width: 150 },
          type: 'text',
          addForm: {
            show: true,
            component: customSwitchComponent,
          },
          editForm: {
            show: true,
            component: customSwitchComponent,
          },
        },
      }
    },
  };
};
