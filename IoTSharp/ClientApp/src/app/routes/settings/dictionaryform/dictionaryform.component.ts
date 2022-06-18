import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-dictionaryform',
  templateUrl: './dictionaryform.component.html',
  styleUrls: ['./dictionaryform.component.less']
})
export class DictionaryformComponent implements OnInit {
  @Input() id: Number = -1;

  optionList: any = [];

  title: string = '';

  constructor(
    private _httpClient: _HttpClient,
    private msg: NzMessageService,
    private fb: FormBuilder,
    private drawerRef: NzDrawerRef<string>
  ) { }
  form!: FormGroup;

  submitting = false;
  ngOnInit() {
    this._httpClient.get('api/dictionarygroup/index').subscribe(
      x => {
        this.optionList = x.Result;
      },
      y => { },
      () => { }
    );
    this.form = this.fb.group({
      DictionaryId: [0, []],
      DictionaryName: ['', [Validators.required]],
      DictionaryValue: ['', []],
      Dictionary18NKeyName: ['', []],
      DictionaryGroupId: [0, []],
      DictionaryDesc: ['', []],
      DictionaryColor: ['', []],
      DictionaryIcon: ['', []]
    });

    if (this.id !== -1) {
      this._httpClient.get<appmessage<any>>('api/dictionary/get?id=' + this.id).subscribe(
        {
          next: next => {
            this.form.patchValue(next.data);
          },
          error: error => { },
          complete: () => { }
        }
      );
    }
  }

  submit() {
    this.submitting = true;
    var uri = this.id > 0 ? 'api/dictionary/update' : 'api/dictionary/save';
    this._httpClient.post<appmessage<any>>(uri, this.form.value).subscribe(
      {

        next: next => {
          this.submitting = false;
          //   this.router.navigateByUrl('manage/uri/userlist');
        },
        error: error => {
          this.submitting = false;
        },
        complete: () => { }
      }
    );
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
