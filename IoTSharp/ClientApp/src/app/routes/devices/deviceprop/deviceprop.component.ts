import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';
import { MyValidators } from '../../util/myvalidators';

@Component({
  selector: 'app-deviceprop',
  templateUrl: './deviceprop.component.html',
  styleUrls: ['./deviceprop.component.less']
})
export class DevicepropComponent implements OnInit {
  @Input()
  params: any = {
    id: Guid.EMPTY,
    customerId: Guid.EMPTY
  };
  submitting: boolean;
  avatarUrl?: string;
  constructor(private http: _HttpClient, private fb: FormBuilder, private msg: NzMessageService, private drawerRef: NzDrawerRef<string>) { }
  form!: FormGroup;
  ngOnInit(): void {
    const { ValidField } = MyValidators;
    this.form = this.fb.group({
      keyName: [null, [Validators.required, ValidField]],
      type: [null, [Validators.required]],
      dataSide: [null, [Validators.required]],
      dataValue: [null, []],
      deviceId: [this.params.id, []]
    });
  }

  close(): void {
    this.drawerRef.close(this.params);
  }
  submit() {
    this.http.post<appmessage<any>>('api/Devices/' + this.params.id + '/AddAttribute', this.form.value).subscribe(
      {

        next: next => {
          if (next.code === 10000) {
            this.msg.create('success', '新增属性成功');
            this.drawerRef.close(this.params);
          } else {
            this.msg.create('error', next.msg);
          }
        },
        error: error => {
          this.msg.create('error', '新增属性失败');
          this.drawerRef.close(this.params);
        },
        complete: () => { }
      }
    );
  }
}
