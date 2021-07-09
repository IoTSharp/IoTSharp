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
  import { GetIdentity, Save, Update, SetAttribute } from '../../../api/iotsharp/device';
  import { defineComponent, ref, computed, unref } from 'vue';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';

  import { FormSchema } from '/@/components/Table';
  //need a dummy data.
  const schemas: FormSchema[] = [
    {
      field: 'id',
      label: 'id',
      component: 'Input',
    },
  ];

  export default defineComponent({
    name: 'Tenantform',
    components: { BasicDrawer, BasicForm },
    emits: ['success', 'register'],
    setup(_, { emit }) {
      const isUpdate = ref(true);

      const [
        registerForm,
        { resetFields, setFieldsValue, validate, appendSchemaByField, updateSchema },
      ] = useForm({
        schemas: schemas,
        showActionButtonGroup: false,
      });

      const getTitle = computed(() => (!unref(isUpdate) ? '修改属性' : '修改属性'));

      async function handleSubmit() {
        try {
          const values = await validate();

          console.log(values);
          setDrawerProps({ confirmLoading: true });
          // TODO custom api

          GetIdentity(values.id).then((x) => {
            delete values.id; //remove extra data
            //  delete values.accesstoken;

            SetAttribute(values, x.identityId).then((y) => {
              emit('success');
              closeDrawer();
            });
          });
        } finally {
          setDrawerProps({ confirmLoading: false });
        }
      }
      const [registerDrawer, { setDrawerProps, closeDrawer }] = useDrawerInner(async (data) => {
        resetFields();
        setDrawerProps({ confirmLoading: false });
        isUpdate.value = !!data?.isUpdate;
        console.log(data);
        var o = {};
        data.item.map((x) => {
          appendSchemaByField(
            {
              field: x.keyName,
              component: 'Input',
              label: x.keyName,
              defaultValue: x.value, //not works,after form created,you need rebind data
              colProps: {
                span: 8,
              },
            },
            x.keyName
          );
          o[x.keyName] = x.value;
          return { keyName: x.keyName, value: x.value };
          //it does work too
          // schemas.push({
          //   field: x.keyName,
          //   component: 'Input',
          //   label: x.keyName,
          //   colProps: {
          //     span: 8,
          //   },
          // });
        });
        //hide id field
        updateSchema({
          field: 'id',
          label: 'id',
          component: 'Input',
          show: false,
        });

        //binding field value
        setFieldsValue({
          ...o,
        });

        if (unref(isUpdate)) {
        }
      });
      function appendField(item) {
        appendSchemaByField(
          {
            field: 'field10',
            label: '字段10',
            component: 'Input',
            colProps: {
              span: 8,
            },
          },
          'field3'
        );
      }
      return {
        registerDrawer,
        registerForm,
        getTitle,
        schemas,
        handleSubmit,
        appendField,
      };
    },
  });
</script>
