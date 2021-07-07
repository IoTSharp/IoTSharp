<template>
  <BasicDrawer
    v-bind="$attrs"
    @register="registerDrawer"
    showFooter
    :title="getTitle"
    width="720px"
    @ok="handleSubmit"
  >
    <BasicForm @register="registerForm" />
  </BasicDrawer>
</template>

<script lang="ts">
  import { Save, Update } from '../../../api/iotsharp/user';
  import { defineComponent, ref, computed, unref } from 'vue';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
  export default defineComponent({
    name: 'Tenantform',
    components: { BasicDrawer, BasicForm },
    emits: ['success', 'register'],
    setup(_, { emit }) {
      const isUpdate = ref(true);
      const [registerForm, { resetFields, setFieldsValue, validate, getFieldsValue }] = useForm({
        labelWidth: 90,
        schemas: [
          {
            field: 'email',
            label: '邮箱',
            required: true,
            component: 'Input',
            rules: [
              {
                required: true,
                validator: async (rule, value) => {
                  if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value)) {
                    return Promise.reject('邮箱格式错误');
                  }
                  return Promise.resolve();
                },
              },
            ],
          },
          {
            field: 'phoneNumber',
            label: '联系电话',
            required: true,
            component: 'Input',
            rules: [
              {
                required: true,
                validator: async (rule, value) => {
                  if (
                    !/^(13[0-9]|14[5|7]|15[0|1|2|3|4|5|6|7|8|9]|18[0|1|2|3|5|6|7|8|9])\d{8}$/.test(
                      value
                    )
                  ) {
                    return Promise.reject('联系电话格式不正确');
                  }
                  return Promise.resolve();
                },
              },
            ],
          },

          {
            field: 'password',
            label: '密码',

            component: 'Input',
            rules: [
              {
                required: true,
                validator: async (rule, value) => {
                  if (!value) {
                    return Promise.reject('两次密码不一致');
                  }
                  if (value.length < 6) {
                    return Promise.reject('密码长度不应小于6');
                  }
                  if (!/^(?=.*[a-zA-Z])(?=.*[1-9])(?=.*[!|@|_|#|$|%|^|&|*]).{6,}$/.test(value)) {
                    return Promise.reject('密码必须包含大小写字符以及特殊符号');
                  }
                  return Promise.resolve();
                },
              },
            ],
          },
          {
            field: 'passwords',
            label: '确认密码',
            rules: [
              {
                required: true,
                validator: async (rule, value) => {
                  if (value != getFieldsValue().password) {
                    return Promise.reject('两次密码不一致');
                  }

                  return Promise.resolve();
                },
                trigger: 'change',
              },
            ],
            component: 'Input',
          },
          {
            field: 'customer',
            label: '联系电话',
            required: false,
            component: 'Input',
            show: false,
          },
        ],
        showActionButtonGroup: false,
      });

      const [registerDrawer, { setDrawerProps, closeDrawer }] = useDrawerInner(async (data) => {
        resetFields();
        setDrawerProps({ confirmLoading: false });
        isUpdate.value = !!data?.isUpdate;

        if (unref(isUpdate)) {
          setFieldsValue({
            ...data.item,
          });
        } else {
          setFieldsValue({
            ...data.item,
          });
        }
      });

      const getTitle = computed(() => (!unref(isUpdate) ? '新增设备' : '编辑设备'));

      async function handleSubmit() {
        try {
          const values = await validate();

          setDrawerProps({ confirmLoading: true });
          // TODO custom api

          if (values.id) {
            Update(values).then((x) => {
              emit('success');
              closeDrawer();
            });
          } else {
            Save(values).then((x) => {
              emit('success');
              closeDrawer();
            });
          }
        } finally {
          setDrawerProps({ confirmLoading: false });
        }
      }

      return {
        registerDrawer,
        registerForm,
        getTitle,
        handleSubmit,
      };
    },
  });
</script>
