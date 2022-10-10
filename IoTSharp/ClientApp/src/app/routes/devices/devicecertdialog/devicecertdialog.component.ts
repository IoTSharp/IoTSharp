import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { ClipboardService } from 'ngx-clipboard';
import { appmessage } from 'src/app/models/appmessage';
@Component({
  selector: 'app-devicecertdialog',
  templateUrl: './devicecertdialog.component.html',
  styleUrls: ['./devicecertdialog.component.less']
})
export class DevicecertdialogComponent implements OnInit {

  ngOnInit(): void {}

  @Input() record: NzSafeAny;
e
  constructor(private modal: NzModalRef,  
     private http: _HttpClient,
    public msg: NzMessageService,) {}

  gen(): void {
    this.http
    .get<appmessage<any>>(
      'api/Devices/' + this.record.id + '/createX509Identity'
    )
    .subscribe(
      {
        next: next => {
          if(next.code===10000){
            this.msg.create('success', '证书生成成功');
          }else{
            this.msg.create('error', '证证书生成失败:'+next.msg);
          }
        },
        error: error => {
          console.log(error)
          this.msg.create('error', '证书生成失败:'+error.error);
        }, complete: () => { }
      }
    );
  }




   download() {
    this.http
      .get(
        'api/Devices/' + this.record.id + '/DownloadCertificates',
        {},{ responseType:'blob'}
      )
      .subscribe(
        {
          next: next => {
            let url = window.URL.createObjectURL(next);
            let a = document.createElement('a');
            document.body.appendChild(a);
            a.setAttribute('style', 'display: none');
            a.href = url;
            a.download = this.record.id + '.zip';
            a.click();
            window.URL.revokeObjectURL(url);
            a.remove();

          },
          error: error => {

            this.msg.create('error', '证书下载失败:' + error.error);
            console.log(error);
          }, complete: () => { }
        }
      );
  }

  cancel(): void {
    this.modal.destroy();
  }
}
