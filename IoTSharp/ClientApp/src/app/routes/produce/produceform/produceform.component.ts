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

  isManufactorLoading: Boolean = false;
  optionList: any;
  @Input() id=Guid.EMPTY;
  nodes = [];
  title: string = '';
  loading = false;
  avatarUrl?: string;
  constructor(
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>
  ) {}
  form!: FormGroup;
  submitting = false;

  devicemodel: devicemodel[] = [];

  data: deviceitem = {
    name: '',
    deviceType: '',
    customerId: '',
    id: Guid.EMPTY,
    identityType: ''
  };
  ngOnInit() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.pattern(/^(\s+\S+\s*)*(?!\s).*$/)]],
      description: ['', []],
      defaultTimeout: [300, []],
      id: [Guid.EMPTY, []],
      defaultIdentityType: ['', []]
    });

    this._httpClient.get('api/produce/get/' + this.id).pipe(
      map(x => {
        this.form.patchValue(this.data);
      })
    ).subscribe();


     
    
  }

  submit() {
    this.submitting = true;

    if (this.id == Guid.EMPTY) {
      this._httpClient.post('api/produce', this.form.value).subscribe(
        () => {
          this.submitting = false;
          this.msg.create('success', '产品新增成功');
          this.close();
        },
        () => {
          this.msg.create('error', '产品新增失败');
          this.close();
        },
        () => {}
      );
    } else {
      this._httpClient.put('api/produce/' + this.id, this.form.value).subscribe(
        () => {
          this.submitting = false;
          this.msg.create('success', '产品修改成功');
          this.close();
        },
        () => {
          this.msg.create('error', '产品修改失败');
          this.close();
        },
        () => {}
      );
    }
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
