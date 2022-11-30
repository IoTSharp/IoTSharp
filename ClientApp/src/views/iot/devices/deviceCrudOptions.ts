import {deviceApi} from "/@/api/devices";
import _ from "lodash-es";
import {compute, dict} from "@fast-crud/fast-crud";
import {Session} from "/@/utils/storage";
import {TableDataRow} from "/@/views/iot/devices/model";
const userInfos = Session.get("userInfo");
// eslint-disable-next-line no-unused-vars
export const createDeviceCrudOptions = function ({ expose }, deviceDetailRef?) {
    let records:any[] = []
    const FsButton = {
        link: true
    }
    const customSwitchComponent = {
        activeColor: 'var(--el-color-primary)',
        inactiveColor: 'var(el-switch-of-color)'
    }
    const pageRequest = async (query) => {
        const params = reactive({
            offset: query.page.currentPage - 1,
            limit: query.page.pageSize,
            onlyActive: false,
            customerId: userInfos.customer.id,
            name: "",
        });

        const res = await deviceApi().devcieList(params)
        records = res.data.rows
        return {
            records,
            currentPage:1,
            pageSize:20,
            total:res.data.total
        }
    };
    const editRequest = async ({ form, row }) => {
        const newItem = _.clone(form)
        newItem.id = row.id
        const target = _.find(records, (item: TableDataRow) =>{return row.id === item.id})
        try {
            await deviceApi().putdevcie(newItem)
            _.merge(target,form)
            return target;
        } catch (e ) {
            ElMessage.error(e.response.msg)
        }
    };
    const delRequest = async ({ row }) => {
        try {
            await deviceApi().deletedevcie(row.id)
            _.remove(records, (item:TableDataRow) => {
                return item.id === row.id
            })
        } catch (e) {
            ElMessage.error(e.response.msg)
        }
    };

    const addRequest = async ({ form }) => {
        await deviceApi().postdevcie(form)
        records.push(form)
        return form
    };
    return {
        crudOptions: {
            request: {
                pageRequest,
                addRequest,
                editRequest,
                delRequest
            },
            table: {
                border: false,
            },
            form: {
                labelWidth: "130px", //
            },
            rowHandle: {
                width: 360,
                dropdown: {
                    more: {
                        //更多按钮配置
                        text: "属性",
                        ...FsButton,
                        icon: 'operation'
                    }
                },

                buttons: {

                    view:{
                        icon:"view", //图标
                        ...FsButton,
                        // FsButton的配置，可以修改文本、颜色，也可以修改成图标按钮、纯文本按钮等
                        order:1, //排序号，越小则排前面，默认值1
                        dropdown:false,//是否折叠此按钮，当配置为true，将会收起到dropdown中
                        //点击事件,点击此按钮会触发此方法
                    },
                    edit:{
                        icon:"editPen",
                        ...FsButton,
                        order:1}, //编辑按钮
                    remove:{
                        icon:"Delete",
                        ...FsButton,
                        order:5},//删除按钮
                    attrNew: {
                        size: "small",
                        dropdown: true,
                        icon:"plus", //图标
                        ...FsButton,
                        text:"新增属性",//按钮文字
                    },
                    attrEdit: {
                        size: "small",
                        dropdown: true,
                        icon:"editPen", //图标
                        ...FsButton,
                        text:"属性修改",//按钮文字
                    },
                    token: {
                        ...FsButton,
                        type: 'primary',
                        text:compute(({row})=>{
                            if (row.identityType === 'AccessToken') return '获取token'
                            else if (row.identityType === 'X509Certificate') return '下载证书 '

                        }),//按钮文字
                    }
                }
            },
            columns: {
                name: {
                    title: "设备名称",
                    type: "button",
                    search: {show: true},
                    column: {
                        component: {
                            ...FsButton,
                            type: 'primary',
                            on:{
                                onClick({row}){
                                    deviceDetailRef.value.openDialog(row)
                                }
                            }
                        }
                    }
                },
                deviceType: {
                    title: "设备类型",
                    type: "dict-select",
                    search: {show: false},
                    dict: dict({
                        data: [
                            { value: 'Gateway', label: "网关", },
                            { value: 'Device', label: "设备", color: "warning" }
                        ]
                    })
                },
                active: {
                    title: "在线状态",
                    type: "dict-switch",
                    search: {show: false},
                    dict: dict({
                        data: [
                            { value: true, label: "在线",  },
                            { value: false, label: "离线", color: "danger" }
                        ]
                    }),
                    column: {
                    },
                    viewForm: {
                        show: true,
                        component: customSwitchComponent
                    },
                    addForm: {
                        show: false,
                        component: customSwitchComponent
                    },
                    editForm: {
                        show: false,
                        component: customSwitchComponent
                    }
                },
                lastActivityDateTime: {
                    title: "最后活动时间",
                    type: "text",
                    column: {
                        show: false
                    },
                    search: {show: false},
                    addForm: {
                        show: false,
                    },
                    editForm: {
                        show: false,
                    }
                },
                identityType: {
                    title: "认证方式",
                    type: "dict-select",
                    search: {show: false},
                    dict: dict({
                        data: [
                            { value: 'AccessToken', label: "AccessToken" },
                            { value: 'X509Certificate', label: "X509Certificate" }
                        ]
                    })
                },
                identityId: {
                    title: "Token",
                    type: "text",
                    search: {show: false},
                    column: {
                        show: false
                    },
                    viewForm: {
                        show: false,
                    },
                    addForm: {
                        show: false,
                    },
                    editForm: {
                        show: false,
                    }
                },
                timeout: {
                    column: {
                        show: false
                    },
                    title: "超时",
                    type: "text",
                    search: {show: false},
                }
            }
        }
    };
}
