import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { AppMessage } from '../../common/AppMessage';
import { devicemodelcommandparam } from '../devicemodelcommandparam';

@Component({
  selector: 'app-devicemodelcommand',
  templateUrl: './devicemodelcommand.component.html',
  styleUrls: ['./devicemodelcommand.component.less']
})
export class DevicemodelcommandComponent implements OnInit {

  title: string = '';
  loading = false;
  @Input() id: devicemodelcommandparam ;
  form!: FormGroup;
  constructor(
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private drawerRef: NzDrawerRef<string>,
  ) {}
  submitting = false;
  ngOnInit(): void {

    this.form = this.fb.group({
      commandTitle: [null, [Validators.required]],
      commandI18N: [null, []],
      commandName: [null, []],
      commandParams: [null, []],
      commandType: [null, []],
      commandTemplate: [null, []],
      commandId: [Guid.EMPTY, []],
      deviceModelId: [null, []],
    });
    if (this.id.commandId !== Guid.EMPTY) {
      this._httpClient.get<AppMessage>('api/deviceModel/getCommand?id=' + this.id.commandId).subscribe(
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
    var uri = this.id.commandId !== Guid.EMPTY ? 'api/deviceModel/updateCommand' : 'api/deviceModel/saveCommand';
    if (this.form.value.id === '') {

    }

    this.form.value.deviceModelId=this.id.deviceModelId;
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
