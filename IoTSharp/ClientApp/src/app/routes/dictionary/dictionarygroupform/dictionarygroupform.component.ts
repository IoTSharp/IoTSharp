import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';

import { AppMessage } from '../../common/AppMessage';

@Component({
  selector: 'app-dictionarygroupform',
  templateUrl: './dictionarygroupform.component.html',
  styleUrls: ['./dictionarygroupform.component.less'],
})
export class DictionarygroupformComponent implements OnInit {
  @Input() id: Number = -1;

  title: string = '';

  avatarUrl?: string;
  constructor(
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private drawerRef: NzDrawerRef<string>,
  ) {}
  form!: FormGroup;

  submitting = false;
  ngOnInit() {
    this.form = this.fb.group({
      DictionaryGroupId: [0, []],
      DictionaryGroupName: ['', [Validators.required]],
      DictionaryGroupValueType: ['', []],
      DictionaryGroupDesc: ['', []],
    });

    if (this.id !== -1) {
      this._httpClient.get<AppMessage>('api/dictionarygroup/get?id=' + this.id).subscribe(
        (x) => {
          this.form.patchValue(x.data);
        },
        () => {},
        () => {},
      );
    }
  }

  submit() {
    this.submitting = true;
    var uri = this.id > 0 ? 'api/dictionarygroup/update' : 'api/dictionarygroup/save';
    this._httpClient.post(uri, this.form.value).subscribe(
      () => {
        this.submitting = false;
        //   this.router.navigateByUrl('manage/uri/userlist');
      },
      () => {
        this.submitting = false;
      },
      () => {},
    );
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
