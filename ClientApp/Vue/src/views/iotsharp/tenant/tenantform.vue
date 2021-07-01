<template>
  <BasicDrawer
    v-bind="$attrs"
    @register="registerDrawer"
    showFooter
    :title="getTitle"
    width="720px"
    @ok="handleSubmit"
  >
    <BasicForm @register="registerForm">
      <template #menu="{ model, field }">
        <BasicTree
          v-model:value="model[field]"
          :treeData="treeData"
          :replaceFields="{ title: 'menuName', key: 'id' }"
          checkable
          toolbar
          title="菜单分配"
        />
      </template>
    </BasicForm>
  </BasicDrawer>
</template>

<script lang="ts">
  import { defineComponent, ref, computed, unref } from 'vue';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
  export default defineComponent({
    name: 'tenantform',
    components: { BasicDrawer, BasicForm },
    emits: ['success', 'register'],
    setup(_, { emit }) {
      const isUpdate = ref(true);

      const [registerForm, { resetFields, setFieldsValue, validate }] = useForm({
        labelWidth: 90,
        schemas: [
          {
            field: 'name',
            label: '租户名称',
            required: true,
            component: 'Input',
          },
          {
            field: 'eMail',
            label: '邮箱',
            required: true,
            component: 'Input',
          },
          {
            field: 'phone',
            label: '联系电话',
            required: true,
            component: 'Input',
          },
          {
            field: 'country',
            label: '国家',
            required: true,
            component: 'Input',
          },
          {
            field: 'province',
            label: '省',
            required: true,
            component: 'Input',
          },
          {
            field: 'city',
            label: '市',
            required: true,
            component: 'Input',
          },
          {
            field: 'street',
            label: '街道',
            required: true,
            component: 'Input',
          },
          {
            field: 'address',
            label: '地址',
            required: true,
            component: 'Input',
          },
          {
            field: 'zipCode',
            label: '邮编',
            required: true,
            component: 'Input',
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
            ...data.record,
          });
        }
      });

      const getTitle = computed(() => (!unref(isUpdate) ? '新增角色' : '编辑角色'));

      async function handleSubmit() {
        try {
          const values = await validate();
          setDrawerProps({ confirmLoading: true });
          // TODO custom api
          console.log(values);
          closeDrawer();
          emit('success');
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
