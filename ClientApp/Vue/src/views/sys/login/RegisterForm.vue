<template>
  <template v-if="getShow">
    <LoginFormTitle class="enter-x" />
    <Form class="p-4 enter-x" :model="formData" :rules="getFormRules" ref="formRef">
      <FormItem name="email" class="enter-x">
        <Input size="large" v-model:value="formData.email" :placeholder="t('sys.login.email')" />
      </FormItem>

      <FormItem name="customerName" class="enter-x">
        <Input
          size="large"
          v-model:value="formData.customerName"
          :placeholder="t('sys.login.customer')"
        />
      </FormItem>

      <FormItem name="customerEMail" class="enter-x">
        <Input
          size="large"
          v-model:value="formData.customerEMail"
          :placeholder="t('sys.login.customerEMail')"
        />
      </FormItem>

      <FormItem name="tenantName" class="enter-x">
        <Input
          size="large"
          v-model:value="formData.tenantName"
          :placeholder="t('sys.login.tenantName')"
        />
      </FormItem>
      <FormItem name="tenantEMail" class="enter-x">
        <Input
          size="large"
          v-model:value="formData.tenantEMail"
          :placeholder="t('sys.login.tenantEMail')"
        />
      </FormItem>
      <FormItem name="phoneNumber" class="enter-x">
        <Input
          size="large"
          v-model:value="formData.phoneNumber"
          :placeholder="t('sys.login.mobile')"
        />
      </FormItem>
      <!-- <FormItem name="sms" class="enter-x">
        <CountdownInput
          size="large"
          v-model:value="formData.sms"
          :placeholder="t('sys.login.smsCode')"
        />
      </FormItem> -->
      <FormItem name="password" class="enter-x">
        <StrengthMeter
          size="large"
          v-model:value="formData.password"
          :placeholder="t('sys.login.password')"
        />
      </FormItem>
      <FormItem name="confirmPassword" class="enter-x">
        <InputPassword
          size="large"
          visibilityToggle
          v-model:value="formData.confirmPassword"
          :placeholder="t('sys.login.confirmPassword')"
        />
      </FormItem>

      <FormItem class="enter-x" name="policy">
        <!-- No logic, you need to deal with it yourself -->
        <Checkbox v-model:checked="formData.policy" size="small">
          {{ t('sys.login.policy') }}
        </Checkbox>
      </FormItem>

      <Button
        type="primary"
        class="enter-x"
        size="large"
        block
        @click="handleRegister"
        :loading="loading"
      >
        {{ t('sys.login.registerButton') }}
      </Button>
      <Button size="large" block class="mt-4 enter-x" @click="handleBackLogin">
        {{ t('sys.login.backSignIn') }}
      </Button>
    </Form>
  </template>
</template>
<script lang="ts">
  import { defineComponent, reactive, ref, unref, computed } from 'vue';

  import LoginFormTitle from './LoginFormTitle.vue';
  import { Form, Input, Button, Checkbox } from 'ant-design-vue';
  import { StrengthMeter } from '/@/components/StrengthMeter';

  import { useI18n } from '/@/hooks/web/useI18n';
  import { useLoginState, useFormRules, useFormValid, LoginStateEnum } from './useLogin';
  import { Register } from '/@/api/sys/user';
  export default defineComponent({
    name: 'RegisterPasswordForm',
    components: {
      Button,
      Form,
      FormItem: Form.Item,
      Input,
      InputPassword: Input.Password,
      Checkbox,
      StrengthMeter,

      LoginFormTitle,
    },
    setup() {
      const { t } = useI18n();
      const { handleBackLogin, getLoginState, setLoginState } = useLoginState();

      const formRef = ref();
      const loading = ref(false);

      const formData = reactive({
        email: '',
        customerName: '',
        tenantEMail: '',
        customerEMail: '',
        tenantName: '',
        phoneNumber: '',
        password: '',
        confirmPassword: '',
        policy: false,
      });

      const { getFormRules } = useFormRules(formData);
      const { validForm } = useFormValid(formRef);

      const getShow = computed(() => unref(getLoginState) === LoginStateEnum.REGISTER);

      async function handleRegister() {
        const data = await validForm();
        if (!data) return;
        Register(data).then(
          (x) => {
            if (x.installed) {
              setLoginState(LoginStateEnum.LOGIN);
            }
          },
          (y) => {
            console.log(y);
          },
          () => {}
        );
      }

      return {
        t,
        formRef,
        formData,
        getFormRules,
        handleRegister,
        loading,
        handleBackLogin,
        getShow,
      };
    },
  });
</script>
