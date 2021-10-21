import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumnTag, STColumn, STData } from '@delon/abc/st';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';

import { AppMessage } from '../../common/AppMessage';
import { DictionarygroupformComponent } from '../dictionarygroupform/dictionarygroupform.component';

@Component({
  selector: 'app-dictionarygrouplist',
  templateUrl: './dictionarygrouplist.component.html',
  styleUrls: ['./dictionarygrouplist.component.less'],
})
export class DictionarygrouplistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,
  ) {}

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
  };
  q: {
    pi: number;
    ps: number;
    DictionaryGroupName: string;
    sorter: string;
    status: number | null;
  } = {
    pi: 0,
    ps: 10,
    DictionaryGroupName: '',
    sorter: '',
    status: null,
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
      checked: false,
    },
    {
      index: -1,
      text: '已删除',
      value: false,
      type: 'processing',
      checked: false,
    },
  ];

  url = 'api/dictionarygroup/index';
  req: STReq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows',
    },
  };

  @ViewChild('st', { static: true })
  st!: STComponent;

  TAG: STColumnTag = {
    1: { text: '成功', color: 'red' },
    2: { text: '进行中', color: 'blue' },
  };

  columnsChildren: STColumn[] = [
    { title: 'id', index: 'dictionaryId' },
    { title: '字典名', index: 'dictionaryName' },
    { title: '字典值', index: 'dictionaryValue' },
    { title: '备注', index: 'dictionaryDesc' },
    { title: '类型', index: 'dictionaryValueTypeName' },
  ];

  columns: STColumn[] = [
    { title: '', index: 'dictionaryGroupId', type: 'checkbox' },
    { title: 'id', index: 'dictionaryGroupId' },
    {
      title: '字典分组名称(数量)',
      index: 'dictionaryGroupName',
    },
    { title: '地址', index: 'dictionaryGroupValueTypeName' },
    { title: '描述', index: 'dictionaryGroupDesc' },

    {
      title: '状态',
      index: 'dictionaryGroupStatus',
      render: 'dictionaryGroupStatus',
      type: 'badge',
      badge: {
        0: { text: '禁用', color: 'error' },
        1: { text: '启用', color: 'success' },
      },
    },
    {
      title: { i18n: 'table.operation' },
      buttons: [
        {
          text: '修改',
          i18n: 'common.edit',
          acl: 60,
          click: (item: any) => {
            this.openComponent(item.dictionaryGroupId);
            //this._router.navigate(['manage/role/roleform'],
            //  {
            //    queryParams: {
            //      UserId: item.UserId,
            //      type: 'clone'
            //    }
            //  });
          },
        },
        {
          text: (record) => (record.dictionaryGroupStatus == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认修改字典组状态?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: any) => {
            this.http.get('api/dictionarygroup/setstatus?id=' + item.DictionaryGroupId).subscribe(
              (x) => {
                this.getData();
              },
              (y) => {},
              () => {},
            );
          },
        },

        {
          text: '删除',
          pop: {
            title: '确认删除字典组?',
            okType: 'danger',
            icon: 'warning',
          },
          acl: 61,
          click: (item: any) => {
            this.http.get('api/dictionarygroup/delete?id=' + item.DdictionaryGroupId).subscribe(
              (x) => {
                this.getData();
              },
              (y) => {},
              () => {},
            );
          },
        },
      ],
    },
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;

  ngOnInit() {}

  rowchange(event) {
    console.log(event);
    switch (event.type) {
      case 'expand':
        this.http.post<AppMessage>('api/dictionary/index' ,{
          DictionaryGroupId:event.expand.dictionaryGroupId,
          offset:0,
          limit:100
        }).subscribe(
          (x) => {
            event.expand.Children = x.data.rows;
          },
          (y) => {},
          () => {},
        );
        break;
    }
  }
  openComponent(id: Number): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id == -1 ? '新增字典分组' : '修改字典分组';
    const drawerRef = this.drawerService.create<DictionarygroupformComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DictionarygroupformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id,
      },
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe((data) => {
      if (typeof data === 'string') {
      }

      this.getData();
    });
  }

  r;
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
