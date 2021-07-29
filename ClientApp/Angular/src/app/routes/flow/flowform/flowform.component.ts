import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AppMessage } from '../../common/AppMessage';

@Component({
  selector: 'app-flowform',
  templateUrl: './flowform.component.html',
  styleUrls: ['./flowform.component.less'],
})
export class FlowformComponent implements OnInit {
  title: string = '';
  loading = false;
  @Input() id: number = -1;
  form!: FormGroup;
  constructor(
    private _router: ActivatedRoute,
    private router: Router,
    private _formBuilder: FormBuilder,
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
  ) {}

  submitting = false;
  ngOnInit() {
    this.form = this.fb.group({
      name: [null, [Validators.required]],
      ruleDesc: [null, []],
      ruleId: [0, []],
    });

    if (this.id !== -1) {
      this._httpClient.get<AppMessage>('api/rules/get?id=' + this.id).subscribe(
        (x) => {
          this.form.patchValue(x.result);
        },
        (y) => {},
        () => {},
      );
    }
  }

  submit() {
    this.submitting = true;
    var uri = this.id !== -1 ? 'api/rules/update' : 'api/rules/save';
    if (this.form.value.id === '') {
    }
    this._httpClient.post(uri, this.form.value).subscribe(
      (x) => {
        this.submitting = false;
      },
      (y) => {
        this.submitting = false;
      },
      () => {},
    );
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
