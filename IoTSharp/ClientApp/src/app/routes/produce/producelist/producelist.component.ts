import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumn, STData, STChange, STColumnTag, STColumnBadge } from '@delon/abc/st';
import { ModalHelper, SettingsService, _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';
import { CreatedeviceformComponent } from '../createdeviceform/createdeviceform.component';
import { ProducedatadictionaryformComponent } from '../producedatadictionaryform/producedatadictionaryform.component';
import { ProducedataformComponent } from '../producedataform/producedataform.component';
import { ProduceformComponent } from '../produceform/produceform.component';

@Component({
  selector: 'app-producelist',
  templateUrl: './producelist.component.html',
  styleUrls: ['./producelist.component.less']
})
export class ProducelistComponent implements OnInit {

  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService
  ) { }

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true
  };
  q: {
    pi: number;
    ps: number;
    name: string;

    sorter: string;
    status: number | null;
  } = {
      pi: 0,
      ps: 20,
      name: '',
      sorter: '',
      status: null
    };

  total = 0;
  data: any[] = [];
  loading = false;
  status = [
    { index: 1, text: '正常', value: false, type: 'default', checked: false },
    {
      index: 2,
      text: '已禁用',
      value: false,
      type: 'processing',
      checked: false
    },
    {
      index: -1,
      text: '已删除',
      value: false,
      type: 'processing',
      checked: false
    }
  ];

  url = 'api/produces/list';
  req: STReq = { method: 'GET', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows'
    }
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
  devicecolumns:STColumn[] = [
    { title: 'id', index: 'id',  },
    {title: '名称',
    index: 'name',
    render: 'name',
    type: 'link',
  },
  { title: '设备类型', index: 'deviceType', type: 'tag', tag: this.DeviceTAG },
  { title: '在线状态', index: 'active', type: 'badge', badge: this.BADGE, sort: true },
  { title: '最后活动时间', index: 'lastActivityDateTime', type: 'date' },
  { title: '认证方式', index: 'identityType', type: 'tag', tag: this.TAG},
  ]

  @ViewChild('st', { static: true })
  st!: STComponent;
  columns: STColumn[] = [
    { title: '', index: 'id', type: 'checkbox' },
    { title: 'id', index: 'id' },
    { title: '产品名称', index: 'name' },
    { title: '超时', index: 'defaultIdentityType' },
    { title: '备注', index: 'description' },

    {
      title: '状态',
      index: 'dictionaryStatus',
      type: 'badge',
      badge: {
        0: { text: '禁用', color: 'error' },
        1: { text: '启用', color: 'success' }
      }
    },
    {
      title: { i18n: 'table.operation' },
      buttons: [
        {
          text: '修改',
          i18n: 'common.edit',
          acl: 56,
          click: (item: any) => {
            this.openComponent(item.id);
          }
        },
        {
          text: '属性',
          //   i18n: 'common.edit',
          acl: 56,
          click: (item: any) => {
            this.editattr(item.id);
          }
        }, {
          text: '字典',
          //   i18n: 'common.edit',
          acl: 56,
          click: (item: any) => {
            this.editdic(item.id);
          }
        },
        {
          text: '创建设备',
          acl: 56,
          click: (item: any) => {
            this.createdevice(item.id);
          }
        },
        {
          text: '删除',
          pop: {
            title: '确认修改产品项状态?',
            okType: 'danger',
            icon: 'warning'
          },
          acl: 57,
          click: (item: any) => {
            this.delete(item.id);
          }
        }
      ]
    }
  ];
  selectedRows: STData[] = [];

  ngOnInit() {

  }


  createdevice( id:string){

    this.modal.create(CreatedeviceformComponent, { id }).subscribe(res => {

      if(res&&res.code===10000){
        this.msg.success('保存成功')
      }else{
        this.msg.warning('保存失败:'+res.msg)
      }
   

    });


  }

  openComponent(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id == Guid.EMPTY ? '新增产品' : '修改产品';
    const drawerRef = this.drawerService.create<ProduceformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: ProduceformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id
      }
    });

    drawerRef.afterOpen.subscribe(() => { });

    drawerRef.afterClose.subscribe(data => {
      if (typeof data === 'string') {
      }
      this.getData();
    });
  }



  editattr(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = '属性编辑';
    const drawerRef = this.drawerService.create<ProducedataformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: ProducedataformComponent,
      nzWidth: '80%',
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id
      }
    });

    drawerRef.afterOpen.subscribe(() => { });

    drawerRef.afterClose.subscribe(data => {
      if (typeof data === 'string') {
      }

      this.getData();
    });
  }
  onchange($events: STChange): void {
    switch ($events.type) {
      case 'expand':
        if ($events.expand.expand) {



        }
        break;

    }

  }




  getdevices() {

    this.http.post('', {}).subscribe({
      next: next => {

      }, error: error => {

      }, complete: () => {

      }

    })




  }


  editdic(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = '字典编辑';
    const drawerRef = this.drawerService.create<ProducedatadictionaryformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: ProducedatadictionaryformComponent,
      nzWidth: "100%",
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id
      }
    });

    drawerRef.afterOpen.subscribe(() => { });

    drawerRef.afterClose.subscribe(data => {
      if (typeof data === 'string') {
      }

      this.getData();
    });
  }

  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }

  delete(id: number) {
    this.http.get('api/produces/delete?id=' + id).subscribe(() => {
      this.st.load(this.st.pi);
    });
  }


  add(tpl: TemplateRef<{}>) { }

  reset() {
    setTimeout(() => { }, 1000);
  }
}
