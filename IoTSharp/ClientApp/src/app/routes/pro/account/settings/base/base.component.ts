import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { zip } from 'rxjs';

interface ProAccountSettingsUser {
  email: string;
  name: string;
  profile: string;
  country: string;
  address: string;
  phone: string;
  avatar: string;
  geographic: {
    province: {
      key: string;
    };
    city: {
      key: string;
    };
  };
}

interface ProAccountSettingsCity {
  name: string;
  id: string;
}

@Component({
  selector: 'app-account-settings-base',
  templateUrl: './base.component.html',
  styleUrls: ['./base.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProAccountSettingsBaseComponent implements OnInit {

  form: FormGroup;
  constructor(private http: _HttpClient,
     private cdr: ChangeDetectorRef,
      private msg: NzMessageService,
      private fb: FormBuilder,) {

    this.form = fb.group({
      email: ['', [Validators.required, Validators.email]],
      name: ['', [Validators.required, Validators.email]],
      phonenumber: ['', [Validators.required, Validators.pattern(/^1\d{10}$/)]],
    });

  }
  avatar = '';
  userLoading = false;
  user!:any;

  // #region geo


  provinces: ProAccountSettingsCity[] = [];
  cities: ProAccountSettingsCity[] = [];




  ngOnInit(): void {
   
    
    this.http.get('api/Account/MyInfo').subscribe(next=>{
      this.userLoading=false;
  
this.form.patchValue(next.data)
    },error=>{},()=>{});
  }

 

  // #endregion

  save(): void {

    Object.keys(this.form.controls).forEach((key) => {
      this.form.controls[key].markAsDirty();
      this.form.controls[key].updateValueAndValidity();
    });
    const data = this.form.value;

    if (this.form.invalid) {
      return;
    }
    this.http.put('api/Account/ModifyMyInfo', data).subscribe((x) => {
      if(x.code===10000){


      }
    })
  }
}
