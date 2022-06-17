import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DevicegraphComponent } from './devicegraph/devicegraph.component';
import { DevicelistComponent } from './devicelist/devicelist.component';
import { DevicemodellistComponent } from './devicemodellist/devicemodellist.component';
import { DevicesceneComponent } from './devicescene/devicescene.component';

const routes: Routes = [
  { path: 'devicelist', component: DevicelistComponent },
  { path: 'devicemodellist', component: DevicemodellistComponent },
  { path: 'devicegraph', component: DevicegraphComponent },
  { path: 'devicescene', component: DevicesceneComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DevicesRoutingModule {}
