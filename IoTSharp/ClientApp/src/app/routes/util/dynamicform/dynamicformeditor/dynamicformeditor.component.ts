import { _HttpClient } from '@delon/theme';
import { Component, OnInit, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { AppMessage } from 'src/app/routes/common/AppMessage';
@Component({
  selector: 'app-dynamicformeditor',
  templateUrl: './dynamicformeditor.component.html',
  styleUrls: ['./dynamicformeditor.component.less'],
})
export class DynamicformeditorComponent implements OnInit {
  isManufactorLoading: Boolean = false;

  optionList: any;
  @Input() id: Number = -1;
  title: string = '';
  loading = false;
  avatarUrl?: string;
  constructor(
    private _router: ActivatedRoute,
    private router: Router,
    private _formBuilder: FormBuilder,
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
  ) {}
  form!: FormGroup;

  submitting = false;
  ngOnInit() {
    this.form = this.fb.group({
      formName: [null, [Validators.required]],
      formId: [0, []],
      formDesc: [null, []],
    });

    if (this.id !== -1) {
      this._httpClient.get<AppMessage>('api/dynamicforminfo/get?id=' + this.id).subscribe(
        (x) => {
          this.form.patchValue(x.data);
        },
        (y) => {},
        () => {},
      );
    }
  }

  submit() {
    this.submitting = true;
    var uri = this.id > 0 ? 'api/dynamicforminfo/update' : 'api/dynamicforminfo/save';
    this._httpClient.post(uri, this.form.value).subscribe(
      (x) => {
        this.submitting = false;
        this.msg.create('success', '表单保存成功');
        this.close();
      },
      (y) => {
        this.submitting = false;
        this.msg.create('error', '表单保存失败');
        this.close();
      },
      () => {

          this.drawerRef.close(this.id);},
    );
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
