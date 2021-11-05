import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { TaskexecutorformComponent } from '../taskexecutorform/taskexecutorform.component';


@Component({
  selector: 'app-taskexecutorlist',
  templateUrl: './taskexecutorlist.component.html',
  styleUrls: ['./taskexecutorlist.component.less']
})
export class TaskexecutorlistComponent implements OnInit {

  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,
    
  ) {

  }

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
  };
  q: {
    pi: number;
    ps: number;
    Name: string;
    Creator: string;
    CreatTime: Date[];
    sorter: string;
    status: number | null;
  } = {
    pi: 0,
    ps: 10,
    Name: '',
    Creator: '',
    CreatTime: [],
    sorter: '',
    status: null,
  };
  total = 0;

  loading = false;

  url = 'api/rules/executors';
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
    { title: '', index: 'executorId', type: 'checkbox' },
    { title: '规则名称', index: 'executorName', render: 'executorName' },
    { title: '备注', index: 'executorDesc' },
    { title: '创建时间', type: 'date', index: 'addDateTime' },
    { title: '测试状态', type: 'date', index: 'addDateTime' },
    {
      title: { i18n: 'i18n.columns.Status' },
      index: 'executorStatus',
      render: 'executorStatus',
      type: 'badge',
      badge: {
        0: { text: '禁用', color: 'error' },
        1: { text: '启用', color: 'success' },
      },
    },

    {
      title: '操作',
      buttons: [
        {
          acl: 9,
          text: '修改',
          click: (item: taskexecutor) => {
            this.openComponent(item.executorId);
          },
        },
        {
          text: (record) => (record.rulestatus == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认修改规则状态?',
            okType: 'danger',
            icon: 'warning',
          },
          click: () => {


            //do something

          },
        },

        {
          text: '删除',
          //    acl: 104,
          pop: {
            title: '确认删除规则?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: taskexecutor) => {
            this.http.get('api/rules/deleteexecutor?id=' + item.executorId).subscribe(
              () => {

                this.msg.create('success', '执行器删除成功');
                this.getData();
              },
              () => {},
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
  openComponent(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id === Guid.EMPTY ? '新建规则' : '修改规则';
    const drawerRef = this.drawerService.create<TaskexecutorformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: TaskexecutorformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id,
      },
    });
  
    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe((data) => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }


  


  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }
  reset() {
    this.q ==
      {
        pi: 0,
        ps: 10,
        Name: '',
        Creator: '',
        CreatTime: [],
        sorter: '',
        status: null,
      };
  }
  setstatus() {}

}


interface taskexecutor{
  executorId:string;
  executorDesc:string;
  executorName:string;
  defaultConfig:string;
  typeName:string;
  path:string;
  addDateTime:string;
  creator:string;
  mataData:string;
    executorStatus:number;
  tag:string;
}