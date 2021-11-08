import { AbstractControl, ValidatorFn, Validators } from "@angular/forms";
import { NzSafeAny } from "ng-zorro-antd/core/types";

export type MyErrorsOptions = { 'zh-cn': string; en: string } & Record<string, NzSafeAny>;
export type MyValidationErrors = Record<string, MyErrorsOptions>;
export class MyValidators extends Validators {
    static minLength(minLength: number): ValidatorFn {
        return (control: AbstractControl): MyValidationErrors | null => {
            if (Validators.minLength(minLength)(control) === null) {
                return null;
            }
            return { minlength: { 'zh-cn': `最小长度为 ${minLength}`, en: `MinLength is ${minLength}` } };
        };
    }

    static maxLength(maxLength: number): ValidatorFn {
        return (control: AbstractControl): MyValidationErrors | null => {
            if (Validators.maxLength(maxLength)(control) === null) {
                return null;
            }
            return { maxlength: { 'zh-cn': `最大长度为 ${maxLength}`, en: `MaxLength is ${maxLength}` } };
        };
    }

    static mobile(control: AbstractControl): MyValidationErrors | null {
        const value = control.value;

        if (isEmptyInputValue(value)) {
            return null;
        }
        return isMobile(value) ? null : { mobile: { 'zh-cn': `手机号码格式不正确`, en: `Mobile phone number is not valid` } };
    }

    static nullbigintid(control: AbstractControl): MyValidationErrors | null {
        const value = control.value;
        if (value !== 0) {
            return null;
        }
        return { messsage: { 'zh-cn': `值不能为空`, en: `this value is not empty` } };
    }


    static ip(control: AbstractControl): MyValidationErrors | null {
        const value = control.value;

        if (isEmptyInputValue(value)) {
            return null;
        }
        return isIp(value) ? null : { mobile: { 'zh-cn': `Ip格式不正确`, en: `ip is not valid` } };
    }

    static identity(control: AbstractControl): MyValidationErrors | null {
        const value = control.value;

        if (isEmptyInputValue(value)) {
            return null;
        }
        return isIdentity(value) ? null : { mobile: { 'zh-cn': `身份证格式不正确`, en: `identity is not valid` } };
    }
   static zip(control: AbstractControl): MyValidationErrors | null {
        const value = control.value;

        if (isEmptyInputValue(value)) {
            return null;
        }
        return isZip(value) ? null : { mobile: { 'zh-cn': `邮编格式不正确`, en: `zipCode is not valid` } };
    }
    static email(control: AbstractControl): MyValidationErrors | null {
        const value = control.value;

        if (isEmptyInputValue(value)) {
            return null;
        }
        return isEmail(value) ? null : { mobile: { 'zh-cn': `邮件格式不正确`, en: `email is not valid` } };
    }
 


    static ValidField(control: AbstractControl): MyValidationErrors | null {
        const value = control.value;

        if (isEmptyInputValue(value)) {
            return null;
        }
        return isValidField(value) ? null : { filed: { 'zh-cn': `字段格式不正确`, en: `field name is not valid` } };
    }
 
}

function isEmptyInputValue(value: NzSafeAny): boolean {
    return value == null || value.length === 0;
}


function isZip(value: string): boolean {
    return typeof value === 'string' && /[1-9]\d{5}(?!\d)/.test(value);
}

function isIp(value: string): boolean {
    return typeof value === 'string' && /((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}/.test(value);
}

function isIdentity(value: string): boolean {
    return typeof value === 'string' && /(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)/.test(value);
}

function isEmail(value: string): boolean {
    return typeof value === 'string' && /^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value);
}

function isMobile(value: string): boolean {
    return typeof value === 'string' && /(^1\d{10}$)/.test(value);
}
function isValidField(value: string): boolean {
    return typeof value === 'string' && /^[^\d#\$\*\+@!%\^&-=]{1,}[\u4E00-\u9FA5\uf900-\ufa2d\w]{1,50}/.test(value);
}