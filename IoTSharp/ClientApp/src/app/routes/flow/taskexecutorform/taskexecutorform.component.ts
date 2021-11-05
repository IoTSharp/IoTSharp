import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AppMessage } from '../../common/AppMessage';

@Component({
  selector: 'app-taskexecutorform',
  templateUrl: './taskexecutorform.component.html',
  styleUrls: ['./taskexecutorform.component.less']
})
export class TaskexecutorformComponent implements OnInit {

  title: string = '';
  loading = false;
  @Input() id: string ;
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
      executorName: [null, [Validators.required]],
      executorDesc: [null, []],
      typeName: [null, []],
      defaultConfig: [null, []],
      executorId: [Guid.EMPTY, []],
    });

    if (this.id !== Guid.EMPTY) {
      this._httpClient.get<AppMessage>('api/rules/getexecutor?id=' + this.id).subscribe(
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
    var uri = this.id !== Guid.EMPTY ? 'api/rules/updateexecutor' : 'api/rules/addexecutor';
    if (this.form.value.id === '') {
    }
    this._httpClient.post(uri, this.form.value).subscribe(
      (x) => {
        this.submitting = false;
        this.msg.create('success', '执行器保存成功');
      },
      (y) => {
        this.submitting = false; this.msg.create('success', '执行器保存成功');
      },
      () => { 
        this.drawerRef.close(this.id);},
    );
  }

   


  

  close(): void {
    this.drawerRef.close(this.id);
  }
}
