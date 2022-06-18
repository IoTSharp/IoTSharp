import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { MyValidators } from '../../util/myvalidators';

@Component({
  selector: 'app-tenantform',
  templateUrl: './tenantform.component.html',
  styleUrls: ['./tenantform.component.less']
})
export class TenantformComponent implements OnInit {
  isManufactorLoading: Boolean = false;

  optionList: any;
  @Input() id: string = '-1';

  nodes = [];

  title: string = '';

  loading = false;
  avatarUrl?: string;
  constructor(
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>
  ) { }
  form!: FormGroup;
  submitting = false;
  ngOnInit() {
    const { zip, email, mobile } = MyValidators;
    this.form = this.fb.group({
      name: [null, [Validators.required]],
      id: [Guid.EMPTY, []],
      eMail: [null, [email]],
      phone: [null, [mobile]],
      country: [null, []],
      province: [null, []],
      city: [null, []],
      street: [null, []],
      address: [null, []],
      zipCode: [null, [Validators.required, zip]]
    });

    if (this.id !== Guid.EMPTY) {
      this._httpClient.get('api/Tenants/' + this.id).subscribe(
        {
          next: x => {
            this.form.patchValue(x.data);
          },
          error: error => { },
          complete: () => { }
        }
      );
    }
  }

  submit() {
    this.submitting = true;
    if (this.id !== Guid.EMPTY) {
      this._httpClient.put('api/Tenants/' + this.form.value.id, this.form.value).subscribe(
        {
          next: next => {
            this.submitting = false;
            this.msg.create('success', '租户保存成功');
            this.close();
          },
          error: error => {
            this.submitting = false;
            this.msg.create('error', '租户保存失败');
          },
          complete: () => { }
        }
      );
    } else {
      this._httpClient.post('api/Tenants', this.form.value).subscribe(
        {

          next: next => {
            this.submitting = false;
            this.msg.create('success', '租户保存成功');
            this.close();
          },
          error: error => { },
          complete: () => { }
        }
      );
    }
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
