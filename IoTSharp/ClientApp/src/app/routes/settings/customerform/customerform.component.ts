import { Component, OnInit, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';
import { MyValidators } from '../../util/myvalidators';

@Component({
  selector: 'app-customerform',
  templateUrl: './customerform.component.html',
  styleUrls: ['./customerform.component.less']
})
export class CustomerformComponent implements OnInit {
  isManufactorLoading: Boolean = false;

  optionList: any;
  @Input()
  params: any = {
    id: '-1',
    tenantId: '-1'
  };

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
      email: [null, [email]],
      phone: [null, [mobile]],
      country: [null, []],
      province: [null, []],
      city: [null, []],
      street: [null, []],
      address: [null, []],
      zipCode: [null, [Validators.required, zip]],
      tenantID: [this.params.tenantId, []]
    });

    if (this.params.id !== '-1') {
      this._httpClient.get('api/Customers/' + this.params.id).subscribe(
        {
          next: x => {
            this.form.patchValue(x.data);
          },
          error: () => { },
          complete: () => { }
        }
      );
    }
  }

  submit() {
    this.submitting = true;

    if (this.params.id !== Guid.EMPTY) {
      this._httpClient.put<appmessage<any>>('api/Customers/' + this.form.value.id, this.form.value).subscribe(
        {
          next: next => {
            this.submitting = false;
            this.msg.create('success', '客户保存成功');
            this.close();
          },
          error: error => {
            this.submitting = false;
            this.msg.create('error', '客户保存失败');
          },
          complete: () => {
            this.submitting = false;
          }

        }
      );
    } else {
      this._httpClient.post<appmessage<any>>('api/Customers', this.form.value).subscribe(
        {
          next: next => {
            this.submitting = false;
          },
          error: error => { },
          complete: () => {
            this.submitting = false;

            this.msg.create('success', '客户保存成功');
            this.close();
          }
        }
      );
    }
  }
  close(): void {
    this.drawerRef.close(this.params);
  }
}
