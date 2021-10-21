import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-account-settings-security',
  templateUrl: './security.component.html',
  styleUrls: ['./security.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProAccountSettingsSecurityComponent implements OnInit{  form: FormGroup;
  constructor(public msg: NzMessageService,private http: _HttpClient,      private fb: FormBuilder,) {

    this.form = fb.group({
      pass: ['', [Validators.required, ]],
      passnew: ['', [Validators.required, Validators.minLength(6), ProAccountSettingsSecurityComponent.checkPassword.bind(this)]],
      passnewsecond: ['', [Validators.required, Validators.minLength(6), ProAccountSettingsSecurityComponent.passwordEquar]],
    });


  }
  ngOnInit(): void {



  }
  save(){    const data = this.form.value;
    if (this.form.invalid) {
      return;
    }
    this.http.put('api/Account/ModifyMyPassword', data).subscribe((x) => {
      if(x.code===10000){


      }
    })


    
  } status = 'pool';
  progress = 0;
  passwordProgressMap: { [key: string]: 'success' | 'normal' | 'exception' } = {
    ok: 'success',
    pass: 'normal',
    pool: 'exception',
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
