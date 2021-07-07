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
          title="设备信息"
        />
      </template>
    </BasicForm>
  </BasicDrawer>
</template>

<script lang="ts">
  import { Guid } from 'guid-typescript';
  import { Save, Update } from '../../../api/iotsharp/device';
  import { defineComponent, ref, computed, unref } from 'vue';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
  export default defineComponent({
    name: 'Tenantform',
    components: { BasicDrawer, BasicForm },
    emits: ['success', 'register'],
    setup(_, { emit }) {
      const isUpdate = ref(true);

      const [registerForm, { resetFields, setFieldsValue, validate }] = useForm({
        labelWidth: 90,
        schemas: [
          {
            field: 'name',
            label: '设备名称',
            required: true,
            component: 'Input',
          },
          {
            field: 'deviceType',
            label: '设备类型',
            required: true,
            component: 'Select',
            componentProps: {
              options: [
                {
                  label: '设备',
                  value: '0',
                  key: '0',
                },
                {
                  label: '网关',
                  value: '1',
                  key: '1',
                },
              ],
            },
          },
          {
            field: 'id',
            label: 'id',
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
            values.id = Guid.create().value;
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
