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
}

function isEmptyInputValue(value: NzSafeAny): boolean {
    return value == null || value.length === 0;
}

function isMobile(value: string): boolean {
    return typeof value === 'string' && /(^1\d{10}$)/.test(value);
}
