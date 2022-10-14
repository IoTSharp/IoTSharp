import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { concat, map } from 'rxjs';
import { appmessage } from 'src/app/models/appmessage';
import { devicemodel, deviceitem } from 'src/app/models/deviceitem';

@Component({
  selector: 'app-produceform',
  templateUrl: './produceform.component.html',
  styleUrls: ['./produceform.component.less']
})
export class ProduceformComponent implements OnInit {
  fullScreen = false;


  @Input() id = Guid.EMPTY;
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
    this.form = this.fb.group({
      id: [Guid.EMPTY, []],
      name: [null, [Validators.required]],
      icon: ['', []],
      gatewayType: ['Unknow', []],
      defaultTimeout: [300, []],
      gatewayConfigurationJson: ['', []],
      gatewayConfigurationName: ['', []],
      gatewayConfiguration: ['', []],
      defaultIdentityType: ['', []],
      defaultDeviceType: ['', []],
      description: ['', []],
    });


    if (this.id != Guid.EMPTY) {
      this._httpClient.get('api/produces/get?id=' + this.id).pipe(
        map(x => {
          this.form.patchValue(x.data);

          if (x.data.gatewayType === 'Customize') {
            this.form.patchValue({ gatewayConfigurationJson: this.form.value.gatewayConfiguration });
          } else if (x.data.gatewayType === 'Unknow') {
            this.form.patchValue({ gatewayConfigurationName: '' });
            this.form.patchValue({ gatewayConfigurationJson: '' });

          } else {
            this.form.patchValue({ gatewayConfigurationName: this.form.value.gatewayConfiguration });
          }


        })
      ).subscribe();
    }
  }

  submit() {
    this.submitting = true;
    if (this.form.value.gatewayType === 'Customize') {
      this.form.patchValue({ gatewayConfiguration: this.form.value.gatewayConfigurationJson });
    } else if (this.form.value.gatewayType === 'Unknow') {
      this.form.patchValue({ gatewayConfiguration: '' });
    } else {
      this.form.patchValue({ gatewayConfiguration: this.form.value.gatewayConfigurationName });
    }
    if (this.id == Guid.EMPTY) {
      this._httpClient.post('api/produces/save', this.form.value).subscribe(
        {

          next: next => {
            this.submitting = false;

            if (next.code === 10000) {
              this.msg.create('success', '产品新增成功');
              this.close();
            }

          },
          error: error => {
            this.msg.create('error', '产品新增失败');
            this.close();
          },
          complete: () => { }

        }
      );
    } else {
      this._httpClient.put('api/produces/update', this.form.value).subscribe(

        {
          next: next => {
            this.submitting = false;
            if (next.code === 10000) {
              this.msg.create('success', '产品修改成功');
              this.close();
            }
          },
          error: error => {
            this.msg.create('error', '产品修改失败');
            this.close();
          },
          complete: () => { }

        }

      );
    }
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
