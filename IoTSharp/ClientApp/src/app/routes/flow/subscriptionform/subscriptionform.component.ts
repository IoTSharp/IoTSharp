import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { AppMessage } from '../../common/AppMessage';

@Component({
  selector: 'app-subscriptionform',
  templateUrl: './subscriptionform.component.html',
  styleUrls: ['./subscriptionform.component.less']
})
export class SubscriptionformComponent implements OnInit {

  title: string = '';
  loading = false;
  @Input() id: string ;
  form!: FormGroup;
  constructor(
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private drawerRef: NzDrawerRef<string>,
  ) {}

  submitting = false;
  ngOnInit() {
    this.form = this.fb.group({
      eventName: [null, [Validators.required]],
      eventDesc: [null, []],
      eventNameSpace: [null, [Validators.required]],
      eventParam: [null, []],
     eventId: [Guid.EMPTY, []],
    });

    if (this.id !== Guid.EMPTY) {
      this._httpClient.get<AppMessage>('api/subscriptionevent/get?id=' + this.id).subscribe(
        (x) => {
          x.data.mountType= x.data.mountType+'';
          this.form.patchValue(x.data);
        },
        () => {},
        () => {},
      );
    }
  }

  submit() {
    this.submitting = true;
    var uri = this.id !== Guid.EMPTY ? 'api/subscriptionevent/update' : 'api/subscriptionevent/save';
    if (this.form.value.id === '') {
    }
    this._httpClient.post(uri, this.form.value).subscribe(
      () => {
        this.submitting = false;
      },
      () => {
        this.submitting = false;
      },
      () => { this.drawerRef.close(this.id);},
    );
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}


