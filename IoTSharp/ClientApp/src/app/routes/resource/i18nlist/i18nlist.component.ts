import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STChange, STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ModalHelper, SettingsService, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { I18NService } from '@core';
import { I18nformComponent } from '../i18nform/i18nform.component';
@Component({
  selector: 'app-i18nlist',
  templateUrl: './i18nlist.component.html',
  styleUrls: ['./i18nlist.component.less'],
})
export class I18nlistComponent implements OnInit {
  url = 'api/i18n/index';
  total = 0;
  data: any[] = [];
  loading = false;
  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
  };
  q: {
    pi: number;
    ps: number;
    KeyName: string;
    sorter: string;
    Status: number | null;
  } = {
    pi: 0,
    ps: 10,
    KeyName: '',
    sorter: '',
    Status: null,
  };

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

  req: STReq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows',
    },
  };
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private i18n: I18NService,
    private settingService: SettingsService,
  ) {}
  @ViewChild('st', { static: true })
  st!: STComponent;
  columns: STColumn[] = [
    { title: '', index: 'id', type: 'checkbox' },
    { title: 'id', index: 'id' },
    { title: { i18n: 'i18n.columns.KeyName' }, index: 'keyName', fixed: 'left', width: 200 },
    // { title: { i18n: 'i18n.columns.ValueBG' }, index: 'valueBG' },
    // { title: { i18n: 'i18n.columns.ValueCS' }, index: 'valueCS' },
    // { title: { i18n: 'i18n.columns.ValueDA' }, index: 'valueDA' },
    // { title: { i18n: 'i18n.columns.ValueDEDE' }, index: 'valueDEDE' },
    // { title: { i18n: 'i18n.columns.ValueESES' }, index: 'valueESES' },
    { title: { i18n: 'i18n.columns.ValueENUS' }, index: 'valueENUS' },
    { title: { i18n: 'i18n.columns.ValueENGR' }, index: 'valueENGR' },
    // { title: { i18n: 'i18n.columns.ValueFI' }, index: 'valueFI' },
    { title: { i18n: 'i18n.columns.ValueFRFR' }, index: 'ValueFRFR' },
    // { title: { i18n: 'i18n.columns.ValueHE' }, index: 'valueHE' },
    // { title: { i18n: 'i18n.columns.ValueHRHR' }, index: 'valueHRHR' },
    // { title: { i18n: 'i18n.columns.ValueHU' }, index: 'valueHU' },
    // { title: { i18n: 'i18n.columns.ValueITIT' }, index: 'valueITIT' },
    // { title: { i18n: 'i18n.columns.ValueJAJP' }, index: 'valueJAJP' },
    // { title: { i18n: 'i18n.columns.ValueKOKR' }, index: 'valueKOKR' },
    // { title: { i18n: 'i18n.columns.ValueNL' }, index: 'valueNL' },
    // { title: { i18n: 'i18n.columns.ValuePLPL' }, index: 'valuePLPL' },
    // { title: { i18n: 'i18n.columns.ValuePT' }, index: 'valuePT' },
    // { title: { i18n: 'i18n.columns.ValueSLSL' }, index: 'valueSLSL' },
    // { title: { i18n: 'i18n.columns.ValueTRTR' }, index: 'valueTRTR' },
    // { title: { i18n: 'i18n.columns.ValueSR' }, index: 'valueSR' },
    // { title: { i18n: 'i18n.columns.ValueSV' }, index: 'valueSV' },
    // { title: { i18n: 'i18n.columns.ValueUK' }, index: 'valueUK' },
    // { title: { i18n: 'i18n.columns.ValueVI' }, index: 'valueVI' },
    { title: { i18n: 'i18n.columns.ValueZHCN' }, index: 'valueZHCN' },
    { title: { i18n: 'i18n.columns.ValueZHTW' }, index: 'valueZHTW' },

    {
      title: { i18n: 'i18n.columns.Status' },
      index: 'Status',
      render: 'Status',
      type: 'badge',
      badge: {
        0: { text: '禁用', color: 'error' },
        1: { text: '启用', color: 'success' },
      },
    },
    {
      title: { i18n: 'table.operation' },
      fixed: 'right',
      width: 180,
      buttons: [
        {
          text: '修改',
          acl: 64,
          i18n: 'common.edit',
          click: (item: any) => {
            this.openComponent(item.id);
          },
        },
        {
          text: (record) => (record.Status == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认资源状态?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: any) => {
            this.http.get('api/manage/i18n/setstatus?id=' + item.id).subscribe(
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
          i18n: 'common.delete',
          pop: {
            title: '确认删除资源?',
            okType: 'danger',
            icon: 'warning',
          },
          acl: 65,
          click: (item: any) => {
            this.http.get('api/manage/i18n/delete?id=' + item.id).subscribe(
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
  ngOnInit(): void {}

  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }
  openComponent(id: Number): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id == -1 ? this.i18n.fanyi('i18n.form.new.title') : this.i18n.fanyi('i18n.form.edit.title');
    const drawerRef = this.drawerService.create<I18nformComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: I18nformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id,
      },
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe((data) => {
      this.getData();
      if (typeof data === 'string') {
      }
    });
  }

  reset() {
    this.q = {
      pi: 0,
      ps: 10,
      KeyName: '',
      sorter: '',
      Status: null,
    };
  }
}
