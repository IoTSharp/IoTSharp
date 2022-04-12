import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STChange, STColumn, STColumnBadge, STColumnTag, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AssetformComponent } from '../assetform/assetform.component';

@Component({
  selector: 'app-assetlist',
  templateUrl: './assetlist.component.html',
  styleUrls: ['./assetlist.component.less']
})
export class AssetlistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService
  ) {}
  @ViewChild('expand')
  tpl: TemplateRef<any>;
  relations = [];

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true
  };
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

  url = 'api/asset/list';
  req: STReq = { method: 'Get', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows'
    }
  };

  @ViewChild('st', { static: true })
  st!: STComponent;

  TAG: STColumnTag = {
    1: { text: '成功', color: 'red' },
    2: { text: '进行中', color: 'blue' }
  };

 

  columns: STColumn[] = [
    { title: '', index: 'id', type: 'checkbox' },
    {
      title: '资产名称(数量)',
      index: 'name'
    },
    { title: '类型', index: 'assetType' },
    { title: '描述', index: 'description' },

    {
      title: { i18n: 'table.operation' },
      buttons: [
        {
          text: '资产管理',
         /* i18n: 'common.asset',*/
          acl: 60,
          click: (item: any) => {
            this._router.navigateByUrl('iot/device/assetentitylist?id='+item.id);
          }
        },
        {
          text: '修改',
          i18n: 'common.edit',
          acl: 60,
          click: (item: any) => {
            this.openComponent(item.id);
          }
        },

        {
          text: '删除',
          pop: {
            title: '确认删除字典组?',
            okType: 'danger',
            icon: 'warning'
          },
          acl: 61,
          click: (item: any) => {
            this.http.delete('api/asset/delete?id=' + item.id).subscribe(
              x => {
                this.getData();
              },
              y => {},
              () => {}
            );
          }
        }
      ]
    }
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;

  ngOnInit() {}
  submit(i){}
  updateEdit(i,edit){
    console.log(this.tpl);
    this.st.setRow(i, { edit }, { refreshSchema: true });
  }



  openComponent(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id == Guid.EMPTY ? '新增资产' : '修改资产';
    const drawerRef = this.drawerService.create<AssetformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: AssetformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id
      }
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe(data => {
      if (typeof data === 'string') {
      }

      this.getData();
    });
  }

  onchange($events: STChange) {

    console.log($events)
    switch ($events.type) {
      case 'expand':
        if ($events.expand.expand) {
          this.http.get('api/asset/relations?assetid=' + $events.expand.id).subscribe(
            next => {
              this.relations = next.data;
            },
            error => {},
            () => {}
          );
        }

        break;
    }
  }
  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }

  add(tpl: TemplateRef<{}>) {}

  reset() {
    setTimeout(() => {}, 1000);
  }
  setstatus(number: number, status: number) {}
}
