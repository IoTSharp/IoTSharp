import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-account-settings-security',
  templateUrl: './security.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProAccountSettingsSecurityComponent implements OnInit {
  form: FormGroup;
  constructor(public msg: NzMessageService, private http: _HttpClient, private fb: FormBuilder) {
    this.form = fb.group({
      pass: ['', [Validators.required]],
      passnew: ['', [Validators.required, Validators.minLength(6), ProAccountSettingsSecurityComponent.checkPassword.bind(this)]],
      passnewsecond: ['', [Validators.required, Validators.minLength(6), ProAccountSettingsSecurityComponent.passwordEquar]]
    });
  }

  ngOnInit(): void {}
  save() {
    const data = this.form.value;
    if (this.form.invalid) {
      return;
    }
    this.http.put<appmessage<any>>('api/Account/ModifyMyPassword', data).subscribe({
      next: next => {
        if (next.code === 10000) {
          this.msg.create('error', '密码修改成功');
        } else {
          this.msg.create('error', '密码修改异常:'+next.msg);
        }
      },
      error: error => {
        this.msg.create('error', '密码修改异常');
      },
      complete: () => {}
    });
  }
  status = 'pool';
  progress = 0;
  passwordProgressMap: { [key: string]: 'success' | 'normal' | 'exception' } = {
    ok: 'success',
    pass: 'normal',
    pool: 'exception'
  };
  visible = false;
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
    if (control.value !== control.parent!.get('passnew')!.value) {
      return { equar: true };
    }
    return null;
  }
}
