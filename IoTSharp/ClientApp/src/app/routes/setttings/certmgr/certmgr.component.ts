import { Component, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from '../../common/AppMessage';

@Component({
  selector: 'app-certmgr',
  templateUrl: './certmgr.component.html',
  styleUrls: ['./certmgr.component.less'],
})
export class CertmgrComponent implements OnInit {

  Domain:string='';
  installed = false;
  constructor(private http: _HttpClient, private msg: NzMessageService) {}

  ngOnInit(): void {
    this.checkcert();
  }

  checkcert() {
    this.http.get('api/Installer/Instance').subscribe(
      (next) => {
        if (next.data.caCertificate) {
          this.installed = true;
        } else {
          this.installed = false;
          this.Domain=next.data.domain
        }
      },
      (error) => {},
      () => {},
    );
  }

  createcert($event) {
    this.http.post('api/Installer/CreateRootCertificate',{Domain:this.Domain}).subscribe(
      (next) => {
        if (next.code === 10000) {
          this.msg.create('success', '证书生成成功');
        } else {
          this.msg.create('error', '证书生成失败' + next.msg);
        }
      },
      (error) => {
        this.msg.create('error', '证书生成失败');
      },
      () => {},
    );
  }
}
