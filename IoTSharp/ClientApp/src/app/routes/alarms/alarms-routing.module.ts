import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AlarmlistComponent } from './alarmlist/alarmlist.component';

const routes: Routes = [{ path: 'alarmlist', component: AlarmlistComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AlarmsRoutingModule {}
