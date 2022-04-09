import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { concat } from 'rxjs';
import { map } from 'rxjs/operators';
import { appmessage } from '../../common/AppMessage';
@Component({
  selector: 'app-assetform',
  templateUrl: './assetform.component.html',
  styleUrls: ['./assetform.component.less']
})
export class AssetformComponent implements OnInit {
  @Input() id: any = Guid.EMPTY;
  config:any={ language: 'zh_CN', height: 250,}
  title: string = '';
  loading = false;
  form!: FormGroup;
  submitting = false;

  cetd: any[] = [];
  cead: any[] = [];
  constructor(
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
  ) {}

  ngOnInit() {
    this.form = this.fb.group({
      name: [null, [Validators.required,Validators.pattern(/^(\s+\S+\s*)*(?!\s).*$/)]],
      assetType: [null, [Validators.required]],
      customerId: [null, []],
      description: [null, []],
      id: [Guid.EMPTY, []],

    });
    if (this.id !== Guid.EMPTY) {
      concat(
        this._httpClient.get<appmessage<any>>('api/asset/get?id=' + this.id).pipe(
          map((x) => {
            this.form.patchValue(x.data)
          }),
        ),
      ).subscribe();
    }
  }

  submit() {
    this.submitting = true;
    if (this.id == Guid.EMPTY) {
      this._httpClient.post('api/asset/save', this.form.value).subscribe(
        () => {
          this.submitting = false;
          this.msg.create('success', '资产新增成功');
          this.close();
        },
        () => {
          this.msg.create('error', '资产新增失败');
          this.close();
        },
        () => {},
      );
    } else {
      this._httpClient.put('api/asset/update', this.form.value).subscribe(
        () => {
          this.submitting = false;
          this.msg.create('success', '资产修改成功');
          this.close();
        },
        () => {
          this.msg.create('error', '资产修改失败');
          this.close();
        },
        () => {},
      );
    }
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
