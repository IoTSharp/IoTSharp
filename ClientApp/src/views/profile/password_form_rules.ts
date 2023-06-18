export const passwordFormRules = [
  {
    type: 'input',
    field: 'pass',
    title: '原密码',
    info: '',
    props: {
      type: 'password',
      placeholder: '请输入原密码',
      showPassword: true
    },
    _fc_drag_tag: 'input',
    hidden: false,
    display: true,
    validate: [
      {
        trigger: 'change',
        message: '请输入原密码',
        required: true,
      },
    ],
  },
  {
    type: 'input',
    field: 'passnew',
    title: '新密码',
    info: '',
    props: {
      type: 'password',
      placeholder: '请输入新密码',
      showPassword: true
    },
    _fc_drag_tag: 'input',
    hidden: false,
    display: true,
    validate: [
      {
        trigger: 'change',
        message: '请输入新密码',
        required: true,
      },
    ],
  },
  {
    type: 'input',
    field: 'passnewsecond',
    title: '请输入新密码',
    info: '',
    props: {
      placeholder: '请重新输入新密码',
      showPassword: true,
      type: 'password',
    },
    _fc_drag_tag: 'input',
    hidden: false,
    display: true,
    value: '',
    validate: [],
  },
]
