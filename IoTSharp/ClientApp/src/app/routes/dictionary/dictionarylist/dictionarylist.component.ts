import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumn, STData } from '@delon/abc/st';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';

import { DictionaryformComponent } from '../dictionaryform/dictionaryform.component';

@Component({
  selector: 'app-dictionarylist',
  templateUrl: './dictionarylist.component.html',
  styleUrls: ['./dictionarylist.component.less'],
})
export class DictionarylistComponent implements OnInit {
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
    DictionaryName: string;
    DictionaryGroupId: number;
    sorter: string;
    status: number | null;
  } = {
    pi: 0,
    ps: 20,
    DictionaryName: '',
    DictionaryGroupId: 0,
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

  url = 'api/dictionary/index';
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
  columns: STColumn[] = [
    { title: '', index: 'dictionaryId', type: 'checkbox' },
    { title: 'id', index: 'dictionaryId' },
    { title: '字典名称', index: 'dictionaryName' },
    { title: '字典值', index: 'dictionaryValue' },
    { title: '类型', index: 'dictionaryValueTypeName' },
    { title: '备注', index: 'dictionaryDesc' },
    { title: 'I18NKey', index: 'dictionary18NKeyName' },

    {
      title: '状态',
      index: 'dictionaryStatus',
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
          acl: 56,
          click: (item: any) => {
            this.openComponent(item.dictionaryId);
          },
        },
        {
          text: (record) => (record.dictionaryStatus == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认修改字典项状态?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: any) => {
            this.http.get('api/dictionary/setstatus?id=' + item.dictionaryId).subscribe(
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
            title: '确认修改字典项状态?',
            okType: 'danger',
            icon: 'warning',
          },
          acl: 57,
          click: (item: any) => {
            this.delete(item.DictionaryId);
          },
        },
      ],
    },
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;
  optionList: any = [];
  ngOnInit() {
    this.http.post('api/dictionarygroup/index', { limit: 20, offset: 0, pi: 0, ps: 20 }).subscribe(
      (x) => {
        this.optionList = x.data.rows;
      },
      (y) => {},
      () => {},
    );
  }
  openComponent(id: Number): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id == -1 ? '新增字典' : '修改字典';
    const drawerRef = this.drawerService.create<DictionaryformComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DictionaryformComponent,
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
    this.st.load(1);
  }

  delete(id: number) {
    this.http.get('api/dictionary/delete?id=' + id).subscribe(() => {
      this.st.load(this.st.pi);
    });
  }

  setstatus(id: number) {
    this.http.get('api/dictionary/setstatus?id=' + id).subscribe(() => {
      this.st.load(this.st.pi);
    });
  }

  add(tpl: TemplateRef<{}>) {}

  reset() {
    setTimeout(() => {}, 1000);
  }
}
