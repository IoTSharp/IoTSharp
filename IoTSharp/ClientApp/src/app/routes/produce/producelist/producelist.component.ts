import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumn, STData } from '@delon/abc/st';
import { ModalHelper, SettingsService, _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';
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

  url = 'api/produce/list';
  req: STReq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows'
    }
  };

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

  r;
  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }

  delete(id: number) {
    this.http.get('api/produce/delete?id=' + id).subscribe(() => {
      this.st.load(this.st.pi);
    });
  }

  setstatus(id: number) {
    this.http.get('api/produce/setstatus?id=' + id).subscribe(() => {
      this.st.load(this.st.pi);
    });
  }

  add(tpl: TemplateRef<{}>) { }

  reset() {
    setTimeout(() => { }, 1000);
  }
}
