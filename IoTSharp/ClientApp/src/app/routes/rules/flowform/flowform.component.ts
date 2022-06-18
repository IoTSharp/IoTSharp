import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { appmessage } from 'src/app/models/appmessage';
@Component({
  selector: 'app-flowform',
  templateUrl: './flowform.component.html',
  styleUrls: ['./flowform.component.less']
})
export class FlowformComponent implements OnInit {
  title: string = '';
  loading = false;
  @Input() id: string;
  form!: FormGroup;
  constructor(private _httpClient: _HttpClient, private fb: FormBuilder, private drawerRef: NzDrawerRef<string>) { }

  submitting = false;
  ngOnInit() {
    this.form = this.fb.group({
      name: [null, [Validators.required]],
      ruleDesc: [null, []],
      mountType: [null, [Validators.required]],
      ruleId: [Guid.EMPTY, []]
    });

    if (this.id !== Guid.EMPTY) {
      this._httpClient.get<appmessage<any>>('api/rules/get?id=' + this.id).subscribe(
        x => {
          x.data.mountType = x.data.mountType + '';
          this.form.patchValue(x.data);
        },
        () => { },
        () => { }
      );
    }
  }

  submit() {
    this.submitting = true;
    var uri = this.id !== Guid.EMPTY ? 'api/rules/update' : 'api/rules/save';
    if (this.form.value.id === '') {
    }
    this._httpClient.post<appmessage<any>>(uri, this.form.value).subscribe(
      {
        next: next => {
          this.submitting = false;
        },
        error: error => {
          this.submitting = false;
        },
        complete: () => {
          this.drawerRef.close(this.id);
        }


      }
    );
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
