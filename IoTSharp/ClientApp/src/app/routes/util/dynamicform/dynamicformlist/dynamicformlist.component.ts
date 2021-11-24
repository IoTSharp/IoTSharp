import { Component, OnInit, ViewChild } from '@angular/core';
import { STPage, STReq, STRes, STComponent, STColumn, STData } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { ConditionbuilderComponent } from '../conditionbuilder/conditionbuilder.component';
import { DynamicformdesignerComponent } from '../dynamicformdesigner/dynamicformdesigner.component';
import { Dynamicformdesignerv2Component } from '../dynamicformdesignerv2/dynamicformdesignerv2.component';

import { DynamicformeditorComponent } from '../dynamicformeditor/dynamicformeditor.component';
import { DynamicformfieldeditorComponent } from '../dynamicformfieldeditor/dynamicformfieldeditor.component';
import { DynamicformtesterComponent } from '../dynamicformtester/dynamicformtester.component';
import { SearchformgeneratorComponent } from '../searchformgenerator/searchformgenerator.component';

@Component({
  selector: 'app-dynamicformlist',
  templateUrl: './dynamicformlist.component.html',
  styleUrls: ['./dynamicformlist.component.less'],
})
export class DynamicformlistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
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
      total: 'data.total',
      list: 'data.rows',
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
        //  i18n: 'dynamicform.fieldedit',
          click: (item: any) => {
            this.openFieldComponent(item.formId);
          },
        },
    
        {
          text: '预览',
          acl: 103,
       //   i18n: 'dynamicform.formpriview',
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
              () => {
                this.getData();
              },
              () => {},
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
              () => {
                this.getData();
              },
              () => {},
              () => {},
            );
          },



        }, {
          text: '设计',
          acl: 103,
          //    i18n: 'dynamicform.fieldedit',
          click: (item: any) => {
            this.openDesignerComponent(item.formId);
          },
        },


        {
          text: '设计2',
          acl: 103,
          //    i18n: 'dynamicform.fieldedit',
          click: (item: any) => {
            this.openDesignerComponentV2(item.formId);
          },
        },


        {
          text: 'SearchForm',
          acl: 103,
          //    i18n: 'dynamicform.fieldedit',
          click: (item: any) => {
            this.openSearchFormComponent(item.formId);
          },
        },


        {
          text: 'ConditionBuilder',
          acl: 103,
          //    i18n: 'dynamicform.fieldedit',
          click: (item: any) => {
            this.openConditionBuilderComponent(item.formId);
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
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<DynamicformeditorComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DynamicformeditorComponent,
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

  openFieldComponent(id: Number): void {
    var title = id == -1 ? '新增表单' : '修改表单';
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<DynamicformfieldeditorComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DynamicformfieldeditorComponent,
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


  openDesignerComponent(id: Number): void {
    var title = id == -1 ? '设计' : '设计';
    var { nzMaskClosable } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<DynamicformdesignerComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DynamicformdesignerComponent,
       nzWidth: 1024,
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




  openDesignerComponentV2(id: Number): void {
    var title = id == -1 ? '设计' : '设计';
    var { nzMaskClosable } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<Dynamicformdesignerv2Component, { id: Number }, string>({
      nzTitle: title,
      nzContent: Dynamicformdesignerv2Component,
      nzWidth: 1024,
      nzMaskClosable: nzMaskClosable,

      nzContentParams: {
        id: id,
      },
    });

    drawerRef.afterOpen.subscribe(() => { });

    drawerRef.afterClose.subscribe((data) => {
      if (typeof data === 'string') {
      }

      this.getData();
    });
  }
  openFormComponent(id: Number): void {
    var title = '预览';
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<DynamicformtesterComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: DynamicformtesterComponent,
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


  openSearchFormComponent(id: Number): void {
    var title = '预览';
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<SearchformgeneratorComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: SearchformgeneratorComponent,
      nzWidth: 1024,
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


  openConditionBuilderComponent(id: Number): void {
    var title = '预览';
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<ConditionbuilderComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: ConditionbuilderComponent,
      nzWidth: 1024,
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

  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }

  add() {}

  reset() {
    setTimeout(() => {}, 1000);
  }
  setstatus() {}
}
