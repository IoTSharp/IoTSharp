import { Component, Input, OnInit } from '@angular/core';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { NzModalRef } from 'ng-zorro-antd/modal';
import { ClipboardService } from 'ngx-clipboard';

@Component({
  selector: 'app-devicetokendialog',
  templateUrl: './devicetokendialog.component.html',
  styleUrls: ['./devicetokendialog.component.less']
})
export class DevicetokendialogComponent implements OnInit {



  ngOnInit(): void {
  }

  @Input() record: NzSafeAny;

  constructor(private modal: NzModalRef , private cliboardService :ClipboardService,) {}

  ok(): void {

    this.cliboardService.copyFromContent(this.record.identityId);
    this.modal.destroy(`Token已复制`);
  }

  cancel(): void {
    this.modal.destroy();
  }

}
