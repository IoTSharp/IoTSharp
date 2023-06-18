export const rule = [
  {
    type: 'input',
    field: 'email',
    title: '邮箱',
    info: '',
    props: {
      type: 'text',
      placeholder: '请输入邮箱',
      showPassword: false,
      prefixIcon: 'Message',
    },
    _fc_drag_tag: 'input',
    hidden: false,
    display: true,
    validate: [
      {
        trigger: 'change',
        message: '请输入正确的邮箱',
        pattern:  /^[a-z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$/,
        type: 'email',
        required: true,
      },
    ],
  },
  {
    type: 'input',
    field: 'name',
    title: '用户名',
    info: '',
    props: {
      type: 'text',
      placeholder: '请输入用户名',
      prefixIcon: 'User',
    },
    _fc_drag_tag: 'input',
    hidden: false,
    display: true,
    $required: '请输入用户名',
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
      prefixIcon: 'Cellphone'
    },
    _fc_drag_tag: 'input',
    hidden: false,
    display: true,
    value: '',
    validate: [
      {
        trigger: 'change',
        message: '请输入正确的手机号',
        pattern: /^(1[3456789]\d{9})$/,
      },
    ],
  },
]
