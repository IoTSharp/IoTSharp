import { ChangeDetectorRef, Injectable } from '@angular/core';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { WidgetdeviceComponent } from '../widgets/widgetdevice/widgetdevice.component';
@Injectable()
export class CommonDialogSevice {
  constructor(private settingService: SettingsService, private drawerService: NzDrawerService) {}
  showDeviceDialog(deviceId: string) {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    let title = '设备详情';
    const drawerRef = this.drawerService.create<
      WidgetdeviceComponent,
      {
        id: string;
      },
      string
    >({
      nzTitle: title,
      
      nzContent: WidgetdeviceComponent,
      nzWidth: window.innerWidth * 0.8,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: deviceId
      }
    });

    drawerRef.nzBodyStyle={ padding:'10px' };
    drawerRef.afterOpen.subscribe(() => {});
    drawerRef.afterClose.subscribe(() => {});
  }

  //eg
  // showTalentDialog(TalentId:string){

  // }
}
