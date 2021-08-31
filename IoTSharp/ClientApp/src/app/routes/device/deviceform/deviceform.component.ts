import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AppMessage } from '../../common/AppMessage';
import { MyValidators } from '../../common/validators/MyValidators';

@Component({
  selector: 'app-deviceform',
  templateUrl: './deviceform.component.html',
  styleUrls: ['./deviceform.component.less'],
})
export class DeviceformComponent implements OnInit {
  isManufactorLoading: Boolean = false;
  optionList: any;
  @Input() params: any = {
    id: '-1',
    customerId: '-1',
  };
  nodes = [];
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
    const { nullbigintid } = MyValidators;
    this.form = this.fb.group({
      name: [null, [Validators.required]],
      deviceType: [null, [Validators.required]],
      customerId: [null, []],
      id: [Guid.create().toString(), []], //骗过验证
    });
    if (this.params.id !== '-1') {
      this._httpClient.get('api/Devices/' + this.params.id).subscribe(
        (x) => {
          this.form.patchValue(x.Result);
        },
        (y) => {},
        () => {},
      );
    }
  }

  submit() {
    this.submitting = true;

    if (this.params.id !== '-1') {
      this._httpClient.put('api/Devices', this.form.value).subscribe((x) => {
        this.submitting = false;
        this.drawerRef.close(this.params);
      });
    } else {
      this._httpClient.post('api/Devices', this.form.value).subscribe((x) => {
        this.submitting = false;
        this.drawerRef.close(this.params);
      });
    }
  }
  close(): void {
    this.drawerRef.close(this.params);
  }
}
