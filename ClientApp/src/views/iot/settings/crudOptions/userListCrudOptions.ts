import { accountApi } from '/@/api/user';
import _ from 'lodash-es';
import { TableDataRow } from '../model/userListModel';
import { ElMessage } from 'element-plus';
import { compute } from '@fast-crud/fast-crud';
export const createUserListCrudOptions = function ({ expose }, customerId) {
    let records: any[] = [];
    const FsButton = {
        link: true,
    };
    const customSwitchComponent = {
        activeColor: 'var(--el-color-primary)',
        inactiveColor: 'var(el-switch-of-color)',
    };
    const requiredCustomSwitchComponent = {
        required: 'required',
        activeColor: 'var(--el-color-primary)',
        inactiveColor: 'var(el-switch-of-color)',
    };
    const pageRequest = async (query) => {
        let {
            form: { userName: name },
            page: { currentPage: currentPage, pageSize: limit },
        } = query;
        let offset = currentPage === 1 ? 0 : currentPage - 1;
        const res = await accountApi().accountList({ name, limit, offset, customerId });
        return {
            records: res.data.rows,
            currentPage: currentPage,
            pageSize: limit,
            total: res.data.total,
        };
    };
    const editRequest = async ({ form, row }) => {
        form.id = row.id;
        try {
            await accountApi().putAccount(form);
            return form;
        } catch (e) {
            ElMessage.error(e.response.msg);
        }
    };
    const delRequest = async ({ row }) => {
        try {
            await accountApi().deleteAccount(row.id);
            _.remove(records, (item: TableDataRow) => {
                return item.id === row.id;
            });
        } catch (e) {
            ElMessage.error(e.response.msg);
        }
    };
    const addRequest = async ({ form }) => {
        try {
            //验证
            var userName = form.userName;
            var email = form.email;
            if (userName) {
                if (email) {
                    const regEmail = /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(\.[a-zA-Z0-9_-])+/;
                    if (regEmail.test(email)) {
                        await accountApi().postAccount({
                            ...form,
                            customerId,
                        });
                        records.push(form);
                        return form;
                    } else {
                        ElMessage.error('请输入合法邮箱');
                    }
                } else {
                    ElMessage.error('请填写邮箱');
                }
            } else {
                ElMessage.error('请填写用户名');
            }
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
            actionbar: {
                buttons: {
                    add: {

                    },
                },
            },
            form: {
                labelWidth: '80px', //
                beforeSubmit: function (subParam) {
                    var form = subParam.form;
                    if (subParam.mode == 'add') {
                        //验证
                        var userName = form.userName;
                        var email = form.email;
                        if (userName) {
                            if (email) {
                                const regEmail = /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(\.[a-zA-Z0-9_-])+/;
                                if (regEmail.test(email)) {
                                   //需要验证是否重复

                                } else {
                                    ElMessage.error('请输入合法邮箱');
                                    throw new Error('请输入合法邮箱');
                                }
                            } else {
                                ElMessage.error('请填写邮箱');
                                throw new Error('请填写邮箱');
                            }
                        } else {
                            ElMessage.error('请填写用户名');
                            throw new Error('请填写用户名');
                        }
                    }
                }
            },
            search: {
                show: true,
            },
            rowHandle: {
                width: 180,
                buttons: {
                    view: {
                        icon: 'View',
                        ...FsButton,
                        show: false,
                    },
                    edit: {
                        icon: 'EditPen',
                        ...FsButton,
                        order: 1,
                    },
                    remove: {
                        icon: 'Delete',
                        ...FsButton,
                        order: 2,
                    }, //删除按钮
                },
            },
            columns: {
                tenantName: {
                    title: '所属租户',
                    type: 'text',
                    column: { width: 140 },
                    addForm: {
                        show: false,
                    },
                    editForm: {
                        show: false,
                    },
                },
                customerName: {
                    title: '所属客户',
                    type: 'text',
                    column: { width: 140 },
                    addForm: {
                        show: false,
                    },
                    editForm: {
                        show: false,
                    },
                },
                //id: {
                //	title: 'Id',
                //	type: 'text',
                //	column: { width: 300 },
                //	addForm: {
                //		show: false,
                //	},
                //	editForm: {
                //		show: false,
                //	},
                //},
                userName: {
                    title: '名称',
                    type: 'text',
                    column: { width: 200 },
                    search: { show: true }, //显示查询
                    addForm: {
                        show: true,
                        component: requiredCustomSwitchComponent,
                    },
                    editForm: {
                        show: true,
                        component: requiredCustomSwitchComponent,
                    },
                },
                email: {
                    title: '邮件',
                    column: { width: 200 },
                    type: 'text',
                    addForm: {
                        show: true,
                        component: requiredCustomSwitchComponent,
                    },
                    editForm: {
                        show: true,
                        component: requiredCustomSwitchComponent,
                    },
                },
                phoneNumber: {
                    title: '电话',
                    column: { width: 140 },
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
                accessFailedCount: {
                    title: '登录失败次数',
                    column: { width: 135 },
                    type: 'text',
                    addForm: {
                        show: false,
                    },
                    editForm: {
                        show: false,
                    },
                },
                lockoutEnabled: {
                    title: '锁定',
                    addForm: {
                        show: false,
                    },
                    column: {
                        width: 100,
                        component: {
                            name: 'fs-dict-switch',
                            show: true,
                            onChange: compute((context) => {
                                return async () => {
                                    const { id: Id, lockoutEnabled } = context.row;
                                    await accountApi().updateAccountStatus({
                                        Id,
                                        opt: lockoutEnabled ? 'Lock' : 'Unlock',
                                    });
                                };
                            }),
                        },
                    },
                    editForm: {
                        show: false,
                    },
                },
                lockoutEnd: {
                    addForm: {
                        show: false,
                    },
                    title: '锁定截止时间',
                    //column: { width: 150 },
                    type: 'text',
                    editForm: {
                        show: false,
                    },
                },
            },
        },
    };
};
