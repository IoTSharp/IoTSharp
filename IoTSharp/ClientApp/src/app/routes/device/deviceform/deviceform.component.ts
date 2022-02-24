import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { concat } from 'rxjs';
import { map } from 'rxjs/operators';
import { appmessage, AppMessage } from '../../common/AppMessage';
import { MyValidators } from '../../common/validators/MyValidators';
import { devicemodel } from '../../devicemodel/devicemodelcommandparam';
import { deviceitem } from '../devicelist/devicelist.component';

@Component({
  selector: 'app-deviceform',
  templateUrl: './deviceform.component.html',
  styleUrls: ['./deviceform.component.less'],
})
export class DeviceformComponent implements OnInit {
  isManufactorLoading: Boolean = false;
  optionList: any;
  @Input() params: any = {
    id: Guid.EMPTY,
    customerId: '-1',
  };
  nodes = [];
  title: string = '';
  loading = false;
  avatarUrl?: string;
  constructor(
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
  ) {}
  form!: FormGroup;
  submitting = false;


  devicemodel:devicemodel[]=[];

  data: deviceitem = {
    name: '',
    deviceType: '',
    customerId: '',
    id: Guid.EMPTY,
    identityType: '',
  };
  ngOnInit() {
    
 
    this.form = this.fb.group({
      name: [null, [Validators.required,Validators.pattern(/^(\s+\S+\s*)*(?!\s).*$/)]],
      deviceType: [null, [Validators.required]],
      customerId: [null, []],
      // deviceModelId: [null, []],
      timeout: [300, []],
      id: [Guid.EMPTY, []],
      identityType: [Guid.EMPTY, []],
    });

    this._httpClient.post('api/deviceModel/index',{offset:0, limit:100 }).subscribe(next=>{
      this.devicemodel=next.data.rows;
     },error=>{
  
     },()=>{})
  

    if (this.params.id !== Guid.EMPTY) {
      concat(
        this._httpClient.get<appmessage<deviceitem>>('api/Devices/' + this.params.id).pipe(
          map((x) => {
            this.data = x.data;
          }),
        ),
        this._httpClient.get('api/Devices/' + this.params.id + '/Identity').pipe(
          map((x) => {
            this.data.identityType = x.data.identityType;

            this.form.patchValue(this.data);
          }),
        ),
      ).subscribe();

      // this._httpClient.get('api/Devices/' + this.params.id).subscribe(
      //   (x) => {
      //     // this.form.patchValue(x.data);
      //   },
      //   () => {},
      //   () => {},
      // );
    }
  }
  createcert($event) {
    this._httpClient.get('api/Devices/' + this.params.id + '/CreateX509Identity').subscribe(
      (next) => {
        if (next.code === 10000) {
          this.msg.create('success', '证书生成成功');
        }else{
          this.msg.create('error', '证书生成失败:' + next.msg);
        }

      },
      (error) => {
        this.msg.create('error', '证书生成失败');
      },
      () => {},
    );
  }
  submit() {
    this.submitting = true;

    if (this.params.id == Guid.EMPTY) {
      this._httpClient.post('api/Devices', this.form.value).subscribe(
        () => {
          this.submitting = false;
          this.msg.create('success', '设备新增成功');
          this.close();
        },
        () => {
          this.msg.create('error', '设备新增失败');
          this.close();
        },
        () => {},
      );
    } else {
      this._httpClient.put('api/Devices/' + this.params.id, this.form.value).subscribe(
        () => {
          this.submitting = false;
          this.msg.create('success', '设备修改成功');
          this.close();
        },
        () => {
          this.msg.create('error', '设备修改失败');
          this.close();
        },
        () => {},
      );
    }
  }
  close(): void {
    this.drawerRef.close(this.params);
  }
}
