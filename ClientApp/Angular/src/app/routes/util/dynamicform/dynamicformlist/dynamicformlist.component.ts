import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumn, STData } from '@delon/abc/st';
import { _HttpClient, ModalHelper } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Globals } from 'src/app/core/Globals';
import { DynamicformeditorComponent } from '../dynamicformeditor/dynamicformeditor.component';
import { DynamicformfieldeditorComponent } from '../dynamicformfieldeditor/dynamicformfieldeditor.component';
import { DynamicformtesterComponent } from '../dynamicformtester/dynamicformtester.component';

@Component({
  selector: 'app-dynamicformlist',
  templateUrl: './dynamicformlist.component.html',
  styleUrls: ['./dynamicformlist.component.less'],
})
export class DynamicformlistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private globals: Globals,
  ) {}

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
  };
  q: {
    pi: number;
    ps: number;
    RouteName: string;
    sorter: string;
    status: number | null;
  } = {
    pi: 1,
    ps: 10,
    RouteName: '',
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

  url = 'api/dynamicforminfo/index';
  req: STReq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'result.total',
      list: 'result.rows',
    },
  };

  @ViewChild('st', { static: true })
  st!: STComponent;
  columns: STColumn[] = [
    { title: '', index: 'formId', type: 'checkbox' },
    { title: 'id', index: 'formId' },
    { title: '表单名称', index: 'formName' },
    { title: '备注', index: 'formDesc' },
    { title: '创建时间', index: 'fromCreateDate' },
    {
      title: { i18n: 'i18n.columns.Status' },
      index: 'formStatus',
      render: 'formStatus',
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
          acl: 103,
          i18n: 'common.edit',
          click: (item: any) => {
            this.openComponent(item.formId);
          },
        },
        {
          text: '字段编辑',
          acl: 103,
          i18n: 'dynamicform.fieldedit',
          click: (item: any) => {
            this.openFieldComponent(item.formId);
          },
        },

        {
          text: '预览',
          acl: 103,
          i18n: 'dynamicform.formpriview',
          click: (item: any) => {
            this.openFormComponent(item.formId);
          },
        },
        {
          text: (item) => (item.RouteStatus == 1 ? '禁用' : '启用'),
          acl: 104,
          pop: {
            title: '确认设置表单?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: any) => {
            this.http.get('api/dynamicforminfo/setstatus?id=' + item.formId).subscribe(
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
          acl: 104,
          pop: {
            title: '确认删除表单?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: any) => {
            this.http.get('api/dynamicforminfo/delete?id=' + item.formId).subscribe(
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
  openComponent(id: Number): void {
    var title = id == -1 ? '新增表单' : '修改表单';
    const drawerRef = this.drawerService.create<DynamicformeditorComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DynamicformeditorComponent,
      nzWidth: this.globals.drawerwidth,
      nzMaskClosable: this.globals.nzMaskClosable,

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

  openFieldComponent(id: Number): void {
    var title = id == -1 ? '新增表单' : '修改表单';
    const drawerRef = this.drawerService.create<DynamicformfieldeditorComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DynamicformfieldeditorComponent,
      nzWidth: this.globals.drawerwidth,
      nzMaskClosable: this.globals.nzMaskClosable,

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

  openFormComponent(id: Number): void {
    var title = '预览';
    const drawerRef = this.drawerService.create<DynamicformtesterComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DynamicformtesterComponent,
      nzWidth: this.globals.drawerwidth,
      nzMaskClosable: this.globals.nzMaskClosable,

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
