import type { ValidationRule } from 'ant-design-vue/lib/form/Form';
import type { RuleObject } from 'ant-design-vue/lib/form/interface';
import { ref, computed, unref, Ref } from 'vue';
import { useI18n } from '/@/hooks/web/useI18n';

export enum LoginStateEnum {
  LOGIN,
  REGISTER,
  RESET_PASSWORD,
  MOBILE,
  QR_CODE,
}

const currentState = ref(LoginStateEnum.LOGIN);

export function useLoginState() {
  function setLoginState(state: LoginStateEnum) {
    currentState.value = state;
  }

  const getLoginState = computed(() => currentState.value);

  function handleBackLogin() {
    setLoginState(LoginStateEnum.LOGIN);
  }

  return { setLoginState, getLoginState, handleBackLogin };
}

export function useFormValid<T extends Object = any>(formRef: Ref<any>) {
  async function validForm() {
    const form = unref(formRef);
    if (!form) return;
    const data = await form.validate();
    return data as T;
  }

  return { validForm };
}

export function useFormRules(formData?: Recordable) {
  const { t } = useI18n();

  const getAccountFormRule = computed(() => createRule(t('sys.login.accountPlaceholder')));
  const getPasswordFormRule = computed(() => createRule(t('sys.login.passwordPlaceholder')));
  const getSmsFormRule = computed(() => createRule(t('sys.login.smsPlaceholder')));

  const getcustomerFormRule = computed(() => createRule(t('sys.login.customerPlaceholder')));
  const gettenantNameFormRule = computed(() => createRule(t('sys.login.tenantNamePlaceholder')));

  const validatePolicy = async (_: RuleObject, value: boolean) => {
    return !value ? Promise.reject(t('sys.login.policyPlaceholder')) : Promise.resolve();
  };

  const validateConfirmPassword = (password: string) => {
    return async (_: RuleObject, value: string) => {
      if (!value) {
        return Promise.reject(t('sys.login.passwordPlaceholder'));
      }
      if (value !== password) {
        return Promise.reject(t('sys.login.diffPwd'));
      }
      return Promise.resolve();
    };
  };
  const validateEMail = (email: string) => {
    return async (_: RuleObject, value: string) => {
      const pattern = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;
      if (!value) {
        return Promise.reject(t('sys.login.invalidEMail'));
      }
      if (!pattern.test(value)) {
        return Promise.reject(t('sys.login.invalidEMail'));
      }
      return Promise.resolve();
    };
  };
  const validatemobile = (mobile: string) => {
    return async (_: RuleObject, value: string) => {
      const pattern = /^(13[0-9]|15[0-9]|17[0-9]|18[0-9]|14[0-9])[0-9]{8}$/;
      if (!value) {
        return Promise.reject(t('sys.login.validatemobile'));
      }
      if (!pattern.test(value)) {
        console.log(t('sys.login.validatemobile'));
        return Promise.reject(t('sys.login.validatemobile'));
      }
      return Promise.resolve();
    };
  };
  const getFormRules = computed((): { [k: string]: ValidationRule | ValidationRule[] } => {
    const accountFormRule = unref(getAccountFormRule);
    const passwordFormRule = unref(getPasswordFormRule);
    const smsFormRule = unref(getSmsFormRule);
    const customerFormRule = unref(getcustomerFormRule);
    const tenantNameFormRule = unref(gettenantNameFormRule);

    const mobileRule = {
      sms: smsFormRule,
      phoneNumber: [{ validator: validatemobile(formData?.phoneNumber) }],
    };
    switch (unref(currentState)) {
      // register form rules
      case LoginStateEnum.REGISTER:
        return {
          email: [{ validator: validateEMail(formData?.email) }],
          account: accountFormRule,
          password: passwordFormRule,
          customerEMail: [{ validator: validateEMail(formData?.customerEMail) }],
          tenantName: tenantNameFormRule,
          tenantEMail: [{ validator: validateEMail(formData?.tenantEMail) }],
          customerName: customerFormRule,
          confirmPassword: [
            { validator: validateConfirmPassword(formData?.password), trigger: 'change' },
          ],
          policy: [{ validator: validatePolicy, trigger: 'change' }],
          ...mobileRule,
        };

      // reset password form rules
      case LoginStateEnum.RESET_PASSWORD:
        return {
          account: accountFormRule,
          ...mobileRule,
        };

      // mobile form rules
      case LoginStateEnum.MOBILE:
        return mobileRule;

      // login form rules
      default:
        return {
          account: accountFormRule,
          password: passwordFormRule,
        };
    }
  });
  return { getFormRules };
}

function createRule(message: string) {
  return [
    {
      required: true,
      message,
      trigger: 'change',
    },
  ];
}
