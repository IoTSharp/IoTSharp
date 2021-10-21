import { Component, OnDestroy } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzNotificationService } from 'ng-zorro-antd/notification';

@Component({
  selector: 'passport-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.less'],
})
export class UserRegisterComponent implements OnDestroy {
  constructor(fb: FormBuilder, private router: Router, public http: _HttpClient, public msg: NzMessageService,    public notification: NzNotificationService,) {
    this.form = fb.group({
      email: ['iotmaster@iotsharp.net', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Validators.minLength(6), UserRegisterComponent.checkPassword.bind(this)]],
      confirm: ['', [Validators.required, Validators.minLength(6), UserRegisterComponent.passwordEquar]],
      mobilePrefix: ['+86'],
      CustomerName: ['iotmaster@iotsharp.net', [Validators.required]],
      tenantName: ['iotmaster@iotsharp.net', [Validators.required]],
      tenantEMail: ['iotmaster@iotsharp.net', [Validators.required, Validators.email]],
      customerEMail: ['iotmaster@iotsharp.net', [Validators.required, Validators.email]],
      phoneNumber: ['15911111111', [Validators.required, Validators.pattern(/^1\d{10}$/)]],
    });
  }
  
  // #region fields

  get mail(): AbstractControl {
    return this.form.controls.mail;
  }
  get password(): AbstractControl {
    return this.form.controls.password;
  }
  get confirm(): AbstractControl {
    return this.form.controls.confirm;
  }
  get mobile(): AbstractControl {
    return this.form.controls.mobile;
  }
  get captcha(): AbstractControl {
    return this.form.controls.captcha;
  }
  form: FormGroup;
  error = '';
  type = 0;
  visible = false;
  status = 'pool';
  progress = 0;
  passwordProgressMap: { [key: string]: 'success' | 'normal' | 'exception' } = {
    ok: 'success',
    pass: 'normal',
    pool: 'exception',
  };

  // #endregion

  // #region get captcha

  count = 0;
  interval$: any;

  static checkPassword(control: FormControl): NzSafeAny {
    if (!control) {
      return null;
    }
    const self: any = this;
    self.visible = !!control.value;
    if (control.value && control.value.length > 9) {
      self.status = 'ok';
    } else if (control.value && control.value.length > 5) {
      self.status = 'pass';
    } else {
      self.status = 'pool';
    }

    if (self.visible) {
      self.progress = control.value.length * 10 > 100 ? 100 : control.value.length * 10;
    }
  }

  static passwordEquar(control: FormControl): { equar: boolean } | null {
    if (!control || !control.parent!) {
      return null;
    }
    if (control.value !== control.parent!.get('Password')!.value) {
      return { equar: true };
    }
    return null;
  }

  getCaptcha(): void {
    if (this.mobile.invalid) {
      this.mobile.markAsDirty({ onlySelf: true });
      this.mobile.updateValueAndValidity({ onlySelf: true });
      return;
    }
    this.count = 59;
    this.interval$ = setInterval(() => {
      this.count -= 1;
      if (this.count <= 0) {
        clearInterval(this.interval$);
      }
    }, 1000);
  }

  // #endregion

  submit(): void {
    this.error = '';
    Object.keys(this.form.controls).forEach((key) => {
      this.form.controls[key].markAsDirty();
      this.form.controls[key].updateValueAndValidity();
    });
    const data = this.form.value;

    if (this.form.invalid) {
      return;
    }
    this.http.post('api/Installer/Install?_allow_anonymous=true', data).subscribe((x) => {
      if(x.code===10000){
        if (x.data.installed) {
          this.router.navigateByUrl('/passport/login?_allow_anonymous=true');
        }else{
          this.router.navigateByUrl('/passport/login?_allow_anonymous=true');
        }
      }else{
        this.notification.error('错误',x.msg);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.interval$) {
      clearInterval(this.interval$);
    }
  }
}

export interface reguser {
  email: string;
  phoneNumber: string;
  customer: string;
  password: string;
}
