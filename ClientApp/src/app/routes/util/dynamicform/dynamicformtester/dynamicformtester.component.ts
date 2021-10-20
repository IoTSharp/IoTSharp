import { ChangeDetectorRef, Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  SFComponent,
  SFRadioWidgetSchema,
  SFDateWidgetSchema,
  SFSliderWidgetSchema,
  SFSelectWidgetSchema,
  SFTransferWidgetSchema,
  SFTreeSelectWidgetSchema,
  SFUploadWidgetSchema,
  SFSchema,
} from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map } from 'rxjs/operators';
import { AppMessage } from 'src/app/routes/common/AppMessage';
import { CodeviewComponent } from '../../code/codeview/codeview.component';
import { DynamicformresultviewComponent } from '../dynamicformresultview/dynamicformresultview.component';
import { DynamicformviewComponent } from '../dynamicformview/dynamicformview.component';

@Component({
  selector: 'app-dynamicformtester',
  templateUrl: './dynamicformtester.component.html',
  styleUrls: ['./dynamicformtester.component.less'],
})
export class DynamicformtesterComponent implements OnInit {
  @Input() id: Number = -1;
  @ViewChild('dfv', { static: true })
  dfv: DynamicformviewComponent;
  @ViewChild('dfr', { static: true })
  dfr: DynamicformresultviewComponent;
  @ViewChild('dcv', { static: true })
  dcv: CodeviewComponent;
  constructor(
    private _router: ActivatedRoute,
    private router: Router,
    private _formBuilder: FormBuilder,
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.dfv.id = this.id;

    this._httpClient.get<AppMessage>('api/DynamicFormInfo/get?id=' + this.id).subscribe(
      (next) => {
        this.dcv.code = next.data.modelClass;
      },
      (error) => {},
      () => {},
    );
  }

  onsubmit(data): void {
    this.dfr.result = data;
  }
}
