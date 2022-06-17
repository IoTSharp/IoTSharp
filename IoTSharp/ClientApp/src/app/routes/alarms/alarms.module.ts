import { NgModule } from '@angular/core';
import { SharedModule } from '@shared';
import { CommonDialogSevice } from '../util/commonDialogSevice';
import { AlarmlistComponent } from './alarmlist/alarmlist.component';
import { AlarmsRoutingModule } from './alarms-routing.module';

const COMPONENTS = [AlarmlistComponent];

@NgModule({
  imports: [AlarmsRoutingModule, SharedModule],
  providers: [CommonDialogSevice],
  declarations: COMPONENTS
})
export class AlarmsModule {}
