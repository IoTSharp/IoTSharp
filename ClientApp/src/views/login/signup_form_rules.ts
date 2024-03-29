export const signUpRule: any[] = [
    {
        type: 'input',
        field: 'email',
        title: '邮箱',
        info: '',
        props: {
            type: 'text',
            placeholder: '请输入邮箱',
            showPassword: false,
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        validate: [
            {
                trigger: 'change',
                message: '请输入正确的邮箱',
                pattern: /^[a-z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$/,
                type: 'email',
                required: true,
            },
        ],
    },
    {
        type: 'input',
        field: 'customerName',
        title: '用户名',
        info: '',
        props: {
            type: 'text',
            placeholder: '用户名',
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        $required: '用户名',
    },
    {
        type: 'input',
        field: 'password',
        title: '密码',
        info: '',
        props: {
            type: 'password',
            placeholder: '密码',
            showPassword: true,
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        $required: '请输入密码',
    },
    {
        type: 'input',
        field: 'passwordCheck',
        title: '密码确认',
        info: '',
        props: {
            type: 'password',
            placeholder: '请再次输入密码',
            showPassword: true,
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        value: '',
        validate: [],
    },
    {
        type: 'input',
        field: 'tenantName',
        title: '租户名称',
        info: '',
        props: {
            placeholder: '租户名称',
            showPassword: false,
            type: 'text',
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        value: '',
        validate: [{
            required: true,
            message: '请输入租户名称'
        }],
    },
    {
        type: 'input',
        field: 'tenantEMail',
        title: '租户邮箱',
        info: '',
        props: {
            placeholder: '租户邮箱',
            showPassword: false,
            type: 'text',
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        value: '',
        validate: [
            {
                trigger: 'change',
                message: '请输入正确的租户邮箱地址',
                pattern: /^[a-z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$/,
                type: 'email',
                required: true,
            }
        ],
    },
    {
        type: 'input',
        field: 'customerEMail',
        title: '客户邮箱',
        info: '',
        props: {
            placeholder: '客户邮箱',
            showPassword: false,
            type: 'text',
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        value: '',
        validate: [
            {
                trigger: 'change',
                message: '请输入正确的客户邮箱',
                pattern: /^[a-z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$/,
                type: 'email',
                required: true,
            },
        ],
    },
    {
        type: 'input',
        field: 'phoneNumber',
        title: '联系电话',
        info: '',
        props: {
            placeholder: '联系电话',
            showPassword: false,
            type: 'text',
        },
        _fc_drag_tag: 'input',
        hidden: false,
        display: true,
        value: '',
        validate: [
            {
                required: true,
                trigger: 'blur',
                message: '请输入正确的手机号',
                pattern: /^(1[3456789]\d{9})$/,
            },
        ],
    },
]
