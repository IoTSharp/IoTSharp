import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Guid } from 'guid-typescript';
import { STChange, STColumn, STColumnBadge, STColumnTag, STComponent, STPage, STReq, STRes } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { ActivatedRoute, Router } from '@angular/router';
import { map, mergeMap } from 'rxjs/operators';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { CommonDialogSevice } from '../../util/commonDialogSevice';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-ruledevice',
  templateUrl: './ruledevice.component.html',
  styleUrls: ['./ruledevice.component.less']
})
export class RuledeviceComponent implements OnInit {

  @Input() id: any = Guid.EMPTY;

  @ViewChild('st', { static: true })
  st!: STComponent;

  url = '';

  q: {
    pi: number;
    ps: number;
    Name: string;

    sorter: string;
    status: number | null;
  } = {
      pi: 0,
      ps: 10,
      Name: '',
      sorter: '',
      status: null
    };
  req: STReq = { method: 'GET', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows'
    }
  };
  expandForm = false;
  page: STPage = {
    front: false,
    total: true,
    show: true,
    zeroIndexed: true
  };


  BADGE: STColumnBadge = {
    true: { text: '在线', color: 'success' },
    false: { text: '离线', color: 'error' }
  };
  TAG: STColumnTag = {
    AccessToken: { text: 'AccessToken', color: 'green' },
    X509Certificate: { text: 'X509Certificate', color: 'blue' }
  };

  DeviceTAG: STColumnTag = {
    Device: { text: '设备', color: 'green' },
    Gateway: { text: '网关', color: 'blue' }
  };


  columns: STColumn[] = [
    { title: '', index: 'device', type: 'checkbox' },
    {
      title: '名称', index: 'name', render: 'name', type: 'link', click: (item: any) => {
        this.showdeviceDetail(item.id)
      }
    },
    { title: '设备类型', index: 'deviceType', type: 'tag', tag: this.DeviceTAG },
    { title: '最后活动时间', index: 'lastActive', type: 'date' },
    { title: '在线状态', index: 'online', type: 'badge', badge: this.BADGE },
    {
      title: '操作',
      type: 'link',
      render: 'download',
      buttons: [
        {
          acl: 110,
          text: '删除',
          pop: {
            title: '确认删除设备绑定?',
            okType: 'danger',
            icon: 'warning'
          },
          click: (item: any) => {
            this.remove(item.id);
          }
        }
      ]
    }
  ];

  constructor(private _router: ActivatedRoute,
    private http: _HttpClient,
    private settingService: SettingsService,
    private drawerService: NzDrawerService,
    private commonDialogSevice: CommonDialogSevice) { }

  ngOnInit(): void {


    this._router.queryParams
      .pipe(
        map(x => {
          this.id = x['id'];
          this.url = 'api/rules/getRuleDevices?ruleId=' + this.id;
        })
      )
      .subscribe();
  }


  remove(deviceid) {
    this.http.get<appmessage<any>>('api/Rules/DeleteDeviceRules?deviceId=' + deviceid + '&ruleId=' + this.id).subscribe(
      {
        next: next => {
          this.getData();
        },
        error: error => { },
        complete: () => { }
      }
    );
  }

  reset() {
    this.q = {
      pi: 0,
      ps: 10,
      Name: '',
      sorter: '',
      status: null
    }
  }

  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }

  showdeviceDetail(id) {
    this.commonDialogSevice.showDeviceDialog(id);
  }


}
