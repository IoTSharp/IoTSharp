import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { STColumnBadge, STColumnTag, STReq, STRes, STPage, STComponent, STColumn, STChange } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { Subscription, map, interval, zip } from 'rxjs';
import { appmessage } from 'src/app/models/appmessage';
import { AssetentityformComponent } from '../assetentityform/assetentityform.component';
import { AssetrelationformComponent } from '../assetrelationform/assetrelationform.component';

@Component({
  selector: 'app-assetentitylist',
  templateUrl: './assetentitylist.component.html',
  styleUrls: ['./assetentitylist.component.less']
})
export class AssetentitylistComponent implements OnInit {
  @Input() id: any = Guid.EMPTY;

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

  url = '';
  obs: Subscription;
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

  cetd: any[] = [];
  cead: any[] = [];
  @ViewChild('st', { static: true })
  st!: STComponent;
  columns: STColumn[] = [
    { title: '', index: 'device', type: 'checkbox' },
    { title: '名称', index: 'name', render: 'name' },
    { title: '设备类型', index: 'deviceType', type: 'tag', tag: this.DeviceTAG },
    { title: '最后活动时间', index: 'lastActive', type: 'date' },
    { title: '在线状态', index: 'online', type: 'badge', badge: this.BADGE }
  ];

  childrencolumns: STColumn[] = [
    { title: '属性名称', index: 'keyName', render: 'name' },
    { title: '设备类型', index: 'dataSide' },
    { title: '设备类型', index: 'value' },
    { title: '最后活动时间', index: 'dateTime', type: 'date' },
    { title: '在线状态', index: 'keyName', type: 'badge' },
    { title: '操作', index: 'keyName' }
  ];

  constructor(
    private _router: ActivatedRoute,
    private http: _HttpClient,
    private drawerService: NzDrawerService,
    private settingService: SettingsService
  ) {}

  ngOnInit(): void {
    this._router.queryParams
      .pipe(
        map(x => {
          this.id = x['id'];
          this.url = 'api/asset/relations?assetid=' + this.id;
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    if (this.obs) {
      this.obs.unsubscribe();
    }
  }
  onchange($events: STChange) {
    if (this.obs) {
      this.obs.unsubscribe();
      this.obs = null;
    }
    switch ($events.type) {
      case 'expand':
        if ($events.expand.expand) {
          this.cead = [];
          this.cetd = [];
          this.cead = $events.expand?.attrs;
          this.cetd = $events.expand?.temps;
          this.obs = interval(2000).subscribe(async () => {
            zip(
              this.http.get<appmessage<any[]>>('api/Devices/' + $events.expand?.id + '/AttributeLatest'),
              this.http.get<appmessage<any[]>>('api/Devices/' + $events.expand?.id + '/TelemetryLatest')
              //   this.http.get<appmessage<devicemodelcommand[]>>('api/deviceModel/getCommandsByDevice?id=' + $events.expand?.id ),
            ).subscribe(
              ([
                attributes,
                telemetries

                //  commands
              ]) => {
                // console.log(this.cead);
                // console.log(telemetries);
                for (var i = 0; i < this.cead.length; i++) {
                  for (var j = 0; j < attributes.data.length; j++) {
                    if (this.cead[i]['keyName'] === attributes.data[j].keyName) {
                      switch (typeof attributes.data[j]['value']) {
                        case 'number':
                          if (this.cead[i]['value']) {
                            if (this.cead[i]['value'] > attributes.data[j]['value']) {
                              this.cead[i]['className'] = 'valdown';
                            } else if (this.cead[i]['value'] < attributes.data[j]['value']) {
                              this.cead[i]['className'] = 'valup';
                            } else {
                              this.cead[i]['className'] = 'valnom';
                            }
                          } else {
                            this.cead[i]['className'] = 'valnom';
                          }
                          break;
                        default:
                          if (this.cead[i]['value']) {
                            if (this.cead[i]['value'] === attributes.data[j]['value']) {
                              this.cead[i]['className'] = 'valnom';
                            } else {
                              this.cead[i]['className'] = 'valchange';
                            }
                          } else {
                            this.cead[i]['className'] = 'valnom';
                          }

                          break;
                      }
                      this.cead[i]['value'] = attributes.data[j]['value'];
                    }
                  }
                }

                for (var i = 0; i < this.cetd.length; i++) {
                  for (var j = 0; j < telemetries.data.length; j++) {
                    if (this.cetd[i]['keyName'] === telemetries.data[j].keyName) {
                      switch (typeof telemetries.data[j]['value']) {
                        case 'number':
                          if (this.cetd[i]['value']) {
                            if (this.cetd[i]['value'] > telemetries.data[j]['value']) {
                              this.cetd[i]['class'] = 'valdown';
                            } else if (this.cetd[i]['value'] < telemetries.data[j]['value']) {
                              this.cetd[i]['className'] = 'valup';
                            } else {
                              this.cetd[i]['className'] = 'valnom';
                            }
                          } else {
                            this.cetd[i]['className'] = 'valnom';
                          }
                          break;
                        default:
                          if (this.cetd[i]['value']) {
                            if (this.cetd[i]['value'] === telemetries.data[j]['value']) {
                              this.cetd[i]['className'] = 'valnom';
                            } else {
                              this.cetd[i]['className'] = 'valchange';
                            }
                          } else {
                            this.cetd[i]['className'] = 'valnom';
                          }

                          break;
                      }
                      this.cetd[i]['value'] = telemetries.data[j]['value'];
                    }
                  }
                }
                for (var i = 0; i < this.cetd.length; i++) {
                  for (var j = 0; j < telemetries.data.length; j++) {
                    if (this.cetd[i]['keyName'] === telemetries.data[j]['keyName']) {
                      this.cetd[i]['value'] = telemetries.data[j]['value'];
                    }
                  }
                }
              }
            );
          });
        } else {
          this.cead = [];
          this.cetd = [];
          if (this.obs) {
            this.obs.unsubscribe();
          }
        }
        break;
    }
  }

  removeattrs(attr, device) {
    this.http
      .delete<appmessage<Boolean>>('api/asset/removeAssetRaletions', {
        relationId: attr.id
      })
      .subscribe({
        next: next => {
          if (next?.data) {
            this.cead.splice(this.cead.indexOf(attr), 1);
          }
        },
        error: error => {},
        complete: () => {}
      });
  }

  removetemps(temp, device) {
    this.http
      .delete<appmessage<Boolean>>('api/asset/removeAssetRaletions', {
        relationId: temp.id
      })
      .subscribe({
        next: next => {
          if (next?.data) {
          }
        },
        error: error => {},
        complete: () => {}
      });
  }

  openComponent() {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = '导入资产';
    const drawerRef = this.drawerService.create<AssetentityformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: AssetentityformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: this.id
      }
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe(data => {
      if (typeof data === 'string') {
      }

      this.getData();
    });
  }

  editrelation(params) {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = '修改映射';
    const drawerRef = this.drawerService.create<AssetrelationformComponent, { params: any }, any>({
      nzTitle: title,
      nzContent: AssetrelationformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        params: params
      }
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe(data => {
      // data is edited assetralation

      this.getData();
    });
  }

  submit(i) {}
  updateEdit(i, edit) {
    this.st.setRow(i, { edit }, { refreshSchema: true });
  }

  reset() {
    this.q = {
      pi: 0,
      ps: 10,
      Name: '',
      sorter: '',
      status: null
    };
  }
  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }
}
