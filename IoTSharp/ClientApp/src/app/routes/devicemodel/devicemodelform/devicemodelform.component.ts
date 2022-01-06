import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { AppMessage } from '../../common/AppMessage';

@Component({
  selector: 'app-devicemodelform',
  templateUrl: './devicemodelform.component.html',
  styleUrls: ['./devicemodelform.component.less']
})
export class DevicemodelformComponent implements OnInit {
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
  ngOnInit(): void {

    this.form = this.fb.group({
      modelName: [null, [Validators.required]],
      modelDesc: [null, []],
      deviceModelId: [Guid.EMPTY, []],
    });
    if (this.id !== Guid.EMPTY) {
      this._httpClient.get<AppMessage>('api/deviceModel/get?id=' + this.id).subscribe(
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
    var uri = this.id !== Guid.EMPTY ? 'api/deviceModel/update' : 'api/deviceModel/save';
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
