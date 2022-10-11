import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-certmgr',
  templateUrl: './certmgr.component.html',
  styleUrls: ['./certmgr.component.less']
})
export class CertmgrComponent implements OnInit {
  form!: FormGroup;
  installed = true;
  caCertificate=false;
  constructor(private http: _HttpClient, private msg: NzMessageService,  private fb: FormBuilder,) { }

  ngOnInit(): void {
    this.checkcert();
  }

  checkcert() {
    this.form = this.fb.group({
      domain: ['', []],
      caThumbprint: ['', []],
      brokerThumbprint: ['', []],
    });
    this.http.get<appmessage<any>>('api/Installer/Instance').subscribe(
      {
        next: next => {
          if (next.data.installed) {
            this.installed = true;
            this.caCertificate=next.data.caCertificate;
            this.form.patchValue(next.data);
          } else {
            this.installed = false;
            this.caCertificate=next.data.caCertificate;
            this.form.patchValue(next.data);
          }
        },
        error: error => { },
        complete: () => { }
      }
    );
  }

  createcert($event) {
    this.http.post<appmessage<any>>('api/Installer/CreateRootCertificate', {  }).subscribe(
      {
        next: next => {
          if (next.code === 10000) {
            this.msg.create('success', '证书生成成功');
          } else {
            this.msg.create('error', '证书生成失败' + next.msg);
          }
        },
        error: error => {
          this.msg.create('error', '证书生成失败');
        },
        complete: () => { }
      }
    );
  }
}
