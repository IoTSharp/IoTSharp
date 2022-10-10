import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TransferItem } from 'ng-zorro-antd/transfer';
import { debounceTime, zip } from 'rxjs';
import { appmessage } from 'src/app/models/appmessage';
import { attributeitem, telemetryitem } from 'src/app/models/deviceitem';
import { option } from 'src/app/models/option';
@Component({
  selector: 'app-assetentityform',
  templateUrl: './assetentityform.component.html',
  styleUrls: ['./assetentityform.component.less']
})
export class AssetentityformComponent implements OnInit {
  @Input() id: any = Guid.EMPTY;
  config: any = { language: 'zh_CN', height: 250 };
  title: string = '';
  devicename: string = '';
  loading = false;
  form!: FormGroup;
  submitting = false;
  listattr: TransferItem[] = [];
  listtemp: TransferItem[] = [];
  deviceid: string;
  devices: any[] = [];
  constructor(
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
    private http: _HttpClient,
    private settingService: SettingsService
  ) {}

  ngOnInit() {
    this.form = this.fb.group({
      deviceid: [Guid.EMPTY, []],
      devicename: ['', []]
    });
  }
  compare = (o1: option | string, o2: option): boolean => {
    if (o1) {
      return typeof o1 === 'string' ? o1 === o2.label : o1.value === o2.value;
    } else {
      return false;
    }
  };
  oninput($event) {
    var element = $event.target as HTMLInputElement;
    this.http
      .get('api/Devices/Customers', {
        name: element?.value ?? '',
        offset: 0,
        limit: -1,
        pi: 0,
        ps: 10,
        customerId: this.settingService.user['customer']
      })
      .pipe(debounceTime(500))
      .subscribe({
        next: next => {
          this.devices = [
            ...next.data.rows.map(x => {
              return { id: x.id, name: x.name };
            })
          ];
        },
        error: error => {},
        complete: () => {}
      });
  }
  onchange($event) {
    this.deviceid = this.devices.find(c => c.name == $event)?.id;
    if (this.deviceid) {
      zip(
        this.http.get<appmessage<attributeitem[]>>('api/Devices/' + this.deviceid + '/AttributeLatest'),
        this.http.get<appmessage<telemetryitem[]>>('api/Devices/' + this.deviceid + '/TelemetryLatest')
        //   this.http.get<appmessage<devicemodelcommand[]>>('api/deviceModel/getCommandsByDevice?id=' + $events.expand?.id ),
      ).subscribe(
        ([
          attributes,
          telemetries
          //  commands
        ]) => {
          this.listattr = attributes.data.map(c => {
            return { key: c.keyName, title: c.keyName, tag: { keyName: c.keyName, dateTime: c.dateTime, value: c.value } };
          });

          this.listtemp = telemetries.data.map(c => {
            return { key: c.keyName, title: c.keyName, tag: { keyName: c.keyName, dateTime: c.dateTime, value: c.value } };
          });
        }
      );
    }
  }

  submit() {
    this.submitting = true;

    var value = this.form.value;
    value['attrs'] = this.listattr.filter(c => c.direction == 'right').map(c => c['tag']);
    value['temps'] = this.listtemp.filter(c => c.direction == 'right').map(c => c['tag']);
    value['assetid'] = this.id;

    this.http.post<appmessage<any>>('api/asset/adddevice', value).subscribe(
      x => {
        this.submitting = false;

        if (x.code == 10000) {
          this.msg.create('success', '资产导入成功');
          this.close();
        } else {
          this.msg.create('error', '资产导入失败');
          this.close();
        }
      },
      () => {
        this.msg.create('error', '资产导入失败');
        this.close();
      },
      () => {}
    );
  }
  close(): void {
    this.drawerRef.close(this.id);
  }
}
